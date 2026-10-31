using ParquetViewer.Engine.Types;

namespace ParquetViewer.Engine;

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

    public static IEnumerable<(T, R)> PairEnumerables<T, R>(IEnumerable<T> enumerable1, IEnumerable<R> enumerable2)
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

    /// <summary>
    /// Writes <paramref name="value"/> as JSON, including any lists, maps and structs nested inside it.
    /// </summary>
    /// <param name="maxLength">For display: once the writer's running length passes this, stop before the next element
    /// or field at whatever depth we're at. Binary values are also shortened. Leave null to write everything (e.g. exports).</param>
    /// <returns>False if writing stopped early because of <paramref name="maxLength"/>. The JSON is then left incomplete.</returns>
    public static bool WriteValue(Utf8JsonWriterWithRunningLength jsonWriter, object value, int? maxLength = null)
    {
        switch (value)
        {
            case null:
                //Value should never be null as we should be replacing all those with DBNull.Value
                throw new ArgumentNullException(nameof(value));
            case DBNull:
                jsonWriter.WriteNullValue();
                return true;
            case string str:
                return WriteStringValue(jsonWriter, str, maxLength);
            case bool @bool:
                jsonWriter.WriteBooleanValue(@bool);
                return true;
            case not null when value.GetType().IsNumber():
                jsonWriter.WriteNumberValue(value);
                return true;
            case IStructValue @struct:
                jsonWriter.WriteStartObject();
                for (var i = 0; i < @struct.FieldNames.Count; i++)
                {
                    if (i > 0 && IsPastMaxLength(jsonWriter, maxLength))
                        return false;

                    jsonWriter.WritePropertyName(@struct.FieldNames[i]);
                    if (!WriteValue(jsonWriter, @struct.Values[i], maxLength))
                        return false;
                }
                jsonWriter.WriteEndObject();
                return true;
            case IMapValue map:
                jsonWriter.WriteStartArray();
                var isFirst = true;
                foreach ((object mapKey, object mapValue) in map)
                {
                    //Only stop between elements
                    if (!isFirst && IsPastMaxLength(jsonWriter, maxLength))
                        return false;
                    isFirst = false;

                    jsonWriter.WriteStartObject();
                    jsonWriter.WritePropertyName("key");
                    if (!WriteValue(jsonWriter, mapKey, maxLength))
                        return false;
                    jsonWriter.WritePropertyName("value");
                    if (!WriteValue(jsonWriter, mapValue, maxLength))
                        return false;
                    jsonWriter.WriteEndObject();
                }
                jsonWriter.WriteEndArray();
                return true;
            case IListValue list:
                jsonWriter.WriteStartArray();
                isFirst = true;
                foreach (var listItem in list)
                {
                    //Only stop between elements
                    if (!isFirst && IsPastMaxLength(jsonWriter, maxLength))
                        return false;
                    isFirst = false;

                    if (!WriteValue(jsonWriter, listItem, maxLength))
                        return false;
                }
                jsonWriter.WriteEndArray();
                return true;
            case IByteArrayValue byteArray:
                if (maxLength is not null)
                {
                    const int byteArrayMaxStringLength = 24; //arbitrary number that I think looks good
                    jsonWriter.WriteStringValue(byteArray.ToStringTruncated(byteArrayMaxStringLength));
                }
                else
                {
                    jsonWriter.WriteStringValue(byteArray.ToString()!); //All the bytes in hex, so exports don't lose data
                }
                return true;
            case DateTime dt:
                //Write dates as string
                if (ParquetEngineSettings.DateDisplayFormat is not null)
                    jsonWriter.WriteStringValue(dt.ToString(ParquetEngineSettings.DateDisplayFormat));
                else
                    jsonWriter.WriteStringValue(dt.ToString());
                return true;
            case DateOnly dateOnly:
                //Write dates as string
                if (ParquetEngineSettings.DateOnlyDisplayFormat is not null)
                    jsonWriter.WriteStringValue(dateOnly.ToString(ParquetEngineSettings.DateOnlyDisplayFormat));
                else
                    jsonWriter.WriteStringValue(dateOnly.ToString());
                return true;
            case TimeOnly timeOnly:
                //Write time as string
                if (ParquetEngineSettings.TimeOnlyDisplayFormat is not null)
                    jsonWriter.WriteStringValue(timeOnly.ToString(ParquetEngineSettings.TimeOnlyDisplayFormat));
                else
                    jsonWriter.WriteStringValue(timeOnly.ToString());
                return true;
            default:
                //Everything else just try to write it as string
                return WriteStringValue(jsonWriter, value.ToString()!, maxLength);
        }
    }

    private static bool IsPastMaxLength(Utf8JsonWriterWithRunningLength jsonWriter, int? maxLength)
        => maxLength > 0 && jsonWriter.ApproximateStringLengthSoFar > maxLength;

    /// <summary>
    /// Writes <paramref name="text"/>, or only as much of it as fits in what's left of <paramref name="maxLength"/>
    /// so a single long string can't blow past the limit.
    /// </summary>
    /// <returns>False if the text was cut short</returns>
    private static bool WriteStringValue(Utf8JsonWriterWithRunningLength jsonWriter, string text, int? maxLength)
    {
        if (maxLength > 0)
        {
            var remainingLength = Math.Max(1, maxLength.Value - jsonWriter.ApproximateStringLengthSoFar); //Always show something
            if (text.Length > remainingLength)
            {
                jsonWriter.WriteStringValue(TruncateString(text, remainingLength));
                return false;
            }
        }

        jsonWriter.WriteStringValue(text);
        return true;
    }

    /// <summary>
    /// Returns the first <paramref name="length"/> chars of <paramref name="text"/>, without splitting a surrogate pair
    /// (e.g. an emoji) in half. Half a surrogate pair isn't valid text and makes the JSON writer throw.
    /// </summary>
    public static string TruncateString(string text, int length)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(length);

        if (text.Length <= length)
            return text;

        //Make sure we don't split on multichars like emojis as it breaks json writer.
        if (char.IsHighSurrogate(text[length - 1]))
            length++;

        return text[..length];
    }

    /// <summary>
    /// Returns true if the type is one of the built-in number types (<see cref="sbyte"/> through <see cref="decimal"/>).
    /// </summary>
    /// <remarks>
    /// Uses the type code rather than looking for <c>INumber&lt;T&gt;</c>, which would allocate on every call.
    /// <see cref="Utf8JsonWriterWithRunningLength.WriteNumberValue(object)"/> handles exactly these types.
    /// </remarks>
    public static bool IsNumber(this Type type) =>
        Type.GetTypeCode(type) is >= TypeCode.SByte and <= TypeCode.Decimal;

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
            return;

        foreach (var disposable in disposables)
        {
            try
            {
                disposable?.Dispose();
            }
            catch { /* Swallow */ }
        }
    }

    public static async ValueTask EZDisposeAsync(IEnumerable<IAsyncDisposable> disposables)
    {
        if (disposables is null)
            return;

        try
        {
            var tasks = new List<ValueTask>();

            foreach (var disposable in disposables)
            {
                if (disposable is not null)
                {
                    var task = disposable.DisposeAsync();
                    tasks.Add(task);
                }
            }

            foreach (var task in tasks)
            {
                try
                {
                    await task;
                }
                catch { /* Swallow */ }
            }
        }
        catch { /* Swallow */ }
    }

    public static int ByteArraysEqual(ReadOnlySpan<byte> a1, ReadOnlySpan<byte> a2) => a1.SequenceCompareTo(a2);
}