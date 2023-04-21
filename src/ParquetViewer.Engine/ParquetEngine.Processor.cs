using Parquet;
using ParquetViewer.Engine.Exceptions;
using System.Collections;
using System.Data;

namespace ParquetViewer.Engine
{
    public partial class ParquetEngine
    {
        public static readonly string TotalRecordCountExtendedPropertyKey = "TOTAL_RECORD_COUNT";
        private static readonly int MinMemoryGBNeededForAdditionalCaching = 8; //Let's not use extra caching if the PC doesn't have more than 8GB memory

        private static bool? _useDataRowCache;
        private static bool UseDataRowCache =>
            _useDataRowCache ??= (GC.GetGCMemoryInfo().TotalAvailableMemoryBytes / 1048576.0 /*magic number*/) > (MinMemoryGBNeededForAdditionalCaching * 1024);

        public async Task<DataTable> ReadRowsAsync(List<string> selectedFields, int offset, int recordCount, CancellationToken cancellationToken, IProgress<int>? progress = null)
        {
            long recordsLeftToRead = recordCount;
            DataTable result = BuildDataTable(selectedFields);
            result.BeginLoadData(); //might speed things up

            foreach (var reader in this.GetReaders(offset))
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (recordsLeftToRead <= 0)
                    break;

                recordsLeftToRead = await ParquetReaderToDataTable(result, reader.ParquetReader, reader.RemainingOffset, recordsLeftToRead, cancellationToken, progress);
            }

            result.ExtendedProperties.Add(TotalRecordCountExtendedPropertyKey, this.RecordCount);
            result.EndLoadData();

            return result;
        }

        private async Task<long> ParquetReaderToDataTable(DataTable dataTable, ParquetReader parquetReader,
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

        private async Task ProcessRowGroup(DataTable dataTable, ParquetRowGroupReader groupReader, 
            long skipRecords, long readRecords, CancellationToken cancellationToken, IProgress<int>? progress)
        {
            int rowBeginIndex = dataTable.Rows.Count;
            bool isFirstColumn = true;

            var rowLookupCache = new Dictionary<int, DataRow>();
            foreach (DataColumn column in dataTable.Columns)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var field = ParquetSchemaTree.GetChildByName(column.ColumnName);
                if (field.SchemaElement.LogicalType?.LIST is not null)
                {
                    await ReadListField(dataTable, groupReader, rowBeginIndex, field, skipRecords,
                        readRecords, isFirstColumn, rowLookupCache, cancellationToken, progress);
                }
                else if (field.SchemaElement.LogicalType?.MAP is not null)
                {
                    await ReadMapField(dataTable, groupReader, rowBeginIndex, field, skipRecords,
                        readRecords, isFirstColumn, rowLookupCache, cancellationToken, progress);
                }
                else
                {
                    await ReadPrimitiveField(dataTable, groupReader, rowBeginIndex, field, skipRecords,
                        readRecords, isFirstColumn, rowLookupCache, cancellationToken, progress);
                }

                isFirstColumn = false;
            }
        }

        private static async Task ReadPrimitiveField(DataTable dataTable, ParquetRowGroupReader groupReader, int rowBeginIndex, ParquetSchemaElement field,
            long skipRecords, long readRecords, bool isFirstColumn, Dictionary<int, DataRow> rowLookupCache, CancellationToken cancellationToken, IProgress<int>? progress)
        {
            int rowIndex = rowBeginIndex;

            int skippedRecords = 0;
            var dataColumn = await groupReader.ReadColumnAsync(field.DataField, cancellationToken);

            var fieldIndex = dataTable.Columns[field.Path].Ordinal;
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
                    var newRow = dataTable.NewRow();
                    dataTable.Rows.Add(newRow);
                }

                DataRow datarow;
                if (!UseDataRowCache)
                {
                    datarow = dataTable.Rows[rowIndex];
                }
                else
                {
                    if (!rowLookupCache.TryGetValue(rowIndex, out datarow))
                    {
                        datarow = dataTable.Rows[rowIndex];
                        rowLookupCache.TryAdd(rowIndex, datarow);
                    }
                }

                datarow[fieldIndex] = ReadParquetValue(value, field.DataField, field.SchemaElement.LogicalType);

