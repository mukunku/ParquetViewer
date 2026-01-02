using ParquetViewer.Engine.Types;
using System.Data;

namespace ParquetViewer.Engine
{
    public class StructValueBase : IStructValue
    {
        public string Name { get; }

        public DataRowLite Data { get; }

        public IReadOnlyCollection<string> FieldNames => Data.Columns.Keys;

        public StructValueBase(string name, DataRowLite data)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Data = data ?? throw new ArgumentNullException(nameof(data));
        }

        /// <summary>
        /// Sorts by field names first, then by values
        /// </summary>
        public int CompareTo(IStructValue? other)
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
                int comparison = Engine.Helpers.CompareTo(value, otherValue);
                if (comparison != 0)
                    return comparison;
            }

            return 0; //Both structs appear equal
        }

        public int CompareTo(object? obj)
        {
            if (obj is IStructValue @struct)
                return CompareTo(@struct);
            else
                return 1;
        }

        public DataTable ToDataTable() => Data.ToDataTable();

        public override string ToString() => ToJSON(out _);

        public string ToStringTruncated(int desiredLength) => ToJSON(out _, desiredLength);

        public string ToJSON(out bool success, int? desiredLength = null)
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
                        Helpers.WriteValue(jsonWriter, value, desiredLength is not null);

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
    }
}
