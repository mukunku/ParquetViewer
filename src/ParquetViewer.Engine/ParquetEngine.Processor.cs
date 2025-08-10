using Parquet;
using ParquetViewer.Engine.Exceptions;
using ParquetViewer.Engine.Types;
using System.Collections;
using System.Data;
namespace ParquetViewer.Engine
{
    public partial class ParquetEngine
    {
        public static readonly string TotalRecordCountExtendedPropertyKey = "TOTAL_RECORD_COUNT";

        public async Task<Func<bool, DataTable>> ReadRowsAsync(List<string> selectedFields, int offset, int recordCount, CancellationToken cancellationToken, IProgress<int>? progress = null)
        {
            long recordsLeftToRead = recordCount;
            DataTableLite result = BuildDataTable(null, selectedFields, Math.Min(recordCount, (int)this.RecordCount));

            foreach (var reader in this.GetReaders(offset))
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (recordsLeftToRead <= 0)
                    break;

                recordsLeftToRead = await PopulateDataTable(result, reader.ParquetReader, reader.RemainingOffset, recordsLeftToRead, cancellationToken, progress);
            }

            result.DataSetSize = this.RecordCount;

            return (logProgress) =>
            {
                var datatable = result.ToDataTable(cancellationToken, logProgress ? progress : null);
                datatable.ExtendedProperties[TotalRecordCountExtendedPropertyKey] = result.DataSetSize;
                return datatable;
            };
        }

        private async Task<long> PopulateDataTable(DataTableLite dataTable, ParquetReader parquetReader,
            long offset, long recordCount, CancellationToken cancellationToken, IProgress<int>? progress)
        {
            //Read column by column to generate each row in the datatable
            int totalRecordCountSoFar = 0;
            long rowsLeftToRead = recordCount;
            for (int i = 0; i < parquetReader.RowGroupCount; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                using (ParquetRowGroupReader groupReader = parquetReader.OpenRowGroupReader(i))
                {
                    if (groupReader.RowCount > int.MaxValue)
                        throw new ArgumentOutOfRangeException(string.Format("Cannot handle row group sizes greater than {0}. Found {1} instead.", int.MaxValue, groupReader.RowCount));

                    int rowsPassedUntilThisRowGroup = totalRecordCountSoFar;
                    totalRecordCountSoFar += (int)groupReader.RowCount;

                    if (offset >= totalRecordCountSoFar)
                        continue;

                    if (rowsLeftToRead <= 0)
                        break;

                    long numberOfRecordsToReadFromThisRowGroup = Math.Min(Math.Min(totalRecordCountSoFar - offset, rowsLeftToRead), groupReader.RowCount);
                    rowsLeftToRead -= numberOfRecordsToReadFromThisRowGroup;

                    long recordsToSkipInThisRowGroup = Math.Max(offset - rowsPassedUntilThisRowGroup, 0);

                    await ProcessRowGroup(dataTable, groupReader, recordsToSkipInThisRowGroup, numberOfRecordsToReadFromThisRowGroup, cancellationToken, progress);
                }
            }

            return rowsLeftToRead;
        }

