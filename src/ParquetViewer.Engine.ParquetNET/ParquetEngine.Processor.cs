using Parquet;
using ParquetViewer.Engine.Exceptions;
using ParquetViewer.Engine.Types;
using System.Data;

namespace ParquetViewer.Engine.ParquetNET;

public partial class ParquetEngine
{
    public async Task<Func<bool, DataTable>> ReadRowsAsync(List<string> selectedFields, int offset, int recordCount, IProgress<int>? progress = null, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(recordCount, nameof(recordCount));
        ArgumentOutOfRangeException.ThrowIfNegative(offset, nameof(offset));

        long recordsLeftToRead = recordCount;
        var result = BuildDataTableLite(selectedFields, (int)Math.Min(recordCount, RecordCount));

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
            var datatable = result.ToDataTableDispose(cancellationToken, logProgress ? progress : null);
            return datatable;
        };
    }

    private async Task<long> PopulateDataTableAsync(DataTableLite dataTable, ParquetReader parquetReader,
        long offset, long recordCount, CancellationToken cancellationToken, IProgress<int>? progress)
    {
        //Read column by column to generate each row in the datatable
        long totalRecordCountSoFar = 0;
        long rowsLeftToRead = recordCount;
        for (int i = 0; i < parquetReader.RowGroupCount; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using var groupReader = parquetReader.OpenRowGroupReader(i);
            if (groupReader.RowCount > int.MaxValue)
                throw new ArgumentOutOfRangeException(
                    string.Format("Cannot handle row group sizes greater than {0}. Found {1} instead.", int.MaxValue, groupReader.RowCount));

            long rowsPassedUntilThisRowGroup = totalRecordCountSoFar;
            totalRecordCountSoFar += groupReader.RowCount;

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
            await ReadFieldAsync(dataTable, groupReader, rowBeginIndex, field, skipRecords,
                readRecords, isFirstColumn, cancellationToken, progress);

            isFirstColumn = false;
        }
    }

    /// <summary>
    /// Reads a field (primitive, or a list, map or struct nested to any depth) one row at a time.
    /// </summary>
    private static async Task ReadFieldAsync(DataTableLite dataTable, ParquetRowGroupReader groupReader, int rowBeginIndex, ParquetSchemaElement field,
        long skipRecords, long readRecords, bool isFirstColumn, CancellationToken cancellationToken, IProgress<int>? progress)
    {
        var fieldIndex = dataTable.Columns[field.Path]?.Ordinal ?? throw new ParquetEngineException($"Column `{field.Path}` is missing");
        var builder = await CreateValueBuilderAsync(field, groupReader, cancellationToken);

        var rowIndex = rowBeginIndex;
        var progressUntilReport = 0;
        const int progressReportThreshold = 10;
        foreach (var value in builder.ReadRows(skipRecords, readRecords, cancellationToken))
        {
            if (isFirstColumn)
            {
                dataTable.NewRow();
            }

            dataTable.Rows[rowIndex]![fieldIndex] = value;
            rowIndex++;
            progressUntilReport++;

            //Report progress in bursts. Reporting every cell is measurably slower when loading tens of millions of cells
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

    /// <summary>
    /// Reads the leaf columns beneath <paramref name="field"/> and builds the matching tree of value builders.
    /// </summary>
    private static async Task<FieldValueBuilder> CreateValueBuilderAsync(ParquetSchemaElement field, ParquetRowGroupReader groupReader,
        CancellationToken cancellationToken)
    {
        FieldValueBuilder builder;
        switch (field.FieldType)
        {
            case FieldTypeId.Primitive:
                var column = await ReadColumnAsync(groupReader, field, cancellationToken);
                var leaf = new LeafCursor(column, column.RepetitionLevels, column.DefinitionLevels, field.Levels.MaxDefinitionLevel);
                builder = new PrimitiveValueBuilder(leaf, column.ValueType);
                break;
            case FieldTypeId.List:
                var repeatedField = field.GetListField();
                var element = await CreateValueBuilderAsync(repeatedField.GetListItemField(), groupReader, cancellationToken);
                return new ListValueBuilder(repeatedField.Levels.MaxDefinitionLevel, repeatedField.Levels.MaxRepetitionLevel, element);
            case FieldTypeId.Map:
                var keyValueField = field.GetMapKeyValueField();
                var key = await CreateValueBuilderAsync(keyValueField.GetMapKeyField(), groupReader, cancellationToken);
                var value = await CreateValueBuilderAsync(keyValueField.GetMapValueField(), groupReader, cancellationToken);
                return new MapValueBuilder(keyValueField.Levels.MaxDefinitionLevel, keyValueField.Levels.MaxRepetitionLevel, key, value);
            case FieldTypeId.Struct:
                var children = field.Children;
                var fields = new List<FieldValueBuilder>(children.Count);
                foreach (var child in children)
                {
                    fields.Add(await CreateValueBuilderAsync(child, groupReader, cancellationToken));
                }
                builder = new StructValueBuilder(field.Levels.MaxDefinitionLevel, [.. children.Select(child => child.Path)], fields);
                break;
            default:
                throw new UnsupportedFieldException($"Field `{field.PathWithParent}` has an unknown type {field.FieldType}.");
        }

        //The repeated nodes inside a LIST or MAP are handled by those cases above and never built on their own,
        //so a repeated field reaching here repeats by itself (a legacy "two-tier" list) so it's a list of itself
        return field.RepetitionType == RepetitionTypeId.Repeated
            ? new ListValueBuilder(field.Levels.MaxDefinitionLevel, field.Levels.MaxRepetitionLevel, builder)
            : builder;
    }

    private DataTableLite BuildDataTableLite(List<string> fields, int expectedRecordCount)
    {
        var parent = (ParquetSchemaElement)Metadata.SchemaTree;
        DataTableLite dataTable = new(expectedRecordCount);
        foreach (var field in fields)
        {
            var schema = parent.GetChild(field);

            var columnType = schema.ClrType;
            if (schema.RepetitionType == RepetitionTypeId.Repeated)
            {
                //Because we're going through top level fields, none of them should be REPEATED according to the Parquet spec.
                //Only exception is legacy 2-tier Lists. Which can be of primitives or Structs only.
                switch (schema.FieldType)
                {
                    case FieldTypeId.Map:
                    case FieldTypeId.List:
                        throw new MalformedFieldException($"Repeated LIST and MAP fields aren't allowed by the Parquet spec. Field: `{field}`");
                    case FieldTypeId.Primitive:
                    case FieldTypeId.Struct:
                        //A legacy two tier list. Its ClrType is the element's type, but we read it as a list
                        columnType = typeof(ListValue);
                        break;
                    default:
                        break;
                }
            }

            dataTable.AddColumn(field, columnType, parent);
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