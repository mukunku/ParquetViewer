using Parquet;
using Parquet.Meta;
using ParquetViewer.Engine.Exceptions;
using System.Collections;
using System.Data;
using System.Reflection.Emit;

namespace ParquetViewer.Engine
{
    public partial class ParquetEngine
    {
        public static readonly string TotalRecordCountExtendedPropertyKey = "TOTAL_RECORD_COUNT";
        private static readonly int MinMemoryGBNeededForAdditionalCaching = 8; //Let's not use extra caching if the PC doesn't have more than 8GB memory

        private static bool? _useDataRowCache;
        private static bool UseDataRowCache =>
            _useDataRowCache ??= (GC.GetGCMemoryInfo().TotalAvailableMemoryBytes / 1048576.0 /*magic number*/) > (MinMemoryGBNeededForAdditionalCaching * 1024);

        private readonly Dictionary<DataTable, Dictionary<int, DataRow>> _rowLookupCache = new();

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

                recordsLeftToRead = await PopulateDataTable(result, reader.ParquetReader, reader.RemainingOffset, recordsLeftToRead, cancellationToken, progress);
            }

            result.ExtendedProperties.Add(TotalRecordCountExtendedPropertyKey, this.RecordCount);
            result.EndLoadData();

            return result;
        }

        private async Task<long> PopulateDataTable(DataTable dataTable, ParquetReader parquetReader,
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

            foreach (DataColumn column in dataTable.Columns)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var field = ParquetSchemaTree.GetChild(column.ColumnName);
                if (field.SchemaElement.LogicalType?.LIST is not null || field.SchemaElement.ConvertedType == Parquet.Meta.ConvertedType.LIST)
                {
                    await ReadListField(dataTable, groupReader, rowBeginIndex, field, skipRecords,
                        readRecords, isFirstColumn, cancellationToken, progress);
                }
                else if (field.SchemaElement.LogicalType?.MAP is not null || field.SchemaElement.ConvertedType == Parquet.Meta.ConvertedType.MAP)
                {
                    await ReadMapField(dataTable, groupReader, rowBeginIndex, field, skipRecords,
                        readRecords, isFirstColumn, cancellationToken, progress);
                }
                else if (field.SchemaElement.NumChildren > 0) //Struct
                {
                    await ReadStructField(dataTable, groupReader, rowBeginIndex, field, skipRecords,
                        readRecords, isFirstColumn, cancellationToken, progress);
                }
                else
                {
                    await ReadPrimitiveField(dataTable, groupReader, rowBeginIndex, field, skipRecords,
                        readRecords, isFirstColumn, cancellationToken, progress);
                }

                isFirstColumn = false;
            }
        }

        private async Task ReadPrimitiveField(DataTable dataTable, ParquetRowGroupReader groupReader, int rowBeginIndex, ParquetSchemaElement field,
        long skipRecords, long readRecords, bool isFirstColumn, CancellationToken cancellationToken, IProgress<int>? progress)
        {
            int rowIndex = rowBeginIndex;

            int skippedRecords = 0;
            var dataColumn = await groupReader.ReadColumnAsync(field.DataField ?? throw new Exception($"Pritimive field {field.Path} is missing its data field"), cancellationToken);

            var fieldIndex = dataTable.Columns[field.DataField.Path.ToString()]?.Ordinal ?? throw new Exception($"Column {field.Path} is missing");
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

                DataRow datarow = GetRow(dataTable, rowIndex);
                datarow[fieldIndex] = FixDateTime(value, field) ?? DBNull.Value;

                rowIndex++;
                progress?.Report(1);
            }
        }

        /// <summary>
        /// This is a patch fix to handle malformed datetime fields. We assume TIMESTAMP fields are DateTime values.
        /// </summary>
        /// <param name="value">Original value</param>
        /// <param name="field">Schema element</param>
        /// <returns>If the field is a timestamp, a DateTime object will be returned. Otherwise the value will not be changed.</returns>
        private async Task ReadListField(DataTable dataTable, ParquetRowGroupReader groupReader, int rowBeginIndex, ParquetSchemaElement field,
            long skipRecords, long readRecords, bool isFirstColumn, CancellationToken cancellationToken, IProgress<int>? progress)
        {
            var listField = field.GetChild("list");
            ParquetSchemaElement itemField;
            try
            {
                itemField = listField.GetImmediateChildOrSingle("item"); //Not all parquet files follow the same format so we're being lax with getting the child here
            }
            catch (Exception ex)
            {
                throw new UnsupportedFieldException($"Cannot load field '{field.Path}. Invalid List type.'", ex);
            }

            if (itemField.Children.Any())
                throw new UnsupportedFieldException($"Cannot load field '{field.Path}'. Nested list types are not supported");

            int rowIndex = rowBeginIndex;

            int skippedRecords = 0;
            var dataColumn = await groupReader.ReadColumnAsync(itemField.DataField!, cancellationToken);

            ArrayList? rowValue = null;
            var fieldIndex = dataTable.Columns[field.Path]!.Ordinal;
            for (int i = 0; i < dataColumn.Data.Length; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                rowValue ??= new ArrayList();

                bool IsEndOfRow() => (i + 1) == dataColumn.RepetitionLevels!.Length
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

                    DataRow datarow = GetRow(dataTable, rowIndex);

                    var lastItem = dataColumn.Data.GetValue(i) ?? DBNull.Value;
                    rowValue.Add(lastItem);

                    datarow[fieldIndex] = new ListValue(rowValue, itemField.DataField!.ClrType);
                    rowValue = null;

                    rowIndex++;
                    progress?.Report(1);

                    if (rowIndex - rowBeginIndex >= readRecords)
                        break;
                }
                else
                {
                    var value = dataColumn.Data.GetValue(i) ?? DBNull.Value;
                    rowValue.Add(value);
                }
            }
        }

        private async Task ReadMapField(DataTable dataTable, ParquetRowGroupReader groupReader, int rowBeginIndex, ParquetSchemaElement field,
            long skipRecords, long readRecords, bool isFirstColumn, CancellationToken cancellationToken, IProgress<int>? progress)
        {
            var keyValueField = field.GetChild("key_value");
            var keyField = keyValueField.GetChild("key");
            var valueField = keyValueField.GetChild("value");

            if (keyField.Children.Any() || valueField.Children.Any())
                throw new UnsupportedFieldException($"Cannot load field '{field.Path}'. Nested map types are not supported");

            int rowIndex = rowBeginIndex;

            int skippedRecords = 0;
            var keyDataColumn = await groupReader.ReadColumnAsync(keyField.DataField!, cancellationToken);
            var valueDataColumn = await groupReader.ReadColumnAsync(valueField.DataField!, cancellationToken);

            var fieldIndex = dataTable.Columns[field.Path]!.Ordinal;
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

                DataRow datarow = GetRow(dataTable, rowIndex);

                bool isMapTypeValid = keyDataColumn.Data.Length == valueDataColumn.Data.Length;
                if (!isMapTypeValid)
                    throw new UnsupportedFieldException($"{field.Path} is malformed and cannot be loaded");

                var key = keyDataColumn.Data.GetValue(i) ?? DBNull.Value;
                var value = valueDataColumn.Data.GetValue(i) ?? DBNull.Value;
                datarow[fieldIndex] = new MapValue(key, keyField.DataField!.ClrType, value, valueField.DataField!.ClrType);

                rowIndex++;
                progress?.Report(1);

                if (rowIndex - rowBeginIndex >= readRecords)
                    break;
            }
        }

        private async Task ReadStructField(DataTable dataTable, ParquetRowGroupReader groupReader, int rowBeginIndex, ParquetSchemaElement field,
           long skipRecords, long readRecords, bool isFirstColumn, CancellationToken cancellationToken, IProgress<int>? progress)
        {
            //Read struct data as a new datatable
            DataTable structFieldDataTable = BuildDataTable(field.Children.Select(f => $"{field.Path}/{f.Path}").ToList());

            //Need to calculate progress differently for structs
            var structFieldReadProgress = new SimpleProgress();
            structFieldReadProgress.ProgressChanged += (int progressSoFar) =>
            {
                if (structFieldDataTable.Columns.Count > 0)
                {
                    //To report progress accurately we'll need to divide the progress total  
                    //by the field count to convert it to row count in the main data table.
                    var increment = progressSoFar % structFieldDataTable.Columns.Count;
                    if (increment == 0)
                        progress?.Report(1);
                }
                else
                {
                    //If the struct field has no columns then each read is for each row.
                    progress?.Report(1);
                }
            };

            //Read the struct data and populate the datatable
            await ProcessRowGroup(structFieldDataTable, groupReader, skipRecords, readRecords, cancellationToken, structFieldReadProgress);

            if (isFirstColumn)
            {
                var newRow = dataTable.NewRow();
                dataTable.Rows.Add(newRow);
            }

            var rowIndex = rowBeginIndex;
            var fieldIndex = dataTable.Columns[field.Path]?.Ordinal ?? throw new Exception($"Column {field.Path} is missing");
            for (var i = 0; i < structFieldDataTable.Rows.Count; i++)
            {
                DataRow datarow = GetRow(dataTable, rowIndex);
                datarow[fieldIndex] = new StructValue(field.Path, structFieldDataTable.Rows[i]);
                rowIndex++;
            }
        }

        private DataRow GetRow(DataTable dataTable, int rowIndex)
        {
            if (!UseDataRowCache)
            {
                //This is a costly call when tables have a lot of columns
                return dataTable.Rows[rowIndex];
            }
            else
            {
                if (!_rowLookupCache.TryGetValue(dataTable, out var lookupCache))
                {
                    lookupCache = new();
                    _rowLookupCache.Add(dataTable, lookupCache);
                }

                if (!lookupCache.TryGetValue(rowIndex, out var datarow))
                {
                    datarow = dataTable.Rows[rowIndex];
                    lookupCache.TryAdd(rowIndex, datarow);
                }

                return datarow;
            }
        }

        private DataTable BuildDataTable(List<string> fields)
        {
            DataTable dataTable = new();
            foreach (var field in fields)
            {
                var schema = ParquetSchemaTree.GetChild(field);

                DataColumn newColumn;
                if (schema.SchemaElement.ConvertedType == ConvertedType.LIST)
                {
                    newColumn = new DataColumn(field, typeof(ListValue));
                }
                else if (schema.SchemaElement.ConvertedType == ConvertedType.MAP)
                {
                    newColumn = new DataColumn(field, typeof(MapValue));
                }
                else if (this.FixMalformedDateTime
                    && schema.SchemaElement.LogicalType?.TIMESTAMP is not null
                    && schema.SchemaElement?.ConvertedType is null)
                {
                    //Fix for malformed datetime fields (#88)
                    newColumn = new DataColumn(field, typeof(DateTime));
                }
                else if (schema.SchemaElement.NumChildren > 0) //Struct
                {
                    newColumn = new DataColumn(field, typeof(StructValue));
                }
                else
                {
                    var clrType = schema.DataField?.ClrType ?? throw new Exception($"{field} has no data field");
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

        private object? FixDateTime(object value, ParquetSchemaElement field)
        {
            if (!this.FixMalformedDateTime || value is null)
                return value;

            var timestampSchema = field.SchemaElement?.LogicalType?.TIMESTAMP;
            if (timestampSchema is not null && field.SchemaElement?.ConvertedType is null)
            {
                long castValue;
                if (field.DataField?.ClrType == typeof(long?))
                {
                    castValue = ((long?)value).Value; //We know this isn't null from the null check above
                }
                else if (field.DataField?.ClrType == typeof(long))
                {
                    castValue = (long)value;
                }
                else
                {
                    throw new UnsupportedFieldException($"Field {field.Path} is not a valid timestamp field");
                }

                int divideBy = 0;
                if (timestampSchema.Unit.NANOS != null)
                    divideBy = 1000 * 1000;
                else if (timestampSchema.Unit.MICROS != null)
                    divideBy = 1000;
                else if (timestampSchema.Unit.MILLIS != null)
                    divideBy = 1;

                if (divideBy > 0)
                    value = DateTimeOffset.FromUnixTimeMilliseconds(castValue / divideBy).DateTime;
                else //Not sure if this 'else' is correct but adding just in case
                    value = DateTimeOffset.FromUnixTimeSeconds(castValue);
            }

            return value;
        }

        private class SimpleProgress : IProgress<int>
        {
            private int _progress = 0;
            public Action<int>? ProgressChanged;

            public void Report(int value)
            {
                _progress += value;
                ProgressChanged?.Invoke(_progress);
            }
        }
    }
}
