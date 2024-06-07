namespace ParquetViewer.Engine
{
    internal static class Helpers
    {
        public static int CompareTo(object? value, object? otherValue)
        {
            value ??= DBNull.Value;
            otherValue ??= DBNull.Value;

            if (otherValue == DBNull.Value && value == DBNull.Value)
                return 0;

            if (otherValue == DBNull.Value)
                return 1;

            if (value == DBNull.Value)
                return -1;

            if (value is IComparable comparableValue && otherValue is IComparable otherComparableValue
                && value.GetType().Equals(otherValue.GetType()))
            {
                return comparableValue.CompareTo(otherComparableValue);
            }
            else
            {
                return value.ToString()!.CompareTo(otherValue.ToString()!);
            }
        }
    }
}
