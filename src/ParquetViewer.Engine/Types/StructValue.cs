using System.Data;
using System.Text.Json;

namespace ParquetViewer.Engine.Types
{
    public class StructValue
    {
        public string Name { get; }

        //we are always guaranteed to have exactly one row in 'Data' since we don't allow nested structs right now
        public DataRow Data { get; }

        public StructValue(string name, DataRow data)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Data = data ?? throw new ArgumentNullException(nameof(data));
        }

        public override string ToString() => ToJSONWithJavaScriptSerializer(false);

        public string ToStringTruncated() => ToJSONWithJavaScriptSerializer(true);

        private string ToJSONWithJavaScriptSerializer(bool truncateForDisplay)
        {
            try
            {
                using var ms = new MemoryStream();
                using (var jsonWriter = new Utf8JsonWriter(ms))
                {
                    jsonWriter.WriteStartObject();
                    for (var i = 0; i < this.Data.Table.Columns.Count; i++)
                    {
                        string columnName = this.Data.Table.Columns[i].ColumnName.Replace($"{this.Name}/", string.Empty); //Remove the parent field name from columns when rendering the data as json in the gridview cell.
                        jsonWriter.WritePropertyName(columnName);

                        object value = this.Data[i];
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

                        if (value == DBNull.Value)
                        {
                            jsonWriter.WriteNullValue();
                        }
                        else
                        {
                            jsonWriter.WriteRawValue(value.ToString() ?? string.Empty);
                        }
                    }
                    jsonWriter.WriteEndObject();
                }

                ms.Position = 0;
                using var reader = new StreamReader(ms);
                return reader.ReadToEnd();
            }
            catch (Exception ex)
            {
                return $"Error while deserializing field: {Environment.NewLine}{Environment.NewLine}{ex}";
            }
        }
    }
}