        private async Task ProcessRowGroup(DataTableLite dataTable, ParquetRowGroupReader groupReader,
            long skipRecords, long readRecords, CancellationToken cancellationToken, IProgress<int>? progress)
        {
            int rowBeginIndex = dataTable.Rows.Count;
            bool isFirstColumn = true;

            foreach (DataTableLite.ColumnLite column in dataTable.Columns.Values)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var field = column.ParentSchema.GetChild(column.Name);
                switch (field.FieldType())
                {
                    case ParquetSchemaElement.FieldTypeId.Primitive:
                        await ReadPrimitiveField(dataTable, groupReader, rowBeginIndex, field, skipRecords,
                            readRecords, isFirstColumn, cancellationToken, progress);
                        break;
                    case ParquetSchemaElement.FieldTypeId.List:
                        var listField = field.GetSingleOrByName("list");
                        ParquetSchemaElement itemField;
                        try
                        {
                            itemField = listField.GetSingleOrByName("item");
                        }
                        catch (Exception ex)
                        {
                            throw new UnsupportedFieldException($"Cannot load field `{field.Path}`. Invalid List type.", ex);
                        }
                        var fieldIndex = dataTable.Columns[field.Path]!.Ordinal;
                        await ReadListField(dataTable, groupReader, rowBeginIndex, itemField, fieldIndex,
                            skipRecords, readRecords, isFirstColumn, cancellationToken, progress);
                        break;
                    case ParquetSchemaElement.FieldTypeId.Map:
                        await ReadMapField(dataTable, groupReader, rowBeginIndex, field, skipRecords,
                            readRecords, isFirstColumn, cancellationToken, progress);
                        break;
                    case ParquetSchemaElement.FieldTypeId.Struct:
                        await ReadStructField(dataTable, groupReader, rowBeginIndex, field, skipRecords,
                            readRecords, isFirstColumn, cancellationToken, progress);
                        break;
                }

                isFirstColumn = false;
            }
        }

        private async Task ReadPrimitiveField(DataTableLite dataTable, ParquetRowGroupReader groupReader, int rowBeginIndex, ParquetSchemaElement field,
        long skipRecords, long readRecords, bool isFirstColumn, CancellationToken cancellationToken, IProgress<int>? progress)
        {
            int rowIndex = rowBeginIndex;

            int skippedRecords = 0;
            var dataColumn = await groupReader.ReadColumnAsync(field.DataField ?? throw new MalformedFieldException($"Pritimive field `{field.Path}` is missing its data field"), cancellationToken);

            bool doesFieldBelongToAList = field.Parents.Any(field => field.FieldType() == ParquetSchemaElement.FieldTypeId.List);
            int fieldIndex = dataTable.Columns[field.Path]?.Ordinal ?? throw new ParquetEngineException($"Column `{field.Path}` is missing");
            if (doesFieldBelongToAList)
            {
                dataColumn = null;
                await ReadListField(dataTable, groupReader, rowBeginIndex, field, fieldIndex, skipRecords, readRecords, isFirstColumn, cancellationToken, progress);
            }
            else
            {
                var fieldType = dataTable.Columns[field.Path].Type; var byteArrayValueType = typeof(ByteArrayValue);
                foreach (var value in dataColumn.Data)
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
                    else if (fieldType == byteArrayValueType)
                    {
                        dataTable.Rows[rowIndex]![fieldIndex] = new ByteArrayValue(field.Path, (byte[])value);
                    }
                    else
                    {
                        dataTable.Rows[rowIndex]![fieldIndex] = value;
                    }

                    rowIndex++;
                    progress?.Report(1);
                }
            }
        }

        private async Task ReadListField(DataTableLite dataTable, ParquetRowGroupReader groupReader, int rowBeginIndex, ParquetSchemaElement itemField, int fieldIndex,
            long skipRecords, long readRecords, bool isFirstColumn, CancellationToken cancellationToken, IProgress<int>? progress)
        {
            if (itemField.FieldType() == ParquetSchemaElement.FieldTypeId.Primitive)
            {
                int rowIndex = rowBeginIndex;

                int skippedRecords = 0;
                var dataColumn = await groupReader.ReadColumnAsync(itemField.DataField!, cancellationToken);

                var dataEnumerable = dataColumn.Data.Cast<object?>().Select(d => d ?? DBNull.Value);
                //Some parquet writers don't write null entries into the data array for empty and null lists.
                //This throws off our logic below so lets find all empty/null lists and add a null entry into 
                //the data array to align it with the repetition levels.
                int levelCount = dataColumn.RepetitionLevels?.Length ?? 0;
                if (levelCount > dataColumn.Data.Length)
                {
                    if (dataColumn.RepetitionLevels?.Length != dataColumn.DefinitionLevels?.Length)
                    {
                        throw new MalformedFieldException($"Field `{itemField.Path}` appears malformed due to different lengths in repetition and definition levels");
                    }

                    dataEnumerable = GetDataWithPaddedNulls();

                    IEnumerable<object> GetDataWithPaddedNulls()
                    {
                        var index = -1;
                        foreach (var data in dataColumn.Data)
                        {
                            index++;

                            while (dataColumn.IsEmpty(index) || dataColumn.IsNull(index))
                            {
                                yield return DBNull.Value;
                                index++;
                            }

                            yield return data ?? DBNull.Value;
                        }

                        //Need to handle case where last N rows are null/empty
                        while (levelCount > index + 1)
                        {
                            yield return DBNull.Value;
                            index++;
                        }
                    }
                }

                ArrayList? rowValue = null;
                int index = -1;
                foreach (var data in dataEnumerable)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    index++;

                    bool IsEndOfRow(int index) => (index + 1) == dataColumn.RepetitionLevels!.Length
                        || dataColumn.RepetitionLevels[index + 1] == 0; //0 means new list

                    //Skip rows
                    if (skipRecords > skippedRecords)
                    {
                        if (IsEndOfRow(index))
                            skippedRecords++;

                        continue;
                    }

                    rowValue ??= new ArrayList();
                    if (IsEndOfRow(index))
                    {
                        if (isFirstColumn)
                        {
                            dataTable.NewRow();
                        }

                        rowValue.Add(data);

                        if (dataColumn.IsNull(index))
                            dataTable.Rows[rowIndex]![fieldIndex] = DBNull.Value;
                        else if (dataColumn.IsEmpty(index))
                            dataTable.Rows[rowIndex]![fieldIndex] = new ListValue([], itemField.DataField!.ClrType);
                        else
                            dataTable.Rows[rowIndex]![fieldIndex] = new ListValue(rowValue, itemField.DataField!.ClrType);

                        rowValue = null;

                        rowIndex++;
                        progress?.Report(1);

                        if (rowIndex - rowBeginIndex >= readRecords)
                            break;
                    }
                    else
                    {
                        rowValue.Add(data);
                    }
                }
            }
            else if (itemField.FieldType() == ParquetSchemaElement.FieldTypeId.Struct)
            {
                //Read struct data as a new datatable
                DataTableLite structFieldTable = BuildDataTable(itemField, itemField.Children.Select(f => f.Path).ToList(), (int)readRecords);

                //Need to calculate progress differently for structs
                var structFieldReadProgress = StructReadProgress(progress, structFieldTable.Columns.Count);

                //Read the struct data and populate the datatable
                await ProcessRowGroup(structFieldTable, groupReader, skipRecords, readRecords, cancellationToken, structFieldReadProgress);

                //We need to pivot the data into a new data table (because we read it in columnar fashion above)
                int rowIndex = rowBeginIndex;
                foreach (var values in structFieldTable.Rows)
                {
                    var newStructFieldTable = BuildDataTable(itemField, itemField.Children.Select(f => f.Path).ToList(), (int)readRecords);
                    for (var columnOrdinal = 0; columnOrdinal < values.Length; columnOrdinal++)
                    {
                        if (values[columnOrdinal] == DBNull.Value)
                        {
                            //Empty array
                            continue;
                        }

                        var columnValues = (ListValue)values[columnOrdinal];
                        for (var rowValueIndex = 0; rowValueIndex < columnValues.Length; rowValueIndex++)
                        {
                            var columnValue = columnValues.Data[rowValueIndex] ?? throw new SystemException("This should never happen");
                            bool isFirstValueColumn = columnOrdinal == 0;
                            if (isFirstValueColumn)
                            {
                                newStructFieldTable.NewRow();
                            }
                            newStructFieldTable.Rows[rowValueIndex][columnOrdinal] = columnValue;
                        }
                    }

                    if (isFirstColumn)
                        dataTable.NewRow();

                    var listValuesDataTable = newStructFieldTable.ToDataTable(cancellationToken);
                    var listValues = new ArrayList(listValuesDataTable.Rows.Count);
                    foreach (DataRow row in listValuesDataTable.Rows)
                    {
                        var newStructField = new StructValue(itemField.Path, row);
                        listValues.Add(newStructField);
                    }

                    dataTable.Rows[rowIndex][fieldIndex] = new ListValue(listValues, typeof(StructValue));
                    rowIndex++;
                }
            }
            else
            {
                throw new NotSupportedException($"Lists of {itemField.FieldType()}s are not currently supported");
            }
        }

        private static async Task ReadMapField(DataTableLite dataTable, ParquetRowGroupReader groupReader, int rowBeginIndex, ParquetSchemaElement field,
            long skipRecords, long readRecords, bool isFirstColumn, CancellationToken cancellationToken, IProgress<int>? progress)
        {
            var keyValueField = field.GetSingleOrByName("key_value");
            var keyField = keyValueField.GetChildCI("key");
            var valueField = keyValueField.GetChildCI("value");

            if (keyField.Children.Any() || valueField.Children.Any())
                throw new UnsupportedFieldException($"Cannot load field `{field.Path}`. Nested map types are not supported");

            int rowIndex = rowBeginIndex;

            int skippedRecords = 0;
            var keyDataColumn = await groupReader.ReadColumnAsync(keyField.DataField!, cancellationToken);
            var valueDataColumn = await groupReader.ReadColumnAsync(valueField.DataField!, cancellationToken);

            var dataEnumerable = Helpers.PairEnumerables(
                keyDataColumn.Data.Cast<object?>().Select(key => key ?? DBNull.Value), 
                valueDataColumn.Data.Cast<object?>().Select(value => value ?? DBNull.Value),
                DBNull.Value);

            //Some parquet writers don't write null entries into the data array for empty and null maps.
            //This throws off our logic below so lets find all empty/null maps and add a null entry into 
            //the data array to align it with the repetition levels.
            int levelCount = Math.Max(keyDataColumn.RepetitionLevels?.Length ?? 0, valueDataColumn.RepetitionLevels?.Length ?? 0);
            int dataCount = Math.Max(keyDataColumn.Data.Length, valueDataColumn.Data.Length);
            if (levelCount > dataCount)
            {
                if (keyDataColumn.RepetitionLevels?.Length != keyDataColumn.DefinitionLevels?.Length)
                    throw new MalformedFieldException($"Field `{field.Path}` appears have malformed keys due to different lengths in repetition and definition levels");
                else if (valueDataColumn.RepetitionLevels?.Length != valueDataColumn.DefinitionLevels?.Length)
                    throw new MalformedFieldException($"Field `{field.Path}` appears have malformed values due to different lengths in repetition and definition levels");

                dataEnumerable = GetDataWithPaddedNulls();

                IEnumerable<(object?, object?)> GetDataWithPaddedNulls()
                {
                    var index = -1;
                    foreach ((var key, var value) in Helpers.PairEnumerables(keyDataColumn.Data.Cast<object?>(), valueDataColumn.Data.Cast<object?>(), DBNull.Value))
                    {
                        index++;

                        while (keyDataColumn.IsNull(index) || valueDataColumn.IsNull(index)
                            || keyDataColumn.IsEmpty(index) || valueDataColumn.IsEmpty(index))
                        {
                            yield return (DBNull.Value, DBNull.Value);
                            index++;
                        }

                        yield return (key ?? DBNull.Value, value ?? DBNull.Value);
                    }

                    //Need to handle case where last N rows are null/empty
                    while (levelCount > index + 1)
                    {
                        yield return (DBNull.Value, DBNull.Value);
                        index++;
                    }
                }
            }

            var fieldIndex = dataTable.Columns[field.Path]!.Ordinal;
            ArrayList? mapKeys = null;
            ArrayList? mapValues = null;
            int index = -1;
            foreach (var (key, value) in dataEnumerable)
            {
                cancellationToken.ThrowIfCancellationRequested();
                index++;

                bool IsEndOfRow() => (index + 1) == levelCount
                    || GetRepetitionLevel(index + 1) == 0; //0 means new map

                //Skip rows
                if (skipRecords > skippedRecords)
                {
                    if (IsEndOfRow())
                        skippedRecords++;

                    continue;
                }

                mapKeys ??= [];
                mapValues ??= [];
                if (IsEndOfRow())
                {
                    if (isFirstColumn)
                    {
                        dataTable.NewRow();
                    }

                    mapKeys.Add(key);
                    mapValues.Add(value);

                    if (keyDataColumn.IsNull(index) || valueDataColumn.IsNull(index)) //If one is null the other should be as well
                        dataTable.Rows[rowIndex]![fieldIndex] = DBNull.Value;
                    else if (keyDataColumn.IsEmpty(index) || valueDataColumn.IsEmpty(index)) //If one is empty the other should be as well
                        dataTable.Rows[rowIndex]![fieldIndex] = new MapValue([], keyField.DataField!.ClrType, [], valueField.DataField!.ClrType);
                    else
                        dataTable.Rows[rowIndex]![fieldIndex] = new MapValue(mapKeys, keyField.DataField!.ClrType, mapValues, valueField.DataField!.ClrType);

                    mapKeys = null;
                    mapValues = null;

                    rowIndex++;
                    progress?.Report(1);

                    if (rowIndex - rowBeginIndex >= readRecords)
                        break;
                }
                else
                {
                    mapKeys.Add(key);
                    mapValues.Add(value);
                }

                int GetRepetitionLevel(int dataIndex)
                {
                    if (keyDataColumn.RepetitionLevels is null && valueDataColumn.RepetitionLevels is null)
                        return 0; // assume each entry is a new row since we have no repetition levels
                    else if (keyDataColumn.RepetitionLevels?.Length > dataIndex)
                        return keyDataColumn.RepetitionLevels[dataIndex];
                    else if (valueDataColumn.RepetitionLevels?.Length > dataIndex)
                        return valueDataColumn.RepetitionLevels[dataIndex];
                    else
                        throw new ArgumentOutOfRangeException(nameof(dataIndex));
                }
            }
        }

        private async Task ReadStructField(DataTableLite dataTable, ParquetRowGroupReader groupReader, int rowBeginIndex, ParquetSchemaElement field,
           long skipRecords, long readRecords, bool isFirstColumn, CancellationToken cancellationToken, IProgress<int>? progress)
        {
            //Read struct data as a new datatable
            DataTableLite structFieldTable = BuildDataTable(field, field.Children.Select(f => f.Path).ToList(), 1);

            //Need to calculate progress differently for structs
            var structFieldReadProgress = StructReadProgress(progress, structFieldTable.Columns.Count);

            //Read the struct data and populate the datatable
            await ProcessRowGroup(structFieldTable, groupReader, skipRecords, readRecords, cancellationToken, structFieldReadProgress);

            var rowIndex = rowBeginIndex;
            var fieldIndex = dataTable.Columns[field.Path]?.Ordinal ?? throw new ParquetEngineException($"Column `{field.Path}` is missing");
            var finalResultDataTable = structFieldTable.ToDataTable(cancellationToken);
            for (var i = 0; i < finalResultDataTable.Rows.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (isFirstColumn)
                {
                    dataTable.NewRow();
                }

                //Not sure how to detect if a Struct is NULL vs. all its fields being NULL.
                //For now lets consider that if all fields are NULL, the row is supposed to be NULL.
                bool isNull = !finalResultDataTable.Rows[i].ItemArray.Any(item => item != DBNull.Value);

                if (isNull)
                {
                    dataTable.Rows[rowIndex]![fieldIndex] = DBNull.Value;
                }
                else
                {
                    dataTable.Rows[rowIndex]![fieldIndex] = new StructValue(field.Path, finalResultDataTable.Rows[i]);
                }
                rowIndex++;
            }
        }

        private SimpleProgress StructReadProgress(IProgress<int>? _progress, int fieldCount)
        {
            var progress = new SimpleProgress();
            progress.ProgressChanged += (int progressSoFar) =>
            {
                if (fieldCount > 0)
                {
                    //To report progress accurately we'll need to divide the progress total  
                    //by the field count to convert it to row count in the main data table.
                    var increment = progressSoFar % fieldCount;
                    if (increment == 0)
                        _progress?.Report(1);
                }
                else
                {
                    //If the struct field has no columns, then each read is one row.
                    _progress?.Report(1);
                }
            };
            return progress;
        }

        private DataTableLite BuildDataTable(ParquetSchemaElement? parent, List<string> fields, int expectedRecordCount)
        {
            parent ??= this.ParquetSchemaTree;
            DataTableLite dataTable = new(expectedRecordCount);
            foreach (var field in fields)
            {
                var schema = parent.GetChild(field);
                if (schema.FieldType() == ParquetSchemaElement.FieldTypeId.List)
                {
                    dataTable.AddColumn(field, typeof(ListValue), parent);
                }
                else if (schema.FieldType() == ParquetSchemaElement.FieldTypeId.Map)
                {
                    dataTable.AddColumn(field, typeof(MapValue), parent);
                }
                else if (schema.FieldType() == ParquetSchemaElement.FieldTypeId.Struct)
                {
                    dataTable.AddColumn(field, typeof(StructValue), parent);
                }
                else if (schema.SchemaElement.Type == Parquet.Meta.Type.BYTE_ARRAY
                    && schema.SchemaElement.LogicalType is null
                    && schema.SchemaElement.ConvertedType is null)
                {
                    dataTable.AddColumn(field, typeof(ByteArrayValue), parent);
                }
                else
                {
                    var clrType = schema.DataField?.ClrType ?? throw new MalformedFieldException($"{(parent is not null ? parent + "/" : string.Empty)}/{field} has no data field");
                    dataTable.AddColumn(field, clrType, parent);
                }
            }
            return dataTable;
        }
    }
}
