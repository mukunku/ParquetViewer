using System;
using System.Collections.Generic;
using System.Data;

namespace ParquetFileViewer
{
    public static class UtilityMethods
    {
        public static DataTable ParquetDataSetToDataTable(Parquet.Data.DataSet dataset)
        {
            DataTable datatable = new DataTable();
            if (dataset != null)
            {
                if (dataset.Schema.GetDataFields().Count == dataset.FieldCount)
                {
                    List<int> datetimeOffsetFieldIndexes = new List<int>();
                    int index = 0;
                    foreach (Parquet.Data.DataField field in dataset.Schema.GetDataFields())
                    {
                        Type columnType = null;
                        switch (field.DataType)
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
                                datetimeOffsetFieldIndexes.Add(index);
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

                        DataColumn newColumn = datatable.Columns.Add(field.Name, columnType);
                        newColumn.AllowDBNull = field.HasNulls;
                        index++;
                    }

                    foreach (Parquet.Data.Row row in dataset)
                    {
                        DataRow dataRow = datatable.NewRow();
                        object[] rawValues = row.RawValues;

                        //Convert DateTimeOffsets to DateTime
                        foreach (int datetimeOffsetIndex in datetimeOffsetFieldIndexes)
                        {
                            if (rawValues[datetimeOffsetIndex] != null)
                                rawValues[datetimeOffsetIndex] = ((DateTimeOffset)rawValues[datetimeOffsetIndex]).DateTime; //the DateTime property ignores the Offset value. Is there any instance where a parquet file can have an offset?
                        }

                        dataRow.ItemArray = rawValues;

                        datatable.Rows.Add(dataRow);
                    }
                }
                else
                    throw new ArgumentException("The provided dataset has some unsupported data types such as Lists, Maps or Structs");
            }
            return datatable;
        }

        public static List<string> GetDataTableColumns(DataTable datatable)
        {
            List<string> columns = new List<string>(datatable != null ? datatable.Columns.Count : 0);
            if (datatable != null)
            {
                foreach(DataColumn column in datatable.Columns)
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
