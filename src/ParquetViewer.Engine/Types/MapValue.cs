using System.Collections;
using System.Text;

namespace ParquetViewer.Engine.Types;

public class MapValue : IMapValue
{
    public ArrayList Keys { get; }

    public Type KeyType { get; }

    public ArrayList Values { get; }

    public Type ValueType { get; }

    public int Length => Math.Max(Keys.Count, Values.Count);

    public MapValue(ArrayList keys, Type keyType, ArrayList values, Type valueType)
    {
        if (keys is null)
            throw new ArgumentNullException(nameof(keys));
        else if (values is null)
            throw new ArgumentNullException(nameof(values));
        else if (keys.Count != values.Count)
            throw new ArrayTypeMismatchException("The keys and values must be of the same length");

        Keys = keys;
        Values = values;

        //Only put the type in the message, never the value itself since it's user data
        var mismatchedKey = keys.Cast<object?>().Where(key => key != DBNull.Value).FirstOrDefault(key => key!.GetType() != keyType);
        if (mismatchedKey is not null)
            throw new ArgumentException($"The key's type {mismatchedKey.GetType()} doesn't match the passed key-type {keyType}");

        var mismatchedValue = values.Cast<object?>().Where(value => value != DBNull.Value).FirstOrDefault(value => value!.GetType() != valueType);
        if (mismatchedValue is not null)
            throw new ArgumentException($"The value's type {mismatchedValue.GetType()} doesn't match the passed value-type {valueType}");

        //We need the types because if the key/value arraylists are empty
        //there's no way to determine what the type's supposed to be
        KeyType = keyType;
        ValueType = valueType;
    }

    /// <summary>
    /// Sorts by Key first, then Value.
    /// </summary>
    public int CompareTo(IMapValue? other)
    {
        if (other is null)
            return 1;
        else if (this is null)
            return -1;

        for (var i = 0; i < Length; i++)
        {
            if (other.Length == i)
            {
                //This map has more records, so lets say it's 'less than' in sort order
                return -1;
            }

            var (key, value) = GetMapValue(i);
            var (otherKey, otherValue) = other.GetMapValue(i);

            int comparison = Helpers.CompareTo(key, otherKey);
            if (comparison != 0)
                return comparison;

            comparison = Helpers.CompareTo(value, otherValue);
            if (comparison != 0)
                return comparison;
        }

        if (Length < other.Length)
            return 1; //this map's records is a subset of the other's records so say it's 'more than' in sort order

        return 0; //the map records appear equal
    }

    public int CompareTo(object? obj)
    {
        if (obj is IMapValue mapValue)
            return CompareTo(mapValue);
        else
            return 1;
    }

    public IEnumerator<(object Key, object Value)> GetEnumerator()
    {
        for (var i = 0; i < Length; i++)
        {
            yield return GetMapValue(i);
        }
    }

    public (object Key, object Value) GetMapValue(int index)
        => (Keys[index] ?? DBNull.Value, Values[index] ?? DBNull.Value);

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public override string ToString() => ToString(null);

    /// <summary>
    /// Formats the map, but stops adding entries once the text passes <paramref name="desiredLength"/> (NOT GUARANTEED).
    /// </summary>
    public string ToStringTruncated(int desiredLength) => ToString(desiredLength);

    private string ToString(int? desiredLength)
    {
        var mapValuesStringBuilder = new StringBuilder("[");
        for (var i = 0; i < Length; i++)
        {
            if (i != 0)
            {
                if (desiredLength > 0 && mapValuesStringBuilder.Length > desiredLength)
                {
                    return mapValuesStringBuilder.Append("[...]").ToString();
                }

                mapValuesStringBuilder.Append(',');
            }

            mapValuesStringBuilder.Append(FormatString(GetMapValue(i), desiredLength - mapValuesStringBuilder.Length));
        }

        mapValuesStringBuilder.Append(']');
        return mapValuesStringBuilder.ToString();

        //We don't handle keys or values that contain the reserved chars we format maps with: , [ ] ( )
        //E.g. a key of "a,b" renders as (a,b,1). I can't decide what the best way to format Maps are so this is what I'm going with for now.
        //remainingLength: how much of desiredLength is left for this entry, so nested lists, maps, structs and binary
        //values stop early too instead of being fully formatted. Null when we aren't truncating.
        static string FormatString((object Key, object Value) map, int? remainingLength)
        {
            var key = FormatPart(map.Key, remainingLength);
            var value = FormatPart(map.Value, remainingLength - key.Length); //The value gets whatever the key left over
            return $"({key},{value})";
        }

        static string FormatPart(object part, int? remainingLength)
        {
            //Always leave at least 1 so we show something. A used-up budget (0 or less) would mean "no limit" to
            //ToStringTruncated and isn't a valid length for TruncateString.
            int? budget = remainingLength is null ? null : Math.Max(1, remainingLength.Value);

            if (part is DateTime dt && ParquetEngineSettings.DateDisplayFormat is not null)
                return dt.ToString(ParquetEngineSettings.DateDisplayFormat);
            else if (part is DateOnly dateOnly && ParquetEngineSettings.DateOnlyDisplayFormat is not null)
                return dateOnly.ToString(ParquetEngineSettings.DateOnlyDisplayFormat);
            else if (part is TimeOnly timeOnly && ParquetEngineSettings.TimeOnlyDisplayFormat is not null)
                return timeOnly.ToString(ParquetEngineSettings.TimeOnlyDisplayFormat);
            else if (budget is not null && part is ITruncatable truncatable)
                return truncatable.ToStringTruncated(budget.Value);
            else if (budget is not null && part is string text && text.Length > budget)
                return Helpers.TruncateString(text, budget.Value) + "[...]"; //So a single long string can't blow past the limit
            else
                return part?.ToString() ?? string.Empty;
        }
    }
}