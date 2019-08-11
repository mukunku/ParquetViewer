using Parquet;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace ParquetFileViewer
{
    public static class UtilityMethods
    {
        public static DataTable ParquetReaderToDataTable(ParquetReader parquetReader, out int totalRecordCount, List<string> selectedFields, int offset, int recordCount)
        {
            //Get list of data fields and construct the DataTable
            DataTable dataTable = new DataTable();
            List<Parquet.Data.DataField> fields = new List<Parquet.Data.DataField>();
            var dataFields = parquetReader.Schema.GetDataFields();
            foreach (string selectedField in selectedFields)
            {
                var dataField = dataFields.FirstOrDefault(f => f.Name.Equals(selectedField, StringComparison.InvariantCultureIgnoreCase));
                if (dataField != null)
                {
                    fields.Add(dataField);
                    DataColumn newColumn = new DataColumn(dataField.Name, ParquetNetTypeToCSharpType(dataField.DataType))
                    {
                        AllowDBNull = dataField.HasNulls
                    };
                    dataTable.Columns.Add(newColumn);
                }
                else
                    throw new Exception(string.Format("Field '{0}' does not exist", selectedField));
            }

            //Read column by column to generate each row in the datatable
            totalRecordCount = 0;
            for (int i = 0; i < parquetReader.RowGroupCount; i++)
            {
                int rowsLeftToRead = recordCount;
                using (ParquetRowGroupReader groupReader = parquetReader.OpenRowGroupReader(i))
                {
                    if (groupReader.RowCount > int.MaxValue)
                        throw new ArgumentOutOfRangeException(string.Format("Cannot handle row group sizes greater than {0}", groupReader.RowCount));

                    int rowsPassedUntilThisRowGroup = totalRecordCount;
                    totalRecordCount += (int)groupReader.RowCount;

                    if (offset >= totalRecordCount)
                        continue;

                    if (rowsLeftToRead > 0)
                    {
                        int recordsToSkipInThisRowGroup = Math.Max(offset - rowsPassedUntilThisRowGroup, 0);

                        int numberOfRecordsToReadFromThisRowGroup = Math.Min(Math.Min(totalRecordCount - offset, recordCount), (int)groupReader.RowCount);
                        rowsLeftToRead -= numberOfRecordsToReadFromThisRowGroup;

                        ProcessRowGroup(dataTable, groupReader, fields, recordsToSkipInThisRowGroup, numberOfRecordsToReadFromThisRowGroup);
                    }
                }
            }

            return dataTable;
        }

        private static void ProcessRowGroup(DataTable dataTable, ParquetRowGroupReader groupReader, List<Parquet.Data.DataField> fields, int skipRecords, int readRecords)
        {
            int rowBeginIndex = dataTable.Rows.Count;
            bool isFirstColumn = true;

            foreach (var field in fields)
            {
                int rowIndex = rowBeginIndex;

                int skippedRecords = 0;
                foreach (var value in groupReader.ReadColumn(field).Data)
                {
                    if (skipRecords > skippedRecords)
                    {
                        skippedRecords++;
                        continue;
                    }

                    //int newRowsAdded = dataTable.Rows.Count - rowBeginIndex;
                    if (rowIndex >= readRecords)
                        break;

                    if (isFirstColumn)
                    {
                        var newRow = dataTable.NewRow();
                        dataTable.Rows.Add(newRow);
                    }

                    if (field.DataType == Parquet.Data.DataType.DateTimeOffset)
                        dataTable.Rows[rowIndex][field.Name] = ((DateTimeOffset)value).DateTime; //converts to local time!
                    else
                        dataTable.Rows[rowIndex][field.Name] = value;

                    rowIndex++;
                }

                isFirstColumn = false;
            }
        }


        public static Type ParquetNetTypeToCSharpType(Parquet.Data.DataType type)
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
                    columnType = typeof(long);
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

        public static List<string> GetDataTableColumns(DataTable datatable)
        {
            List<string> columns = new List<string>(datatable != null ? datatable.Columns.Count : 0);
            if (datatable != null)
            {
                foreach (DataColumn column in datatable.Columns)
                {
                    columns.Add(column.ColumnName);
                }
            }
            return columns;
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
    }
}
