using Parquet;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ParquetFileViewer.Helpers
{
    public static class UtilityMethods
    {
        public static async Task<DataTable> ParquetReaderToDataTable(ParquetReader parquetReader, List<string> selectedFields, int offset, int recordCount, CancellationToken cancellationToken)
        {
            //Get list of data fields and construct the DataTable
            var dataTable = new DataTable();
            var fields = new List<(Parquet.Thrift.SchemaElement, Parquet.Schema.DataField)>();
            var dataFields = parquetReader.Schema.GetDataFields();
            foreach (string selectedField in selectedFields)
            {
                var dataField = dataFields.FirstOrDefault(f => f.Name.Equals(selectedField, StringComparison.InvariantCultureIgnoreCase));
                if (dataField != null)
                {
                    var thriftSchema = parquetReader.ThriftMetadata.Schema.First(f => f.Name.Equals(selectedField, StringComparison.InvariantCultureIgnoreCase));

                    fields.Add((thriftSchema, dataField));
                    DataColumn newColumn = new DataColumn(dataField.Name, ParquetNetTypeToCSharpType(thriftSchema, dataField.DataType));

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

            //Read column by column to generate each row in the datatable
            int totalRecordCountSoFar = 0;
            int rowsLeftToRead = recordCount;
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

                    if (rowsLeftToRead > 0)
                    {
                        int numberOfRecordsToReadFromThisRowGroup = Math.Min(Math.Min(totalRecordCountSoFar - offset, rowsLeftToRead), (int)groupReader.RowCount);
                        rowsLeftToRead -= numberOfRecordsToReadFromThisRowGroup;

                        int recordsToSkipInThisRowGroup = Math.Max(offset - rowsPassedUntilThisRowGroup, 0);

                        await ProcessRowGroup(dataTable, groupReader, fields, recordsToSkipInThisRowGroup, numberOfRecordsToReadFromThisRowGroup, cancellationToken);
                    }
                    else
                        break;
                }
            }

            return dataTable;
        }

        private static async Task ProcessRowGroup(DataTable dataTable, ParquetRowGroupReader groupReader, List<(Parquet.Thrift.SchemaElement, Parquet.Schema.DataField)> fields,
            int skipRecords, int readRecords, CancellationToken cancellationToken)
        {
            int rowBeginIndex = dataTable.Rows.Count;
            bool isFirstColumn = true;

            foreach (var fieldTuple in fields)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var logicalType = fieldTuple.Item1.LogicalType;
                var field = fieldTuple.Item2;

                int rowIndex = rowBeginIndex;

                int skippedRecords = 0;
                foreach (var value in (await groupReader.ReadColumnAsync(field)).Data)
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

                    if (value == null)
                        dataTable.Rows[rowIndex][field.Name] = DBNull.Value;
                    else if (field.DataType == Parquet.Schema.DataType.DateTimeOffset)
                        dataTable.Rows[rowIndex][field.Name] = (DateTime)value; 
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
                            dataTable.Rows[rowIndex][field.Name] = DateTimeOffset.FromUnixTimeMilliseconds((long)value / divideBy).DateTime;
                        else //Not sure if this 'else' is correct but adding just in case
                            dataTable.Rows[rowIndex][field.Name] = DateTimeOffset.FromUnixTimeSeconds((long)value);
                    }
                    else
                        dataTable.Rows[rowIndex][field.Name] = value;

                    rowIndex++;
                }

                isFirstColumn = false;
            }
        }

        public static Type ParquetNetTypeToCSharpType(Parquet.Thrift.SchemaElement thriftSchema, Parquet.Schema.DataType type)
        {
            Type columnType = null;
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
                case Parquet.Schema.DataType.SignedByte: //Should this be unsigned byte? (https://github.com/aloneguid/parquet-dotnet/issues/244)
                    columnType = typeof(byte);
                    break;
                case Parquet.Schema.DataType.String:
                default:
                    columnType = typeof(string);
                    break;
            }

            return columnType;
        }

        public static string CleanCSVValue(string value, bool alwaysEncloseInQuotes = false)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                //In RFC 4180 we escape quotes with double quotes
                string formattedValue = value.Replace("\"", "\"\"");

                //Enclose value with quotes if it contains commas,line feeds or other quotes
                if (formattedValue.Contains(",") || formattedValue.Contains("\r") || formattedValue.Contains("\n") || formattedValue.Contains("\"\"") || alwaysEncloseInQuotes)
                    formattedValue = string.Concat("\"", formattedValue, "\"");

                return formattedValue;
            }
            else
                return string.Empty;
        }

        public static IEnumerable<ICollection<T>> Split<T>(IEnumerable<T> src, int maxItems)
        {
            var list = new List<T>();
            foreach (var t in src)
            {
                list.Add(t);
                if (list.Count == maxItems)
                {
                    yield return list;
                    list = new List<T>();
                }
            }

            if (list.Count > 0)

                yield return list;
        }

        public static DataTable MergeTables(IEnumerable<DataTable> additionalTables)
        {
            // Build combined table columns
            DataTable merged = null;
            foreach (DataTable dt in additionalTables)
            {
                if (merged == null)
                    merged = dt;
                else
                    merged = AddTable(merged, dt);
            }
            return merged ?? new DataTable();
        }

        private static DataTable AddTable(DataTable baseTable, DataTable additionalTable)
        {
            // Build combined table columns
            DataTable merged = baseTable.Clone(); // Include all columns from base table in result.
            foreach (DataColumn col in additionalTable.Columns)
            {
                string newColumnName = col.ColumnName;
                merged.Columns.Add(newColumnName, col.DataType);
            }
            // Add all rows from both tables
            var bt = baseTable.AsEnumerable();
            var at = additionalTable.AsEnumerable();
            var mergedRows = bt.Zip(at, (r1, r2) => r1.ItemArray.Concat(r2.ItemArray).ToArray());
            foreach (object[] rowFields in mergedRows)
            {
                merged.Rows.Add(rowFields);
            }
            return merged;
        }
    }
}