                rowIndex++;
                progress?.Report(1);
            }
        }

        private static async Task ReadListField(DataTable dataTable, ParquetRowGroupReader groupReader, int rowBeginIndex, ParquetSchemaElement field,
            long skipRecords, long readRecords, bool isFirstColumn, Dictionary<int, DataRow> rowLookupCache, CancellationToken cancellationToken, IProgress<int>? progress)
        {
            var listField = field.GetChildByName("list");
            var itemField = listField.GetChildByName("item");

            if (itemField.Children.Any())
                throw new UnsupportedFieldException($"Cannot load field '{field.Path}'. Nested list types are not supported");

            int rowIndex = rowBeginIndex;

            int skippedRecords = 0;
            var dataColumn = await groupReader.ReadColumnAsync(itemField.DataField, cancellationToken);

            ArrayList? rowValue = null;
            var fieldIndex = dataTable.Columns[field.Path].Ordinal;
            for (int i = 0; i < dataColumn.Data.Length; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                rowValue ??= new ArrayList();

                bool IsEndOfRow() => (i + 1) == dataColumn.RepetitionLevels.Length
                    || dataColumn.RepetitionLevels[i + 1] == 0; //0 means new list

                //Skip rows
                while (skipRecords > skippedRecords)
                {
                    if (IsEndOfRow())
                        skippedRecords++;

                    i++;
                }

                //If we skipped to the end then just exit
                if (i == dataColumn.Data.Length)
                    break;

                if (IsEndOfRow())
                {
                    if (isFirstColumn)
                    {
                        var newRow = dataTable.NewRow();
                        dataTable.Rows.Add(newRow);
                    }

                    DataRow datarow;
                    if (!UseDataRowCache)
                    {
                        datarow = dataTable.Rows[rowIndex];
                    }
                    else
                    {
                        if (!rowLookupCache.TryGetValue(rowIndex, out datarow))
                        {
                            datarow = dataTable.Rows[rowIndex];
                            rowLookupCache.TryAdd(rowIndex, datarow);
                        }
                    }

                    var lastItem = ReadParquetValue(dataColumn.Data.GetValue(i), itemField.DataField, itemField.SchemaElement.LogicalType);
                    rowValue.Add(lastItem);

                    datarow[fieldIndex] = new ListValue(rowValue, itemField.DataField.ClrType);
                    rowValue = null;

                    rowIndex++;
                    progress?.Report(1);

                    if (rowIndex - rowBeginIndex >= readRecords)
                        break;
                }
                else
                {
                    var value = ReadParquetValue(dataColumn.Data.GetValue(i), itemField.DataField, itemField.SchemaElement.LogicalType);
                    rowValue.Add(value);
                }
            }
        }

        private static async Task ReadMapField(DataTable dataTable, ParquetRowGroupReader groupReader, int rowBeginIndex, ParquetSchemaElement field,
            long skipRecords, long readRecords, bool isFirstColumn, Dictionary<int, DataRow> rowLookupCache, CancellationToken cancellationToken, IProgress<int>? progress)
        {
            var keyValueField = field.GetChildByName("key_value");
            var keyField = keyValueField.GetChildByName("key");
            var valueField = keyValueField.GetChildByName("value");

            if (keyField.Children.Any() || valueField.Children.Any())
                throw new UnsupportedFieldException($"Cannot load field '{field.Path}'. Nested map types are not supported");

            int rowIndex = rowBeginIndex;

            int skippedRecords = 0;
            var keyDataColumn = await groupReader.ReadColumnAsync(keyField.DataField, cancellationToken);
            var valueDataColumn = await groupReader.ReadColumnAsync(valueField.DataField, cancellationToken);

            var fieldIndex = dataTable.Columns[field.Path].Ordinal;
            for (int i = 0; i < valueDataColumn.Data.Length; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (skippedRecords < skipRecords)
                {
                    skippedRecords++;
                    continue;
                }

                if (isFirstColumn)
                {
                    var newRow = dataTable.NewRow();
                    dataTable.Rows.Add(newRow);
                }

                DataRow datarow;
                if (!UseDataRowCache)
                {
                    datarow = dataTable.Rows[rowIndex];
                }
                else
                {
                    if (!rowLookupCache.TryGetValue(rowIndex, out datarow))
                    {
                        datarow = dataTable.Rows[rowIndex];
                        rowLookupCache.TryAdd(rowIndex, datarow);
                    }
                }

                var key = ReadParquetValue(keyDataColumn.Data.GetValue(i), keyField.DataField, keyField.SchemaElement.LogicalType);
                var value = ReadParquetValue(valueDataColumn.Data.GetValue(i), valueField.DataField, valueField.SchemaElement.LogicalType);

                datarow[fieldIndex] = new MapValue(key, keyField.DataField.ClrType, value, valueField.DataField.ClrType);

                rowIndex++;
                progress?.Report(1);

                if (rowIndex - rowBeginIndex >= readRecords)
                    break;
            }
        }

        private static object ReadParquetValue(object? parquetValue, Parquet.Schema.DataField field, Parquet.Thrift.LogicalType logicalType)
        {
            if (parquetValue is null)
                return DBNull.Value;
            else if (field.DataType == Parquet.Schema.DataType.DateTimeOffset)
                return (DateTime)parquetValue;
            else if (field.DataType == Parquet.Schema.DataType.Int64
                && logicalType?.TIMESTAMP != null)
            {
                int divideBy = 0;
                if (logicalType.TIMESTAMP.Unit.NANOS != null)
                    divideBy = 1000 * 1000;
                else if (logicalType.TIMESTAMP.Unit.MICROS != null)
                    divideBy = 1000;
                else if (logicalType.TIMESTAMP.Unit.MILLIS != null)
                    divideBy = 1;

                if (divideBy > 0)
                    return DateTimeOffset.FromUnixTimeMilliseconds((long)parquetValue / divideBy).DateTime;
                else //Not sure if this 'else' is correct but adding just in case
                    return DateTimeOffset.FromUnixTimeSeconds((long)parquetValue);
            }
            else
                return parquetValue;
        }

        private static Type ParquetNetTypeToCSharpType(Parquet.Thrift.SchemaElement thriftSchema, Parquet.Schema.DataType type)
        {
            Type columnType;
            switch (type)
            {
                case Parquet.Schema.DataType.Boolean:
                    columnType = typeof(bool);
                    break;
                case Parquet.Schema.DataType.Byte:
                    columnType = typeof(sbyte);
                    break;
                case Parquet.Schema.DataType.ByteArray:
                    columnType = typeof(sbyte[]);
                    break;
                case Parquet.Schema.DataType.DateTimeOffset:
                    //Let's treat DateTimeOffsets as DateTime
                    columnType = typeof(DateTime);
                    break;
                case Parquet.Schema.DataType.Decimal:
                    columnType = typeof(decimal);
                    break;
                case Parquet.Schema.DataType.Double:
                    columnType = typeof(double);
                    break;
                case Parquet.Schema.DataType.Float:
                    columnType = typeof(float);
                    break;
                case Parquet.Schema.DataType.Int16:
                case Parquet.Schema.DataType.Int32:
                    columnType = typeof(int);
                    break;
                case Parquet.Schema.DataType.UnsignedInt16:
                case Parquet.Schema.DataType.UnsignedInt32:
                    columnType = typeof(uint);
                    break;
                case Parquet.Schema.DataType.Int64:
                    columnType = thriftSchema.LogicalType?.TIMESTAMP != null ? typeof(DateTime) : typeof(long);
                    break;
                case Parquet.Schema.DataType.SignedByte:
                    columnType = typeof(byte);
                    break;
                case Parquet.Schema.DataType.String:
                default:
                    columnType = typeof(string);
                    break;
            }

            return columnType;
        }

        private DataTable BuildDataTable(List<string> fields)
        {
            DataTable dataTable = new();
            foreach (var field in fields)
            {
                var schema = ParquetSchemaTree.GetChildByName(field);

                DataColumn newColumn;
                if (schema.SchemaElement.Converted_type == Parquet.Thrift.ConvertedType.LIST)
                {
                    newColumn = new DataColumn(field, typeof(ListValue));
                }
                else if (schema.SchemaElement.Converted_type == Parquet.Thrift.ConvertedType.MAP)
                {
                    newColumn = new DataColumn(field, typeof(MapValue));
                }
                else
                {
                    var clrType = ParquetNetTypeToCSharpType(schema.SchemaElement, schema.DataField.DataType);
                    newColumn = new DataColumn(field, clrType);
                }

                //We don't support case sensitive field names unfortunately
                if (dataTable.Columns.Contains(newColumn.ColumnName))
                {
                    throw new NotSupportedException($"Duplicate column '{field}' detected. Column names are case insensitive and must be unique.");
                }

                dataTable.Columns.Add(newColumn);
            }
            return dataTable;
        }
    }
}
