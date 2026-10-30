using System.Data;
using System.Text;

namespace ParquetViewer.Engine.Types;

public class StructValue : IStructValue
{
    private readonly string[] _fieldNames;
    private readonly object[] _values;

    public IReadOnlyList<string> FieldNames => _fieldNames;

    public IReadOnlyList<object> Values => _values;

    /// <param name="fieldNames">Never modified, so it can be shared by every instance of the same struct field.</param>
    public StructValue(string[] fieldNames, object[] values)
    {
        ArgumentNullException.ThrowIfNull(fieldNames);
        ArgumentNullException.ThrowIfNull(values);

        if (fieldNames.Length != values.Length)
            throw new ArgumentException($"Value count {values.Length} doesn't match field count {fieldNames.Length}", nameof(values));

        _fieldNames = fieldNames;
        _values = values;
    }

    public object GetValue(string fieldName)
    {
        var index = Array.IndexOf(_fieldNames, fieldName);
        return index >= 0
            ? _values[index]
            : throw new IndexOutOfRangeException($"Field `{fieldName}` not found");
    }

    /// <summary>
    /// Sorts by field names first, then by values
    /// </summary>
    public int CompareTo(IStructValue? other)
    {
        if (other is null || other.FieldNames.Count == 0)
            return 1;

        if (FieldNames.Count == 0)
            return -1;

        var otherColumnNames = string.Join("|", other.FieldNames);
        var columnNames = string.Join("|", FieldNames);

        int schemaComparison = columnNames.CompareTo(otherColumnNames);
        if (schemaComparison != 0)
            return schemaComparison;

        int fieldCount = FieldNames.Count;
        for (var i = 0; i < fieldCount; i++)
        {
            var otherValue = other.Values[i];
            var value = _values[i];
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
        foreach (var pair in Helpers.PairEnumerables(_fieldNames, _values))
        {
            var columnName = pair.Item1;
            var value = pair.Item2;
            var valueType = value != DBNull.Value ? value.GetType() : typeof(object);
            dt.Columns.Add(new DataColumn(columnName, valueType));
        }
        var row = dt.NewRow();
        row.ItemArray = _values;
        dt.Rows.Add(row);
        return dt;
    }

    public override string ToString() => ToJSON(out _);

    public string ToStringTruncated(int desiredLength) => ToJSON(out _, desiredLength);

    public string ToJSON(out bool success, int? desiredLength = null)
    {
        try
        {
            bool isTruncated;
            using var ms = new MemoryStream();
            using (var jsonWriter = new Utf8JsonWriterWithRunningLength(ms))
            {
                isTruncated = !Helpers.WriteValue(jsonWriter, this, desiredLength);
            }

            var json = Encoding.UTF8.GetString(ms.GetBuffer(), 0, (int)ms.Length);
            success = true;
            return isTruncated ? json + "[...]" : json;
        }
        catch (Exception ex)
        {
            success = false;
            return $"Error while serializing Struct field: {Environment.NewLine}{Environment.NewLine}{ex}";
        }
    }
}