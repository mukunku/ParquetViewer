using System.Collections;
using System.Text;

namespace ParquetViewer.Engine.Types;

public class ListValue : IListValue
{
    public IList Data { get; }

    public Type Type { get; }

    public ListValue(ArrayList data, Type type)
    {
        Data = data;
        Type = type; //the parameter is needed for the case where the entire list is null

        foreach (var d in data)
        {
            if (d != DBNull.Value && d is not null
                && Type != d.GetType())
            {
                throw new ArrayTypeMismatchException($"Data type {d.GetType()} doesn't match the passed type {type}");
            }
        }
    }

    public override string ToString() => ToJSON(null);

    /// <summary>
    /// Serializes the list, but stops once the text passes <paramref name="desiredLength"/> (NOT GUARANTEED), at any
    /// depth of nesting. So large lists (e.g. embeddings) aren't fully serialized just to be cut off for display.
    /// </summary>
    public string ToStringTruncated(int desiredLength) => ToJSON(desiredLength);

    private string ToJSON(int? desiredLength)
    {
        bool isTruncated;
        using var ms = new MemoryStream();
        using (var jsonWriter = new Utf8JsonWriterWithRunningLength(ms))
        {
            isTruncated = !Helpers.WriteValue(jsonWriter, this, desiredLength);
        }

        var json = Encoding.UTF8.GetString(ms.GetBuffer(), 0, (int)ms.Length);
        return isTruncated ? json + "[...]" : json;
    }

    public int CompareTo(IListValue? other)
    {
        if (other is null)
            return 1;
        else if (this is null)
            return -1;

        for (var i = 0; i < Data.Count; i++)
        {
            if (other.Data.Count == i)
            {
                //This list has more values, so lets say it's 'less than' in sort order
                return -1;
            }

            var value = Data[i];
            var otherValue = other.Data[i];
            int comparison = Helpers.CompareTo(value, otherValue);
            if (comparison != 0)
                return comparison;
        }

        if (Data.Count < other.Data.Count)
            return 1; //this list has less values so say it's 'more than' in sort order

        return 0; //the lists appear equal
    }

    public int CompareTo(object? obj)
    {
        if (obj is IListValue list)
            return CompareTo(list);
        else
            return 1;
    }

    public IEnumerator<object> GetEnumerator()
    {
        foreach (var item in Data)
        {
            yield return item;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}