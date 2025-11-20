using System.Numerics;

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

        #region Dubious Functions
        //This logic is a cluster f... right now. It blends https://www.aloneguid.uk/posts/2023/04/parquet-empty-vs-null
        //with some of my understanding of how the dremel algorithm works. No way will it work for all cases.

        public static bool IsNull(this Parquet.Data.DataColumn dataColumn, int index, ParquetSchemaElement field)
            => dataColumn.DefinitionLevels?.Length > index && dataColumn.DefinitionLevels[index] <= field.CurrentDefinitionLevel - 1;

        public static bool IsEmpty(this Parquet.Data.DataColumn dataColumn, int index, ParquetSchemaElement field)
            => dataColumn.DefinitionLevels?.Length > index && dataColumn.DefinitionLevels[index] == field.CurrentDefinitionLevel
                    && field.DataField?.MaxDefinitionLevel != dataColumn.DefinitionLevels[index] /*Fixes STRUCT_TYPE_TEST*/;
        #endregion

        /// <summary>
        /// Some parquet writers don't write null entries into the data array for empty and null lists.
        /// This throws off our logic so lets find all empty/null lists and add a null entry into 
        /// the data array to align it with the repetition/definition levels.
        /// </summary>
        /// <param name="dataColumn">The parquet data column</param>
        public static IEnumerable<object> GetDataWithPaddedNulls(this Parquet.Data.DataColumn dataColumn, ParquetSchemaElement field)
        {
            var dataEnumerable = dataColumn.Data.Cast<object?>().Select(d => d ?? DBNull.Value);
            
            int levelCount = dataColumn.DefinitionLevels?.Length ?? 0;
            if (levelCount > dataColumn.Data.Length)
            {
                dataEnumerable = GetDataWithPaddedNulls();

                IEnumerable<object> GetDataWithPaddedNulls()
                {
                    var index = -1;
                    foreach (var data in dataColumn.Data)
                    {
                        index++;

                        while (dataColumn.IsEmpty(index, field) || dataColumn.IsNull(index, field))
                        {
                            yield return DBNull.Value;
                            index++;
                        }

                        yield return data ?? DBNull.Value;
                    }

                    //Need to handle case where last N rows are null/empty
                    while (levelCount > index + 1)
                    {
                        yield return DBNull.Value;
                        index++;
                    }
                }
            }

            return dataEnumerable;
        }

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

        /// <summary>
        /// Returns true if the type is a number type.
        /// </summary>
        public static bool IsNumber(this Type type) =>
            Array.Exists(type.GetInterfaces(), i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(INumber<>));
    }
}
