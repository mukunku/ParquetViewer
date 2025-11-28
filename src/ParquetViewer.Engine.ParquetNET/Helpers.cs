namespace ParquetViewer.Engine.ParquetNET
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

        //source: https://www.aloneguid.uk/posts/2023/04/parquet-empty-vs-null
        public static bool IsNull(this Parquet.Data.DataColumn dataColumn, int index) => dataColumn.DefinitionLevels?.Length > index && dataColumn.DefinitionLevels[index] == 0;
        public static bool IsEmpty(this Parquet.Data.DataColumn dataColumn, int index) => dataColumn.DefinitionLevels?.Length > index && dataColumn.DefinitionLevels[index] == 1;

        public static IEnumerable<(object?, object?)> PairEnumerables(IEnumerable<object?> enumerable1, IEnumerable<object?> enumerable2, object? missingIndexValue = null)
        {
            ArgumentNullException.ThrowIfNull(enumerable1);
            ArgumentNullException.ThrowIfNull(enumerable2);

            var enumerator1 = enumerable1.GetEnumerator();
            var enumerator2 = enumerable2.GetEnumerator();

            var hasMore1 = enumerator1.MoveNext();
            var hasMore2 = enumerator2.MoveNext();
            while (hasMore1 || hasMore2)
            {
                yield return (hasMore1 ? enumerator1.Current : missingIndexValue, hasMore2 ? enumerator2.Current : missingIndexValue);
                hasMore1 = enumerator1.MoveNext();
                hasMore2 = enumerator2.MoveNext();
            }
        }
    }
}
