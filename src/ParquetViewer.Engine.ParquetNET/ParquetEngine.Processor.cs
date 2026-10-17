using Parquet;
using Parquet.Schema;
using ParquetViewer.Engine.Exceptions;
using ParquetViewer.Engine.Types;
using System.Collections;
using System.Data;

namespace ParquetViewer.Engine.ParquetNET;

public partial class ParquetEngine
{
    public async Task<Func<bool, DataTable>> ReadRowsAsync(List<string> selectedFields, int offset, int recordCount, IProgress<int>? progress = null, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(recordCount, nameof(recordCount));
        ArgumentOutOfRangeException.ThrowIfNegative(offset, nameof(offset));

        long recordsLeftToRead = recordCount;
        var result = BuildDataTableLite(null, selectedFields, Math.Min(recordCount, (int)RecordCount));

        foreach (var reader in GetReaders(offset))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (recordsLeftToRead <= 0)
                break;

            recordsLeftToRead = await PopulateDataTableAsync(result, reader.ParquetReader, reader.RemainingOffset, recordsLeftToRead, cancellationToken, progress);
        }

        result.DataSetSize = RecordCount;

        return (logProgress) =>
        {
            var datatable = result.ToDataTable(cancellationToken, logProgress ? progress : null);
            return datatable;
        };
    }

    private async Task<long> PopulateDataTableAsync(DataTableLite dataTable, ParquetReader parquetReader,
        long offset, long recordCount, CancellationToken cancellationToken, IProgress<int>? progress)
    {
        //Read column by column to generate each row in the datatable
        int totalRecordCountSoFar = 0;
        long rowsLeftToRead = recordCount;
        for (int i = 0; i < parquetReader.RowGroupCount; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using var groupReader = parquetReader.OpenRowGroupReader(i);
            if (groupReader.RowCount > int.MaxValue)
                throw new ArgumentOutOfRangeException(
                    string.Format("Cannot handle row group sizes greater than {0}. Found {1} instead.", int.MaxValue, groupReader.RowCount));

            int rowsPassedUntilThisRowGroup = totalRecordCountSoFar;
            totalRecordCountSoFar += (int)groupReader.RowCount;

            if (offset >= totalRecordCountSoFar)
                continue;

            if (rowsLeftToRead <= 0)
                break;

            long numberOfRecordsToReadFromThisRowGroup = Math.Min(Math.Min(totalRecordCountSoFar - offset, rowsLeftToRead), groupReader.RowCount);
            rowsLeftToRead -= numberOfRecordsToReadFromThisRowGroup;

            long recordsToSkipInThisRowGroup = Math.Max(offset - rowsPassedUntilThisRowGroup, 0);

            await ProcessRowGroupAsync(dataTable, groupReader, recordsToSkipInThisRowGroup, numberOfRecordsToReadFromThisRowGroup, cancellationToken, progress);
        }

        return rowsLeftToRead;
    }

    private async Task ProcessRowGroupAsync(DataTableLite dataTable, ParquetRowGroupReader groupReader,
        long skipRecords, long readRecords, CancellationToken cancellationToken, IProgress<int>? progress)
    {
        int rowBeginIndex = dataTable.Rows.Count;
        bool isFirstColumn = true;

        foreach (var columnLite in dataTable.Columns.Values)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var field = (ParquetSchemaElement)columnLite.ParentSchema.GetChild(columnLite.Name);
            switch (field.FieldType)
            {
                case FieldTypeId.Primitive:
                    await ReadPrimitiveFieldAsync(dataTable, groupReader, rowBeginIndex, field, skipRecords,
                        readRecords, isFirstColumn, cancellationToken, progress);
                    break;
                case FieldTypeId.List:
                    var listField = field.GetListField();
                    var itemField = listField.GetListItemField();
                    var fieldIndex = dataTable.Columns[field.Path]!.Ordinal;
                    await ReadListFieldAsync(dataTable, groupReader, rowBeginIndex, itemField, fieldIndex,
                        skipRecords, readRecords, isFirstColumn, cancellationToken, progress);
                    break;
                case FieldTypeId.Map:
                    await ReadMapFieldAsync(dataTable, groupReader, rowBeginIndex, field, skipRecords,
                        readRecords, isFirstColumn, cancellationToken, progress);
                    break;
                case FieldTypeId.Struct:
                    await ReadStructFieldAsync(dataTable, groupReader, rowBeginIndex, field, skipRecords,
                        readRecords, isFirstColumn, cancellationToken, progress);
                    break;
                default:
                    throw new InvalidDataException($"Field `{columnLite.Name}` has an unknown type {field.FieldType}.");
            }

            isFirstColumn = false;
        }
    }

    private async Task ReadPrimitiveFieldAsync(DataTableLite dataTable, ParquetRowGroupReader groupReader, int rowBeginIndex, ParquetSchemaElement field,
        long skipRecords, long readRecords, bool isFirstColumn, CancellationToken cancellationToken, IProgress<int>? progress)
    {
        var rowIndex = rowBeginIndex;
        var skippedRecords = 0;
        var fieldIndex = dataTable.Columns[field.Path]?.Ordinal ?? throw new ParquetEngineException($"Column `{field.Path}` is missing");

        if (field.BelongsToListField || field.BelongsToListOfStructsField || field.DataField?.IsArray == true)
        {
            await ReadListFieldAsync(dataTable, groupReader, rowBeginIndex, field, fieldIndex, skipRecords, readRecords, isFirstColumn, cancellationToken, progress);
        }
        else
        {
            var dataColumn = await ReadColumnAsync(groupReader, field, cancellationToken);
            var dataEnumerable = dataColumn.Data;

            var fieldType = dataTable.Columns[field.Path].Type;
            var progressUntilReport = 0;
            const int progressReportThreshold = 10;
            foreach (object value in dataEnumerable)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (skipRecords > skippedRecords)
                {
                    skippedRecords++;
                    continue;
                }

                if (rowIndex - rowBeginIndex >= readRecords)
                    break;

                if (isFirstColumn)
                {
                    dataTable.NewRow();
                }

                if (value == DBNull.Value || value is null)
                {
                    dataTable.Rows[rowIndex]![fieldIndex] = DBNull.Value;
                }
                else if (fieldType == typeof(ByteArrayValue))
                {
                    dataTable.Rows[rowIndex]![fieldIndex] = new ByteArrayValue((byte[])value);
                }
                else
                {
                    dataTable.Rows[rowIndex]![fieldIndex] = value;
                }

                rowIndex++;
                progressUntilReport++;

                //Report progress in bursts. This is a micro-optimization when loading more than 50 million cells
                if (progressUntilReport >= progressReportThreshold)
                {
                    progress?.Report(progressUntilReport);
                    progressUntilReport = 0;
                }
            }

            if (progressUntilReport > 0)
            {
                progress?.Report(progressUntilReport);
            }
        }
    }

    private async Task ReadListFieldAsync(DataTableLite dataTable, ParquetRowGroupReader groupReader, int rowBeginIndex, ParquetSchemaElement itemField, int fieldIndex,
        long skipRecords, long readRecords, bool isFirstColumn, CancellationToken cancellationToken, IProgress<int>? progress)
    {
        var lastMilestone = "Start";
        try
        {
            if (itemField.FieldType == FieldTypeId.List)
            {
                var nestedListField = itemField.GetListField();
                var nestedItemField = nestedListField.GetListItemField();
                lastMilestone = "Read";

                await ReadListFieldAsync(dataTable, groupReader, rowBeginIndex, nestedItemField, fieldIndex,
                    skipRecords, readRecords, isFirstColumn, cancellationToken, progress);
            }
            else if (itemField.FieldType == FieldTypeId.Primitive)
            {
                int rowIndex = rowBeginIndex;

                var dataColumn = await ReadColumnAsync(groupReader, itemField, cancellationToken);
                lastMilestone = "Read";

                var dataEnumerable = dataColumn.Data;
                var listValueBuilder = new ListValueBuilder(dataColumn.RepetitionLevels!, dataColumn.DefinitionLevels!, dataEnumerable, itemField.ClrType);
                var listValues = listValueBuilder.ReadRows((int)skipRecords, (int)readRecords, itemField.Levels, cancellationToken);
                lastMilestone = "ReadRows";

                foreach (var listValue in listValues)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (isFirstColumn)
                    {
                        dataTable.NewRow();
                    }

                    dataTable.Rows[rowIndex][fieldIndex] = listValue as object ?? DBNull.Value;
                    rowIndex++;
                    progress?.Report(1);
                }
            }
            else if (itemField.FieldType == FieldTypeId.Struct)
            {
                await ReadStructFieldAsListAsync(dataTable, groupReader, rowBeginIndex, itemField, fieldIndex,
                    skipRecords, readRecords, isFirstColumn, cancellationToken, progress);
                lastMilestone = "Processed";
            }
            else
            {
                throw new NotSupportedException($"Lists of {itemField.FieldType}s are not currently supported");
            }
        }
        catch (Exception ex)
        {
            ex.Data["last_milestone"] = lastMilestone;
            throw;
        }
    }

    /// <summary>
    /// Reads a struct that is the element of a list (or nested inside such an element) as a single
    /// columnar <see cref="ListValue"/> where each entry is a <see cref="StructValueExt"/> (or DBNull
    /// when the struct instance itself is absent).
    /// </summary>
    private async Task ReadStructFieldAsListAsync(DataTableLite dataTable, ParquetRowGroupReader groupReader, int rowBeginIndex,
        ParquetSchemaElement structField, int fieldIndex, long skipRecords, long readRecords, bool isFirstColumn,
        CancellationToken cancellationToken, IProgress<int>? progress)
    {
        var lastMilestone = "Start";
        try
        {
            //Read struct data as a new datatable
            var structFieldTable = BuildDataTableLite(structField, structField.Children.Select(f => f.Path).ToList(), (int)readRecords);

            //Need to calculate progress differently for structs
            var structFieldReadProgress = StructReadProgress(progress, structFieldTable.Columns.Count);

            //Read the struct data and populate the datatable
            await ProcessRowGroupAsync(structFieldTable, groupReader, skipRecords, readRecords, cancellationToken, structFieldReadProgress);
            lastMilestone = "Processed";

            //Determine, per struct instance, whether the struct itself is present (non-null). This lets us
            //tell a genuinely null struct apart from a struct whose fields all happen to be null.
            var structPresence = await GetStructInstancePresenceAsync(structField, groupReader, skipRecords, readRecords, cancellationToken);

            //We need to pivot the data into a new data table (because we read it in columnar fashion above)
            int rowIndex = rowBeginIndex;
            int structRowIndex = 0;
            foreach (var values in structFieldTable.Rows)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var presence = structRowIndex < structPresence.Count ? structPresence[structRowIndex] : null;
                structRowIndex++;

                DataTableLite PivotLiteTable(object[] valueArray, DataTableLite? newStructFieldTableOverride = null)
                {
                    var newStructFieldTable = newStructFieldTableOverride ?? BuildDataTableLite(structField, structField.Children.Select(f => f.Path).ToList(), (int)readRecords);
                    for (var columnOrdinal = 0; columnOrdinal < valueArray.Length; columnOrdinal++)
                    {
                        lastMilestone = $"#{rowIndex}-{columnOrdinal}";
                        if (valueArray[columnOrdinal] == DBNull.Value)
                        {
                            //Empty array
                            continue;
                        }

                        var columnValues = (ListValue)valueArray[columnOrdinal];

                        if (columnValues.Data.Count == 0 && columnOrdinal != 0) //All values are null
                        {
                            for (var i = 0; i < newStructFieldTable.Rows.Count; i++)
                            {
                                newStructFieldTable.Rows[i][columnOrdinal] = DBNull.Value;
                            }

                            continue;
                        }

                        for (var rowValueIndex = 0; rowValueIndex < columnValues.Data.Count; rowValueIndex++)
                        {
                            lastMilestone = $"#{rowIndex}-{columnOrdinal}-{rowValueIndex}";

                            var columnValue = columnValues.Data[rowValueIndex] ?? throw new SystemException("Column value missing during pivot");
                            #region Hack for LIST_OF_STRUCT_OF_LIST_OF_STRUCT test
                            if (columnValue is StructValue structValue
                                && newStructFieldTable.Columns.Values.ElementAt(columnOrdinal).Type == typeof(ListValue))
                            {
                                //We need to convert `columnValue` from struct to a list of structs as it was a nested structure
                                if (structValue.Data is not DataRowLite dataRowLite)
                                {
                                    throw new InvalidDataException("Struct data wasn't the expected type.");
                                }

                                var nestedStructFieldTable = PivotLiteTable(structValue.Data.Row, dataRowLite.Table.Clone());
                                var listValues = new ArrayList(nestedStructFieldTable.Rows.Count);
                                for (var i = 0; i < nestedStructFieldTable.Rows.Count; i++)
                                {
                                    var row = nestedStructFieldTable.GetRowAt(i);

                                    //If every field of the struct is null, treat the struct as empty (matches DuckDB)
                                    if (row.Row.All(IsNullValued))
                                    {
                                        continue;
                                    }

                                    listValues.Add(new StructValue(row));
                                }
                                columnValue = new ListValue(listValues, typeof(StructValue));
                            }
                            #endregion

                            //Columns can have different numbers of values (e.g. the first column is null/empty
                            //while a later one isn't), so make sure a row exists before assigning into it.
                            while (newStructFieldTable.Rows.Count <= rowValueIndex)
                            {
                                newStructFieldTable.NewRow();

                                //Some pivoted columns don't have the same Data array lengths it seems.
                                //So pre-fill all values with DBNull to prevent NRE's later on.
                                Array.Fill(newStructFieldTable.Rows[newStructFieldTable.Rows.Count - 1], DBNull.Value);
                            }
                            newStructFieldTable.Rows[rowValueIndex][columnOrdinal] = columnValue;
                        }
                    }
                    return newStructFieldTable;
                }

                ArrayList? GetListOfStructs(object[] valueArray, List<bool>? presence)
                {
                    //A completely null struct column (e.g. the whole list is null) has no elements at all
                    if (valueArray.All(value => value == DBNull.Value))
                    {
                        return null;
                    }

                    var newStructFieldTable = PivotLiteTable(valueArray);

                    var listValues = new ArrayList(newStructFieldTable.Rows.Count);
                    for (var i = 0; i < newStructFieldTable.Rows.Count; i++)
                    {
                        var dataRow = newStructFieldTable.GetRowAt(i);

                        //A struct that is present but has all null fields must still be rendered as a struct.
                        //An absent struct whose row is entirely null (no nested list structure) is null; but a
                        //row that still carries nested collection structure (e.g. an empty list) must be kept
                        //as a struct so the parent level can collapse it to an empty list instead of null.
                        if (presence is not null && i < presence.Count && !presence[i]
                            && dataRow.Row.All(value => value == DBNull.Value))
                        {
                            listValues.Add(DBNull.Value);
                        }
                        else
                        {
                            listValues.Add(new StructValue(dataRow));
                        }
                    }
                    return listValues;
                }

                var listValues = GetListOfStructs(values, presence);

                if (isFirstColumn)
                {
                    dataTable.NewRow();
                }

                dataTable.Rows[rowIndex][fieldIndex] = listValues is null
                    ? DBNull.Value
                    : new ListValue(listValues, typeof(StructValue));
                rowIndex++;
                progress?.Report(1);
            }
        }
        catch (Exception ex)
        {
            ex.Data["last_milestone"] = lastMilestone;
            throw;
        }
    }

    /// <summary>
    /// For a List of Structs field, this method computes for each row in the current read range, the presence of each struct instance.
    /// A struct is present when the definition level of one of its leaves reaches the struct's <see cref="FieldLevels.MaxDefinitionLevel"/>.
    /// </summary>
    private static async Task<List<List<bool>>> GetStructInstancePresenceAsync(ParquetSchemaElement structField,
        ParquetRowGroupReader groupReader, long skipRecords, long readRecords, CancellationToken cancellationToken)
    {
        var leaf = GetFirstPrimitiveDescendant(structField);
        if (leaf is null)
        {
            return [];
        }

        var dataColumn = await ReadColumnAsync(groupReader, leaf, cancellationToken);
        var repetitionLevels = dataColumn.RepetitionLevels;
        var definitionLevels = dataColumn.DefinitionLevels;
        if (repetitionLevels is null || definitionLevels is null)
        {
            return [];
        }

        var rows = new List<List<bool>>();
        var currentRow = new List<bool>();
        for (var i = 0; i < repetitionLevels.Length; i++)
        {
            if (i > 0 && repetitionLevels[i] == 0)
            {
                rows.Add(currentRow);
                currentRow = [];
            }

            //Entries with a repetition level at or below the struct's own repetition nesting are the
            //beginning of a new struct instance; deeper entries belong to nested lists within the same
            //struct instance.
            if (repetitionLevels[i] <= structField.Levels.MaxRepetitionLevel)
            {
                currentRow.Add(definitionLevels.Length > i && definitionLevels[i] >= structField.Levels.MaxDefinitionLevel);
            }
        }
        rows.Add(currentRow);

        return rows.Skip((int)skipRecords).Take((int)readRecords).ToList();
    }

    private static ParquetSchemaElement? GetFirstPrimitiveDescendant(ParquetSchemaElement field)
    {
        var current = field;
        while (current.Children.Count > 0)
        {
            current = current.Children[0];
        }

        return current.IsPrimitive ? current : null;
    }

    /// <summary>
    /// Determines whether a value is effectively null, recursing into structs and lists.
    /// </summary>
    private static bool IsNullValued(object? value)
    {
        if (value == DBNull.Value)
            return true;

        if (value is IStructValue @struct)
            return @struct.Data.Row.All(IsNullValued);

        if (value is IListValue list)
            return list.Data.Cast<object?>().All(IsNullValued);

        return false;
    }

    private static async Task ReadMapFieldAsync(DataTableLite dataTable, ParquetRowGroupReader groupReader, int rowBeginIndex, ParquetSchemaElement field,
        long skipRecords, long readRecords, bool isFirstColumn, CancellationToken cancellationToken, IProgress<int>? progress)
    {
        var builder = await CreateMapValueBuilderAsync(field, groupReader, parentRepetitionBoundary: 0, cancellationToken);

        var fieldIndex = dataTable.Columns[field.Path]?.Ordinal ?? throw new ParquetEngineException($"Column `{field.Path}` is missing");
        int rowIndex = rowBeginIndex;
        foreach (var mapValue in builder.ReadRows(skipRecords, readRecords, cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (isFirstColumn)
            {
                dataTable.NewRow();
            }

            dataTable.Rows[rowIndex]![fieldIndex] = mapValue as object ?? DBNull.Value;
            rowIndex++;
            progress?.Report(1);
        }
    }

    /// <summary>
    /// Reads the leaf columns of a MAP and wraps them in a <see cref="MapValueBuilder"/>. Nested map
    /// values are read recursively, with each outer entry becoming a parent element of the nested builder.
    /// </summary>
    private static async Task<MapValueBuilder> CreateMapValueBuilderAsync(ParquetSchemaElement mapField,
        ParquetRowGroupReader groupReader, int parentRepetitionBoundary, CancellationToken cancellationToken)
    {
        var keyValueField = mapField.GetMapKeyValueField();
        var keyField = keyValueField.GetMapKeyField();
        var valueField = keyValueField.GetMapValueField();

        var keyColumn = await ReadColumnAsync(groupReader, keyField, cancellationToken);
        if (keyColumn.RepetitionLevels is null || keyColumn.DefinitionLevels is null)
            throw new MalformedFieldException($"Map field `{mapField.PathWithParent}` is missing repetition/definition levels");

        var keyValues = keyColumn.Data.ToArray();

        if (valueField.FieldType == FieldTypeId.Map)
        {
            var valueBuilder = await CreateMapValueBuilderAsync(valueField, groupReader,
                keyField.Levels.MaxRepetitionLevel, cancellationToken);
            return new MapValueBuilder(parentRepetitionBoundary, keyField.Levels, keyColumn.RepetitionLevels,
                keyColumn.DefinitionLevels, keyValues, keyField.ClrType, valueField.ClrType, valueBuilder: valueBuilder);
        }

        if (valueField.FieldType == FieldTypeId.Primitive && valueField.DataField?.IsArray != true)
        {
            var valueColumn = await ReadColumnAsync(groupReader, valueField, cancellationToken);
            return new MapValueBuilder(parentRepetitionBoundary, keyField.Levels, keyColumn.RepetitionLevels,
                keyColumn.DefinitionLevels, keyValues, keyField.ClrType, valueField.ClrType,
                valueValues: valueColumn.Data.ToArray());
        }

        throw new UnsupportedFieldException(
            $"Cannot load field `{mapField.PathWithParent}`. Map values of type {valueField.FieldType} are not supported");
    }

    private async Task ReadStructFieldAsync(DataTableLite dataTable, ParquetRowGroupReader groupReader, int rowBeginIndex, ParquetSchemaElement field,
       long skipRecords, long readRecords, bool isFirstColumn, CancellationToken cancellationToken, IProgress<int>? progress)
    {
        //A struct that lives inside a list-of-structs element (at any depth) must be read as a
        //columnar list value so it can be pivoted together with its sibling columns.
        if (field.BelongsToListOfStructsField)
        {
            var listFieldIndex = dataTable.Columns[field.Path]?.Ordinal ?? throw new ParquetEngineException($"Column `{field.Path}` is missing");
            await ReadStructFieldAsListAsync(dataTable, groupReader, rowBeginIndex, field, listFieldIndex,
                skipRecords, readRecords, isFirstColumn, cancellationToken, progress);
            return;
        }

        //Read struct data as a new datatable
        var structFieldTable = BuildDataTableLite(field, field.Children.Select(f => f.Path).ToList(), 1);

        //Need to calculate progress differently for structs
        var structFieldReadProgress = StructReadProgress(progress, structFieldTable.Columns.Count);

        //Read the struct data and populate the datatable
        await ProcessRowGroupAsync(structFieldTable, groupReader, skipRecords, readRecords, cancellationToken, structFieldReadProgress);

        var rowIndex = rowBeginIndex;
        var fieldIndex = dataTable.Columns[field.Path]?.Ordinal ?? throw new ParquetEngineException($"Column `{field.Path}` is missing");
        for (var i = 0; i < structFieldTable.Rows.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (isFirstColumn)
            {
                dataTable.NewRow();
            }

            //If all the fields of the struct are null, we assume the struct itself is null
            bool isNull = structFieldTable.Rows[i].All(item => item == DBNull.Value);

            if (isNull)
            {
                dataTable.Rows[rowIndex]![fieldIndex] = DBNull.Value;
            }
            else
            {
                var dataRow = structFieldTable.GetRowAt(i);
                dataTable.Rows[rowIndex]![fieldIndex] = new StructValue(dataRow);
            }
            rowIndex++;
        }
    }

    private static SimpleProgress StructReadProgress(IProgress<int>? _progress, int fieldCount)
    {
        var progress = new SimpleProgress();
        var reportedRowCount = 0;
        progress.ProgressChanged += (int progressSoFar) =>
        {
            if (fieldCount > 0)
            {
                //Progress is reported per struct field, so divide the cumulative total by the field
                //count to convert it to row count in the main data table. 
                var rowCount = progressSoFar / fieldCount;
                var increment = rowCount - reportedRowCount;
                if (increment > 0)
                {
                    _progress?.Report(increment);
                    reportedRowCount = rowCount;
                }
            }
            else
            {
                //If the struct field has no columns, then each read is one row.
                _progress?.Report(1);
            }
        };
        return progress;
    }

    private DataTableLite BuildDataTableLite(ParquetSchemaElement? parent, List<string> fields, int expectedRecordCount)
    {
        parent ??= (ParquetSchemaElement)Metadata.SchemaTree;
        DataTableLite dataTable = new(expectedRecordCount);
        foreach (var field in fields)
        {
            var schema = parent.GetChild(field);
            if (schema.FieldType == FieldTypeId.List
                || schema.DataField?.IsArray == true)
            {
                dataTable.AddColumn(field, typeof(ListValue), parent);
            }
            else if (schema.FieldType == FieldTypeId.Map)
            {
                dataTable.AddColumn(field, typeof(MapValue), parent);
            }
            else if (schema.FieldType == FieldTypeId.Struct)
            {
                dataTable.AddColumn(field, typeof(StructValue), parent);
            }
            else if (schema.SchemaElement.Type == Parquet.Meta.Type.BYTE_ARRAY
                && schema.SchemaElement.LogicalType is null
                && schema.SchemaElement.ConvertedType is null)
            {
                dataTable.AddColumn(field, typeof(ByteArrayValue), parent);
            }
            else if (schema.DataField is DateTimeDataField dateField)
            {
                if (dateField.DateTimeFormat == DateTimeFormat.Date)
                {
                    dataTable.AddColumn(field, typeof(DateOnly), parent);
                }
                else
                {
                    dataTable.AddColumn(field, typeof(DateTime), parent);
                }
            }
            else
            {
                var clrType = schema.ClrType ?? throw new MalformedFieldException($"`{(parent is not null ? parent + "/" : string.Empty)}/{field}` has no data field");
                dataTable.AddColumn(field, clrType, parent);
            }
        }
        return dataTable;
    }

    private static async Task<ParquetColumnData> ReadColumnAsync(ParquetRowGroupReader groupReader, ParquetSchemaElement field, CancellationToken cancellationToken)
    {
        try
        {
            return await ReflectiveColumnReader.ReadAsync(groupReader, field, cancellationToken);
        }
        catch (OverflowException ex)
        {
            var isDecimalField = field.SchemaElement?.ConvertedType == Parquet.Meta.ConvertedType.DECIMAL
                || field.SchemaElement?.LogicalType?.DECIMAL is not null;
            if (isDecimalField)
            {
                var scale = field.SchemaElement!.Scale ?? 0;
                var precision = field.SchemaElement.Precision ?? 0;
                if (scale > DecimalOverflowException.MAX_DECIMAL_SCALE
                    || precision > DecimalOverflowException.MAX_DECIMAL_PRECISION)
                {
                    throw new DecimalOverflowException(field.PathWithParent, precision, scale, ex);
                }
            }

            throw;
        }
        catch (ParquetException ex)
        {
            var maskedExMessage = ex.Message.Replace($"'{field.Path}'", $"`{field.Path}`");
            throw new ParquetEngineException(maskedExMessage, ex);
        }
    }
}