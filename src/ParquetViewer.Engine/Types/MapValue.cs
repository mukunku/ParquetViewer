namespace ParquetViewer.Engine.Types
{
    public class MapValue : IComparable<MapValue>, IComparable
    {
        public object Key { get; } = DBNull.Value;
        public Type KeyType { get; }
        public object Value { get; } = DBNull.Value;
        public Type ValueType { get; }
        public static string? DateDisplayFormat { get; set; }

        public MapValue(object key, Type keyType, object value, Type valueType)
        {
            if (key is null)
                throw new ArgumentNullException(nameof(key));
            else if (value is null)
                throw new ArgumentNullException(nameof(value));

            Key = key;
            Value = value;

            //We need the types because if the key/value is null itself,
            //there's no way to determine what the type's supposed to be
            KeyType = keyType;
            ValueType = valueType;

            if (key != DBNull.Value && key.GetType() != keyType)
                throw new ArgumentException($"The key's type {key.GetType()} doesn't match the passed key-type {keyType}");

            if (value != DBNull.Value && value.GetType() != valueType)
                throw new ArgumentException($"The value's type {value.GetType()} doesn't match the passed value-type {valueType}");
        }

        public override string ToString()
        {
            string key;
            if (Key is DateTime dt && DateDisplayFormat is not null)
                key = dt.ToString(DateDisplayFormat);
            else
                key = Key?.ToString() ?? string.Empty;

            string value;
            if (Value is DateTime dt2 && DateDisplayFormat is not null)
                value = dt2.ToString(DateDisplayFormat);
            else
                value = Value?.ToString() ?? string.Empty;

            return $"({key},{value})";
        }

        /// <summary>
        /// Sorts by Key first, then Value.
        /// </summary>
        public int CompareTo(MapValue? other)
        {
            if (other is null)
                return 1;

            int comparison = Helpers.CompareTo(Key, other.Key);
            if (comparison != 0) 
                return comparison;

            return Helpers.CompareTo(Value, other.Value);
        }

        public int CompareTo(object? obj)
        {
            if (obj is MapValue mapValue)
                return CompareTo(mapValue);
            else
                return 1;
        }


    }
}
