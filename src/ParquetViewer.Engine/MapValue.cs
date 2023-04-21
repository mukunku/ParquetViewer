namespace ParquetViewer.Engine
{
    public class MapValue : IComplexValue
    {
        public object? Key { get; }
        public Type KeyType { get; }
        public object? Value { get; }
        public Type ValueType { get; }
        public static string? DateDisplayFormat { get; set; }

        public MapValue(object key, Type keyType, object value, Type valueType)
        {
            Key = key;
            Value = value;

            //We need the types because if the key/value is null itself,
            //there's no way to determine what the type's supposed to be
            KeyType = keyType;
            ValueType = valueType;

            if (key is not null && key != DBNull.Value && key.GetType() != keyType)
                throw new ArgumentException($"The key's type {key.GetType()} doesn't match the passed key-type {keyType}");

            if (value is not null && value != DBNull.Value && value.GetType() != valueType)
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
    }
}
