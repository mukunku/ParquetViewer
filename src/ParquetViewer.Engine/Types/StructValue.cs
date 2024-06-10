using System.Data;
using System.Numerics;
using System.Text.Json;

namespace ParquetViewer.Engine.Types
{
    public class StructValue : IComparable<StructValue>, IComparable
    {
        public string Name { get; }

        public DataRow Data { get; }

        public static string? DateDisplayFormat { get; set; }

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
                        WriteValue(jsonWriter, value, truncateForDisplay);
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

        public static void WriteValue(Utf8JsonWriter jsonWriter, object value, bool truncateForDisplay)
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
                WriteValue(jsonWriter, map.Key, truncateForDisplay);
                jsonWriter.WritePropertyName("value");
                WriteValue(jsonWriter, map.Value, truncateForDisplay);
                jsonWriter.WriteEndObject();
            }
            else if (value is ListValue list)
            {
                //Hopefully lists also generate valid JSON on .ToString()
                jsonWriter.WriteRawValue(list.ToString());
            }
            else if (value is ByteArrayValue byteArray)
            {
                var byteArrayAsString = byteArray.ToString();
                if (truncateForDisplay && byteArrayAsString.Length > 64) //arbitrary number to give us a larger string to work with
                {
                    byteArrayAsString = $"{byteArrayAsString[..12]}[...]{byteArrayAsString.Substring(byteArrayAsString.Length - 8, 8)}";
                }
                jsonWriter.WriteStringValue(byteArrayAsString);
            }
            else if (value is DateTime dt)
            {
                //Write dates as string
                if (DateDisplayFormat is not null)
                    jsonWriter.WriteStringValue(dt.ToString(DateDisplayFormat));
                else
                    jsonWriter.WriteStringValue(dt.ToString());
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

        private IReadOnlyCollection<string>? _columnNames = null;
        private IReadOnlyCollection<string> GetFieldNames() =>
            _columnNames ??= GetColumns(Data).Select(c => c.ColumnName).ToList().AsReadOnly();

        /// <summary>
        /// Sorts by field names first, then by values
        /// </summary>
        public int CompareTo(StructValue? other)
        {
            if (other?.Data is null || other.GetFieldNames().Count == 0)
                return 1;

            if (Data is null || GetFieldNames().Count == 0)
                return -1;

            var otherColumnNames = string.Join("|", other.GetFieldNames());
            var columnNames = string.Join("|", this.GetFieldNames());

            int schemaComparison = columnNames.CompareTo(otherColumnNames);
            if (schemaComparison != 0)
                return schemaComparison;

            int fieldCount = GetFieldNames().Count;
            for (var i = 0; i < fieldCount; i++)
            {
                var otherValue = other.Data[i];
                var value = Data[i];
                int comparison = Helpers.CompareTo(value, otherValue);
                if (comparison != 0)
                    return comparison;
            }

            return 0; //Both structs appear equal
        }

        private static IEnumerable<DataColumn> GetColumns(DataRow dataRow)
        {
            foreach (DataColumn column in dataRow.Table.Columns)
            {
                yield return column;
            }
        }

        public int CompareTo(object? obj)
        {
            if (obj is StructValue @struct)
                return CompareTo(@struct);
            else
                return 1;
        }
    }
}
