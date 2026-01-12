using System.Data;

namespace ParquetViewer.Engine.Types
{
    public class StructValue : IStructValue
    {
        public IDataRowLite Data { get; }

        public IReadOnlyCollection<string> FieldNames => Data.ColumnNames;

        public StructValue(IDataRowLite data)
        {
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
            var columnNames = string.Join("|", FieldNames);

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
            if (obj is IStructValue @struct)
                return CompareTo(@struct);
            else
                return 1;
        }

        public DataTable ToDataTable()
        {
            var dt = new DataTable();
            foreach (var pair in Helpers.PairEnumerables(this.Data.ColumnNames, this.Data.Row))
            {
                var columnName = pair.Item1;
                var value = pair.Item2;
                var valueType = value != DBNull.Value ? value.GetType() : typeof(object);
                dt.Columns.Add(new DataColumn(columnName, valueType));
            }
            var row = dt.NewRow();
            row.ItemArray = this.Data.Row;
            dt.Rows.Add(row);
            return dt;
        }

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
                    for (var i = 0; i < Data.ColumnNames.Count; i++)
                    {
                        string columnName = Data.ColumnNames.ElementAt(i);
                        jsonWriter.WritePropertyName(columnName);

                        object value = Data.Row[i];
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
                return $"Error while serializing Struct field: {Environment.NewLine}{Environment.NewLine}{ex}";
            }
        }
    }
}