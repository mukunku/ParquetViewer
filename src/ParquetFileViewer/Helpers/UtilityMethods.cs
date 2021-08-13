using Parquet;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;

namespace ParquetFileViewer.Helpers
{
    public static class UtilityMethods
    {
        public static DataTable ParquetReaderToDataTable(ParquetReader parquetReader, List<string> selectedFields, int offset, int recordCount, CancellationToken cancellationToken)
        {
            //Get list of data fields and construct the DataTable
            DataTable dataTable = new DataTable();
            var fields = new List<(Parquet.Thrift.SchemaElement, Parquet.Data.DataField)>();
            var dataFields = parquetReader.Schema.GetDataFields();
            foreach (string selectedField in selectedFields)
            {
                var dataField = dataFields.FirstOrDefault(f => f.Name.Equals(selectedField, StringComparison.InvariantCultureIgnoreCase));
                if (dataField != null)
                {
                    var thriftSchema = parquetReader.ThriftMetadata.Schema.First(f => f.Name.Equals(selectedField, StringComparison.InvariantCultureIgnoreCase));

                    fields.Add((thriftSchema, dataField));
                    DataColumn newColumn = new DataColumn(dataField.Name, ParquetNetTypeToCSharpType(thriftSchema, dataField.DataType));
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
                if (cancellationToken.IsCancellationRequested)
                    break;

                using (ParquetRowGroupReader groupReader = parquetReader.OpenRowGroupReader(i))
                {
                    if (groupReader.RowCount > int.MaxValue)
                        throw new ArgumentOutOfRangeException(string.Format("Cannot handle row group sizes greater than {0}", groupReader.RowCount));

                    int rowsPassedUntilThisRowGroup = totalRecordCountSoFar;
                    totalRecordCountSoFar += (int)groupReader.RowCount;

                    if (offset >= totalRecordCountSoFar)
                        continue;

                    if (rowsLeftToRead > 0)
                    {
                        int numberOfRecordsToReadFromThisRowGroup = Math.Min(Math.Min(totalRecordCountSoFar - offset, rowsLeftToRead), (int)groupReader.RowCount);
                        rowsLeftToRead -= numberOfRecordsToReadFromThisRowGroup;

                        int recordsToSkipInThisRowGroup = Math.Max(offset - rowsPassedUntilThisRowGroup, 0);

                        ProcessRowGroup(dataTable, groupReader, fields, recordsToSkipInThisRowGroup, numberOfRecordsToReadFromThisRowGroup, cancellationToken);
                    }
                    else
                        break;
                }
            }

            return dataTable;
        }

        private static void ProcessRowGroup(DataTable dataTable, ParquetRowGroupReader groupReader, List<(Parquet.Thrift.SchemaElement, Parquet.Data.DataField)> fields,
            int skipRecords, int readRecords, CancellationToken cancellationToken)
        {
            int rowBeginIndex = dataTable.Rows.Count;
            bool isFirstColumn = true;

            foreach (var fieldTuple in fields)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                var logicalType = fieldTuple.Item1.LogicalType;
                var field = fieldTuple.Item2;

                int rowIndex = rowBeginIndex;

                int skippedRecords = 0;
                foreach (var value in groupReader.ReadColumn(field).Data)
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;

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
                    else if (field.DataType == Parquet.Data.DataType.DateTimeOffset)
                        dataTable.Rows[rowIndex][field.Name] = ((DateTimeOffset)value).DateTime; 
                    else if (field.DataType == Parquet.Data.DataType.Int64
                        && logicalType.TIMESTAMP != null)
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


        public static Type ParquetNetTypeToCSharpType(Parquet.Thrift.SchemaElement thriftSchema, Parquet.Data.DataType type)
        {
            Type columnType = null;
            switch (type)
            {
                case Parquet.Data.DataType.Boolean:
                    columnType = typeof(bool);
                    break;
                case Parquet.Data.DataType.Byte:
                    columnType = typeof(sbyte);
                    break;
                case Parquet.Data.DataType.ByteArray:
                    columnType = typeof(sbyte[]);
                    break;
                case Parquet.Data.DataType.DateTimeOffset:
                    //Let's treat DateTimeOffsets as DateTime
                    columnType = typeof(DateTime);
                    break;
                case Parquet.Data.DataType.Decimal:
                    columnType = typeof(decimal);
                    break;
                case Parquet.Data.DataType.Double:
                    columnType = typeof(double);
                    break;
                case Parquet.Data.DataType.Float:
                    columnType = typeof(float);
                    break;
                case Parquet.Data.DataType.Short:
                case Parquet.Data.DataType.Int16:
                case Parquet.Data.DataType.Int32:
                case Parquet.Data.DataType.UnsignedInt16:
                    columnType = typeof(int);
                    break;
                case Parquet.Data.DataType.Int64:
                    columnType = thriftSchema.LogicalType.TIMESTAMP != null ? typeof(DateTime) : typeof(long);
                    break;
                case Parquet.Data.DataType.UnsignedByte:
                    columnType = typeof(byte);
                    break;
                case Parquet.Data.DataType.String:
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
