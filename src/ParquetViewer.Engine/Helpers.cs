using ParquetViewer.Engine.Types;
using System.Numerics;

namespace ParquetViewer.Engine
{
    public static class Helpers
    {
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

        public static IEnumerable<(T, R)> PairEnumerables<T,R>(IEnumerable<T> enumerable1, IEnumerable<R> enumerable2)
        {
            ArgumentNullException.ThrowIfNull(enumerable1);
            ArgumentNullException.ThrowIfNull(enumerable2);

            var enumerator1 = enumerable1.GetEnumerator();
            var enumerator2 = enumerable2.GetEnumerator();

            var hasMore1 = enumerator1.MoveNext();
            var hasMore2 = enumerator2.MoveNext();
            while (hasMore1 && hasMore2)
            {
                yield return (enumerator1.Current, enumerator2.Current);
                hasMore1 = enumerator1.MoveNext();
                hasMore2 = enumerator2.MoveNext();
            }

            if (hasMore1 || hasMore2)
            {
                throw new InvalidDataException("Enumerables are of different lengths.");
            }
        }

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

        public static void WriteValue(Utf8JsonWriterWithRunningLength jsonWriter, object value, bool truncateForDisplay)
        {
            if (value is null)
            {
                //Value should never be null as we should be replacing all those with DBNull.Value
                throw new ArgumentNullException(nameof(value));
            }
            else if (value == DBNull.Value)
            {
                jsonWriter.WriteNullValue();
            }
            else if (value is string str)
            {
                jsonWriter.WriteStringValue(str);
            }
            else if (value is bool @bool)
            {
                jsonWriter.WriteBooleanValue(@bool);
            }
            else if (value.GetType().IsNumber())
            {
                jsonWriter.WriteNumberValue(Convert.ToDecimal(value));
            }
            else if (value is IStructValue @struct)
            {
                var json = @struct.ToJSON(out var success);
                if (success)
                    jsonWriter.WriteRawValue(json);
                else
                    jsonWriter.WriteStringValue(json);
            }
            else if (value is IMapValue map)
            {
                jsonWriter.WriteStartArray();
                foreach ((object mapKey, object mapValue) in map)
                {
                    jsonWriter.WriteStartObject();
                    jsonWriter.WritePropertyName("key");
                    WriteValue(jsonWriter, mapKey, truncateForDisplay);
                    jsonWriter.WritePropertyName("value");
                    WriteValue(jsonWriter, mapValue, truncateForDisplay);
                    jsonWriter.WriteEndObject();
                }
                jsonWriter.WriteEndArray();
            }
            else if (value is IListValue list)
            {
                jsonWriter.WriteStartArray();
                foreach (var item in list)
                {
                    WriteValue(jsonWriter, item, truncateForDisplay);
                }
                jsonWriter.WriteEndArray();
            }
            else if (value is IByteArrayValue byteArray /*&& truncateForDisplay //should use the entire byte array if 
                                                        * we're not truncating for display? Seems kind of unreasonable 
                                                        * for users to rely on binary data within a Struct value preview.*/)
            {
                const int byteArrayMaxStringLength = 24; //arbitrary number that I think looks good
                var byteArrayAsString = byteArray.ToStringTruncated(byteArrayMaxStringLength);
                jsonWriter.WriteStringValue(byteArrayAsString);
            }
            else if (value is DateTime dt)
            {
                //Write dates as string
                if (ParquetEngineSettings.DateDisplayFormat is not null)
                    jsonWriter.WriteStringValue(dt.ToString(ParquetEngineSettings.DateDisplayFormat));
                else
                    jsonWriter.WriteStringValue(dt.ToString());
            }
            else if (value is DateOnly dateOnly)
            {
                //Write dates as string
                if (ParquetEngineSettings.DateOnlyDisplayFormat is not null)
                    jsonWriter.WriteStringValue(dateOnly.ToString(ParquetEngineSettings.DateOnlyDisplayFormat));
                else
                    jsonWriter.WriteStringValue(dateOnly.ToString());
            }
            else
            {
                //Everything else just try to write it as string
                jsonWriter.WriteStringValue(value.ToString()!);
            }
        }

        /// <summary>
        /// Returns true if the type is a number type.
        /// </summary>
        public static bool IsNumber(this Type type) =>
            Array.Exists(type.GetInterfaces(), i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(INumber<>));

        public static IEnumerable<string> ListParquetFiles(string folderPath)
        {
            var parquetFiles = Directory.EnumerateFiles(folderPath, "*", SearchOption.AllDirectories)
                .Where(file =>
                        file.EndsWith(".parquet") ||
                        file.EndsWith(".parquet.gzip") ||
                        file.EndsWith(".parquet.gz")
                );

            if (!parquetFiles.Any())
            {
                //Check for extensionless files
                parquetFiles = Directory.EnumerateFiles(folderPath, "*", SearchOption.AllDirectories);
            }

            return parquetFiles.OrderBy(filename => filename);
        }

        public static void EZDispose(IEnumerable<IDisposable> disposables)
        {
            if (disposables is null)
            {
                return;
            }

            foreach (var disposable in disposables)
            {
                try
                {
                    disposable?.Dispose();
                }
                catch { /* Swallow */ }
            }
        }
    }
}
