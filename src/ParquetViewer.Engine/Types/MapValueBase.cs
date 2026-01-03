using ParquetViewer.Engine.Types;
using System.Collections;
using System.Text;

namespace ParquetViewer.Engine
{
    public class MapValueBase : IMapValue
    {
        public ArrayList Keys { get; }

        public Type KeyType { get; }

        public ArrayList Values { get; }

        public Type ValueType { get; }

        public int Length => Math.Max(Keys.Count, Values.Count);

        public MapValueBase(ArrayList keys, Type keyType, ArrayList values, Type valueType)
        {
            if (keys is null)
                throw new ArgumentNullException(nameof(keys));
            else if (values is null)
                throw new ArgumentNullException(nameof(values));
            else if (keys.Count != values.Count)
                throw new ArrayTypeMismatchException("The keys and values must be of the same length");

            Keys = keys;
            Values = values;

            var mismatchedType = keys.Cast<object?>().Where(key => key != DBNull.Value).FirstOrDefault(key => key!.GetType() != keyType);
            if (mismatchedType != null)
                throw new ArgumentException($"The key's type {mismatchedType} doesn't match the passed key-type {keyType}");

            mismatchedType = values.Cast<object?>().Where(value => value != DBNull.Value).FirstOrDefault(value => value!.GetType() != valueType);
            if (mismatchedType != null)
                throw new ArgumentException($"The value's type {mismatchedType} doesn't match the passed value-type {valueType}");

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

                var (key, value) = this.GetMapValue(i);
                var (otherKey, otherValue) = other.GetMapValue(i);

                int comparison = Helpers.CompareTo(key, otherKey);
                if (comparison != 0)
                    return comparison;

                comparison = Helpers.CompareTo(value, otherValue);
                if (comparison != 0)
                    return comparison;
            }

            if (this.Length < other.Length)
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

        public override string ToString()
        {
            var mapValuesStringBuilder = new StringBuilder("[");
            for (var i = 0; i < Length; i++)
            {
                if (i != 0)
                {
                    mapValuesStringBuilder.Append(',');
                }

                mapValuesStringBuilder.Append(FormatString(GetMapValue(i)));
            }

            mapValuesStringBuilder.Append(']');
            return mapValuesStringBuilder.ToString();

            static string FormatString((object Key, object Value) map)
            {
                string key;
                if (map.Key is DateTime dt && ParquetEngineSettings.DateDisplayFormat is not null)
                    key = dt.ToString(ParquetEngineSettings.DateDisplayFormat);
                else if (map.Key is DateOnly dateOnly && ParquetEngineSettings.DateOnlyDisplayFormat is not null)
                    key = dateOnly.ToString(ParquetEngineSettings.DateOnlyDisplayFormat);
                else
                    key = map.Key?.ToString() ?? string.Empty;

                string value;
                if (map.Value is DateTime dt2 && ParquetEngineSettings.DateDisplayFormat is not null)
                    value = dt2.ToString(ParquetEngineSettings.DateDisplayFormat);
                else if (map.Value is DateOnly dateOnly && ParquetEngineSettings.DateOnlyDisplayFormat is not null)
                    value = dateOnly.ToString(ParquetEngineSettings.DateOnlyDisplayFormat);
                else
                    value = map.Value?.ToString() ?? string.Empty;

                return $"({key},{value})";
            }
        }
    }
}
