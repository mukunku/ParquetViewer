using System.Data;
using System.Text.Json;

namespace ParquetViewer.Engine
{
    public class StructValue
    {
        public string Name { get; }
        public DataRow Data { get; }

        public StructValue(string name, DataRow data)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            if (data is null)
                throw new ArgumentNullException(nameof(data));

            Name = name;
            Data = data;
        }

        public override string ToString() => DataTableToJSONWithJavaScriptSerializer(this.Data, false);

        public string ToStringTruncated() => DataTableToJSONWithJavaScriptSerializer(this.Data, true);

        private string DataTableToJSONWithJavaScriptSerializer(DataRow dataRow, bool truncateForDisplay)
        {
            try
            {
                var record = new Dictionary<string, object>();
                foreach (DataColumn col in dataRow.Table.Columns)
                {
                    string columnName = col.ColumnName.Replace($"{this.Name}/", string.Empty); //Remove the parent field name from columns when rendering the data as json in the gridview cell.

                    var value = dataRow[col];

                    if (truncateForDisplay)
                    {
                        if (value is ByteArrayValue byteArrayValue)
                        {
                            var byteArrayAsString = byteArrayValue.ToString();
                            if (byteArrayAsString.Length > 64) //arbitrary number to give us a larger string to work with
                            {
                                value = $"{byteArrayAsString[..12]}[...]{byteArrayAsString.Substring(byteArrayAsString.Length - 8, 8)}";
                            }
                            else
                            {
                                value = byteArrayAsString;
                            }
                        }
                    }

                    record.Add(columnName, value);
                }

                return JsonSerializer.Serialize(record,
                     new JsonSerializerOptions()
                     {
                         Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping, //don't escape anything to make it human readable
                         WriteIndented = false //keep it minimized.
                     });
            }
            catch (Exception ex)
            {
                return $"Error while deserializing field: {Environment.NewLine}{Environment.NewLine}{ex}";
            }
        }

    }
}
