using ParquetViewer.Engine.Types;
using System.Data;
using System.Diagnostics.CodeAnalysis;

namespace ParquetViewer.Engine.ParquetNET.Types
{
    public class StructValue : IStructValue, IComparable<StructValue>, IComparable
    {
        public string Name { get; }

        public DataRowLite Data { get; }

        internal bool IsList { get; set; }

        //TODO: Add a public constructor?
        internal StructValue(string name, DataRowLite data)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Data = data ?? throw new ArgumentNullException(nameof(data));
        }

        public override string ToString() => ToJSON(out _);

        public string ToStringTruncated(int desiredLength) => ToJSON(out _, desiredLength);

        private string ToJSON(out bool success, int? desiredLength = null)
        {
            try
            {
                bool isTruncated = false;
                using var ms = new MemoryStream();
                using (var jsonWriter = new Utf8JsonWriterWithRunningLength(ms))
                {
                    jsonWriter.WriteStartObject();
                    for (var i = 0; i < this.Data.Columns.Count; i++)
                    {
                        string columnName = this.Data.Columns.Values.ElementAt(i).Name
                            //Remove the parent field name from columns when rendering the data as json in the gridview cell.
                            .Replace($"{this.Name}/", string.Empty);
                        jsonWriter.WritePropertyName(columnName);

                        object value = this.Data.Row[i];
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
                success = true;
                return json;
            }
            catch (Exception ex)
            {
                success = false;
                return $"Error while serializing Struct field '{Name}': {Environment.NewLine}{Environment.NewLine}{ex}";
            }
        }

        public DataTable ToDataTable() => this.Data.ToDataTable();

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
            else if (value.GetType().IsNumber())
            {
                jsonWriter.WriteNumberValue(Convert.ToDecimal(value));
            }
            else if (value is StructValue @struct)
            {
                var json = @struct.ToJSON(out var success);
                if (success)
                    jsonWriter.WriteRawValue(json);
                else
                    jsonWriter.WriteStringValue(json);
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
                jsonWriter.WriteStartArray();
                foreach (var item in list)
                {
                    WriteValue(jsonWriter, item, truncateForDisplay);
                }
                jsonWriter.WriteEndArray();
            }
            else if (value is ByteArrayValue byteArray /*&& truncateForDisplay //should use the entire byte array if 
                                                        * we're not truncating for display? Seems kind of unreasonable 
                                                        * for users to rely on binary data within a Struct value preview.*/)
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
            else if (value is DateOnly dateOnly)
            {
                //Write dates as string
                if (ParquetEngineSettings.DateOnlyDisplayFormat is not null)
                    jsonWriter.WriteStringValue(dateOnly.ToString(ParquetEngineSettings.DateOnlyDisplayFormat));
                else
                    jsonWriter.WriteStringValue(dateOnly.ToString());
            }
            else
            {
                //Everything else just try to write it as string
                jsonWriter.WriteStringValue(value.ToString()!);
            }
        }

        private IReadOnlyCollection<string> FieldNames => Data.Columns.Keys;

        /// <summary>
        /// Sorts by field names first, then by values
        /// </summary>
        public int CompareTo(StructValue? other)
        {
            if (other?.Data is null || other.FieldNames.Count == 0)
                return 1;

            if (Data is null || FieldNames.Count == 0)
                return -1;

            var otherColumnNames = string.Join("|", other.FieldNames);
            var columnNames = string.Join("|", this.FieldNames);

            int schemaComparison = columnNames.CompareTo(otherColumnNames);
            if (schemaComparison != 0)
                return schemaComparison;

            int fieldCount = FieldNames.Count;
            for (var i = 0; i < fieldCount; i++)
            {
                var otherValue = other.Data.Row[i];
                var value = Data.Row[i];
                int comparison = Helpers.CompareTo(value, otherValue);
                if (comparison != 0)
                    return comparison;
            }

            return 0; //Both structs appear equal
        }

        public int CompareTo(object? obj)
        {
            if (obj is StructValue @struct)
                return CompareTo(@struct);
            else
                return 1;
        }

        public int CompareTo(IStructValue? other) => this.CompareTo((object?)other);
    }
}
