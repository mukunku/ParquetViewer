using System.Data;
using System.Numerics;
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
                        WriteValue(jsonWriter, value);
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

        private static void WriteValue(Utf8JsonWriter jsonWriter, object value)
        {
            if (value is null)
            {
                //Value should never be null as we should be replacing all those with DBNull.Value
                throw new ArgumentNullException(nameof(value));
            }
            else if (value == DBNull.Value)
            {
                jsonWriter.WriteNullValue();
            }
            else if (value is string str)
            {
                jsonWriter.WriteStringValue(str);
            }
            else if (value is bool @bool)
            {
                jsonWriter.WriteBooleanValue(@bool);
            }
            else if (IsNumber(value.GetType()))
            {
                jsonWriter.WriteNumberValue(Convert.ToDecimal(value));
            }
            else if (value is StructValue @struct)
            {
                //Structs already generate JSON on .ToString()
                jsonWriter.WriteRawValue(@struct.ToString());
            }
            else if (value is MapValue map)
            {
                jsonWriter.WriteStartObject();
                jsonWriter.WritePropertyName("key");
                WriteValue(jsonWriter, map.Key);
                jsonWriter.WritePropertyName("value");
                WriteValue(jsonWriter, map.Value);
                jsonWriter.WriteEndObject();
            }
            else if (value is ListValue list)
            {
                //Hopefully lists also generate valid JSON on .ToString()
                jsonWriter.WriteRawValue(list.ToString());
            }
            else
            {
                //Everything else just try to write it raw
                jsonWriter.WriteRawValue(value.ToString()!);
            }
        }

        /// <summary>
        /// Returns true if the type is a number type.
        /// </summary>
        private static bool IsNumber(Type type) =>
            Array.Exists(type.GetInterfaces(), i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(INumber<>));
    }
}
