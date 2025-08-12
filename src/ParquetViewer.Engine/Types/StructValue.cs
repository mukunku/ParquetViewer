using System.Data;
using System.Numerics;

namespace ParquetViewer.Engine.Types
{
    public class StructValue : IComparable<StructValue>, IComparable
    {
        public string Name { get; }

        public DataRow Data { get; }

        public StructValue(string name, DataRow data)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Data = data ?? throw new ArgumentNullException(nameof(data));
        }

        public override string ToString() => ToJSON();

        public string ToStringTruncated(int desiredLength) => ToJSON(desiredLength);

        private string ToJSON(int? desiredLength = null)
        {
            try
            {
                bool isTruncated = false;
                using var ms = new MemoryStream();
                using (var jsonWriter = new Utf8JsonWriterWithRunningLength(ms))
                {
                    jsonWriter.WriteStartObject();
                    for (var i = 0; i < this.Data.Table.Columns.Count; i++)
                    {
                        string columnName = this.Data.Table.Columns[i].ColumnName
                            //Remove the parent field name from columns when rendering the data as json in the gridview cell.
                            .Replace($"{this.Name}/", string.Empty);
                        jsonWriter.WritePropertyName(columnName);

                        object value = this.Data[i];
                        WriteValue(jsonWriter, value, desiredLength is not null);

                        if (desiredLength > 0 && jsonWriter.ApproximateStringLengthSoFar > desiredLength)
                        {
                            isTruncated = true;
                            break;
                        }
                    }
                    if (!isTruncated)
                        jsonWriter.WriteEndObject();
                }

                ms.Position = 0;
                using var reader = new StreamReader(ms);
                var json = reader.ReadToEnd();
                if (isTruncated)
                {
                    json += "[...]";
                }
                return json;
            }
            catch (Exception ex)
            {
                return $"Error while deserializing field '{Name}': {Environment.NewLine}{Environment.NewLine}{ex}";
            }
        }

        public static void WriteValue(Utf8JsonWriterWithRunningLength jsonWriter, object value, bool truncateForDisplay)
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
                jsonWriter.WriteStartArray();
                foreach ((object mapKey, object mapValue) in map)
                {
                    jsonWriter.WriteStartObject();
                    jsonWriter.WritePropertyName("key");
                    WriteValue(jsonWriter, mapKey, truncateForDisplay);
                    jsonWriter.WritePropertyName("value");
                    WriteValue(jsonWriter, mapValue, truncateForDisplay);
                    jsonWriter.WriteEndObject();
                }
                jsonWriter.WriteEndArray();
            }
            else if (value is ListValue list)
            {
                //Hopefully lists also generate valid JSON on .ToString()
                jsonWriter.WriteRawValue(list.ToString());
            }
            else if (value is ByteArrayValue byteArray)
            {
                const int byteArrayMaxStringLength = 24; //arbitrary number that I think looks good
                var byteArrayAsString = byteArray.ToStringTruncated(byteArrayMaxStringLength);
                jsonWriter.WriteStringValue(byteArrayAsString);
            }
            else if (value is DateTime dt)
            {
                //Write dates as string
                if (ParquetEngineSettings.DateDisplayFormat is not null)
                    jsonWriter.WriteStringValue(dt.ToString(ParquetEngineSettings.DateDisplayFormat));
                else
                    jsonWriter.WriteStringValue(dt.ToString());
            }
            else
            {
                //Everything else just try to write it as string
                jsonWriter.WriteStringValue(value.ToString()!);
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
