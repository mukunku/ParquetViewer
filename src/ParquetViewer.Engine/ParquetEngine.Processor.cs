using Parquet;
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
            PerfWatch.Milestone(nameof(ReadRowsAsync));

            long recordsLeftToRead = recordCount;
            DataTable dataTable = null;
            var fields = new List<(Parquet.Thrift.SchemaElement, Parquet.Schema.DataField)>();
            foreach (var reader in this.GetReaders(offset))
            {
                cancellationToken.ThrowIfCancellationRequested();

                //Build datatable once
                if (dataTable is null)
                {
                    PerfWatch.Milestone(nameof(dataTable));

                    dataTable = new DataTable();
                    var dataFieldsDictionary = reader.ParquetReader.Schema.GetDataFields().ToDictionary(df => df.Name, df => df);
                    var thriftSchemaDictionary = reader.ParquetReader.ThriftMetadata.Schema.ToDictionary(f => f.Name, f => f);
                    foreach (string selectedField in selectedFields)
                    {
                        dataFieldsDictionary.TryGetValue(selectedField, out var dataField);
                        if (dataField != null)
                        {
                            var thriftSchema = thriftSchemaDictionary[selectedField];

                            fields.Add((thriftSchema, dataField));
                            var newColumn = new DataColumn(dataField.Name, ParquetNetTypeToCSharpType(thriftSchema, dataField.DataType));

                            //We don't support case sensitive field names unfortunately
                            if (dataTable.Columns.Contains(newColumn.ColumnName))
                            {
                                throw new NotSupportedException("Duplicate column detected. Column names are case insensitive and must be unique.");
                            }

                            dataTable.Columns.Add(newColumn);
                        }
                        else
                            throw new Exception(string.Format("Field '{0}' does not exist", selectedField));
                    }

                    dataTable.BeginLoadData(); //might help boost performance a bit
                    PerfWatch.Milestone(nameof(dataTable));
                }

                if (recordsLeftToRead <= 0)
                    break;

                recordsLeftToRead = await ParquetReaderToDataTable(dataTable, fields, reader.ParquetReader, reader.RemainingOffset, recordsLeftToRead, cancellationToken, progress);
            }

            var result = dataTable ?? new();
            result.ExtendedProperties.Add(TotalRecordCountExtendedPropertyKey, this.RecordCount);
            result.EndLoadData();

            PerfWatch.Milestone(nameof(ReadRowsAsync));
            return result;
        }

        private static async Task<long> ParquetReaderToDataTable(DataTable dataTable, List<(Parquet.Thrift.SchemaElement, Parquet.Schema.DataField)> fields, ParquetReader parquetReader,
            long offset, long recordCount, CancellationToken cancellationToken, IProgress<int>? progress)
        {
            PerfWatch.Milestone(nameof(ParquetReaderToDataTable));

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

                    await ProcessRowGroup(dataTable, groupReader, fields, recordsToSkipInThisRowGroup, numberOfRecordsToReadFromThisRowGroup, cancellationToken, progress);
                }
            }

            PerfWatch.Milestone(nameof(ParquetReaderToDataTable));
            return rowsLeftToRead;
        }

        private static async Task ProcessRowGroup(DataTable dataTable, ParquetRowGroupReader groupReader, List<(Parquet.Thrift.SchemaElement, Parquet.Schema.DataField)> fields,
            long skipRecords, long readRecords, CancellationToken cancellationToken, IProgress<int>? progress)
        {
            PerfWatch.Milestone(nameof(ProcessRowGroup));

            int rowBeginIndex = dataTable.Rows.Count;
            bool isFirstColumn = true;

            var rowLookupCache = new Dictionary<int, DataRow>();
            foreach (var fieldTuple in fields)
            {
                PerfWatch.Milestone(fieldTuple.Item2.Name, "START");

                cancellationToken.ThrowIfCancellationRequested();

                var logicalType = fieldTuple.Item1.LogicalType;
                var field = fieldTuple.Item2;

                int rowIndex = rowBeginIndex;

                int skippedRecords = 0;
                Parquet.Data.DataColumn dataColumn = null;
                try
                {
                    dataColumn = await groupReader.ReadColumnAsync(field, cancellationToken);
                    PerfWatch.Milestone(fieldTuple.Item2.Name, "LOAD");
                }
                catch(Exception)
                {
                    throw;
                }

                var fieldIndex = dataTable.Columns[field.Name].Ordinal;
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
                            rowLookupCache.TryAdd(rowIndex, dataTable.Rows[rowIndex]);
                        }
                    }

                    if (value is null)
                        datarow[fieldIndex] = DBNull.Value;
                    else if (field.DataType == Parquet.Schema.DataType.DateTimeOffset)
                        datarow[fieldIndex] = (DateTime)value;
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
                            datarow[fieldIndex] = DateTimeOffset.FromUnixTimeMilliseconds((long)value / divideBy).DateTime;
                        else //Not sure if this 'else' is correct but adding just in case
                            datarow[fieldIndex] = DateTimeOffset.FromUnixTimeSeconds((long)value);
                    }
                    else
                        datarow[fieldIndex] = value;

                    rowIndex++;
                    progress?.Report(1);
                    PerfWatch.Milestone(fieldTuple.Item2.Name, "READ");
                }

                isFirstColumn = false;
                PerfWatch.Milestone(nameof(ProcessRowGroup));
            }

            PerfWatch.Milestone(nameof(ProcessRowGroup));
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
    }
}
