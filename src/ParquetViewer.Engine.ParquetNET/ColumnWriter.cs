using Parquet;
using Parquet.Schema;
using System.Reflection;

namespace ParquetViewer.Engine.ParquetNET;

/// <summary>
/// Writes a column of values produced by <see cref="ParquetEngine.GetColumnValues"/> (an untyped array)
/// to a <see cref="ParquetRowGroupWriter"/>.
/// </summary>
/// <remarks>
/// Parquet.Net v6 removed <c>WriteColumnAsync(DataColumn, ...)</c> in favour of strongly typed
/// <c>WriteAsync&lt;T&gt;</c> overloads, and there is no untyped entry point, so the correct overload has to
/// be picked and invoked via reflection. <c>string</c> and <c>byte[]</c> columns use dedicated helper
/// overloads because <c>WriteAsync&lt;T&gt;</c> is constrained to <c>where T : struct</c>.
/// </remarks>
internal static class ColumnWriter
{
    //WriteAsync<T>(DataField, ReadOnlyMemory<T>, ReadOnlyMemory<int>?, Dictionary<string, string>?, CancellationToken)
    private static readonly MethodInfo _writeValuesMethod = typeof(ParquetRowGroupWriter)
        .GetMethods(BindingFlags.Public | BindingFlags.Instance)
        .Single(m => m.Name == nameof(ParquetRowGroupWriter.WriteAsync) && m.IsGenericMethodDefinition
            && m.GetParameters()[1].ParameterType == typeof(ReadOnlyMemory<>).MakeGenericType(m.GetGenericArguments()[0]));

    //WriteAsync<T>(DataField, ReadOnlyMemory<T?>, ReadOnlyMemory<int>?, Dictionary<string, string>?, CancellationToken)
    private static readonly MethodInfo _writeNullableValuesMethod = typeof(ParquetRowGroupWriter)
        .GetMethods(BindingFlags.Public | BindingFlags.Instance)
        .Single(m => m.Name == nameof(ParquetRowGroupWriter.WriteAsync) && m.IsGenericMethodDefinition
            && m.GetParameters()[1].ParameterType == typeof(ReadOnlyMemory<>).MakeGenericType(
                typeof(Nullable<>).MakeGenericType(m.GetGenericArguments()[0])));

    private static readonly MethodInfo _writeStringMethod = typeof(ParquetRowGroupWriter)
        .GetMethod(nameof(ParquetRowGroupWriter.WriteAsync),
            [typeof(DataField), typeof(IReadOnlyCollection<string>), typeof(ReadOnlyMemory<int>?)]!)!;

    private static readonly MethodInfo _writeByteArrayMethod = typeof(ParquetRowGroupWriter)
        .GetMethod(nameof(ParquetRowGroupWriter.WriteAsync),
            [typeof(DataField), typeof(IReadOnlyCollection<byte[]>), typeof(ReadOnlyMemory<int>?)]!)!;

    public static Task WriteAsync(ParquetRowGroupWriter rowGroup, DataField field, Array values,
        CancellationToken cancellationToken)
    {
        if (field is TimeDataField)
            values = ConvertTimeValues(field, values);

        var elementType = values.GetType().GetElementType()
            ?? throw new ArgumentException("Expected a single dimensional array of values", nameof(values));

        if (elementType == typeof(string) || elementType == typeof(byte[]))
        {
            //These have dedicated overloads which take care of nullability for us
            var referenceTypeMethod = elementType == typeof(string) ? _writeStringMethod : _writeByteArrayMethod;
            return (Task)referenceTypeMethod.Invoke(rowGroup, [field, values, null])!;
        }

        //Nullability is expressed by the array's element type, e.g. `int?[]` for a nullable int column
        var underlyingType = Nullable.GetUnderlyingType(elementType);
        var isNullable = underlyingType is not null;
        var valueType = underlyingType ?? elementType;
        var method = (isNullable ? _writeNullableValuesMethod : _writeValuesMethod).MakeGenericMethod(valueType);

        var memoryType = typeof(ReadOnlyMemory<>).MakeGenericType(
            isNullable ? typeof(Nullable<>).MakeGenericType(valueType) : valueType);
        var memory = Activator.CreateInstance(memoryType, values)!;

        return (Task)method.Invoke(rowGroup, [field, memory, null, null, cancellationToken])!;
    }

    /// <summary>
    /// Inverse of <see cref="ParquetColumnData"/>'s TIME handling. The engine hands us <see cref="TimeOnly"/>
    /// values (that's what the grid shows), but Parquet.Net v6 stores TIME as a raw count of millis,
    /// micros or nanos, so convert back into whichever the field declares.
    /// </summary>
    private static Array ConvertTimeValues(DataField field, Array values)
    {
        var elementType = values.GetType().GetElementType()
            ?? throw new ArgumentException("Expected a single dimensional array of values", nameof(values));
        var isNullable = Nullable.GetUnderlyingType(elementType) is not null || !elementType.IsValueType;

        var timeField = (TimeDataField)field;
        var underlyingType = timeField.Precision == TimeUnitPrecision.Millis ? typeof(int) : typeof(long);
        var targetType = isNullable ? typeof(Nullable<>).MakeGenericType(underlyingType) : underlyingType;
        var converted = Array.CreateInstance(targetType, values.Length);

        for (var index = 0; index < values.Length; index++)
        {
            var value = values.GetValue(index);
            if (value is null or DBNull)
            {
                converted.SetValue(null, index);
                continue;
            }

            if (value is not TimeOnly time)
                throw new InvalidCastException($"Expected a TimeOnly value for field `{field.Path}` but found '{value.GetType()}'");

            converted.SetValue(timeField.Precision switch
            {
                TimeUnitPrecision.Millis => (object)(int)(time.Ticks / TimeSpan.TicksPerMillisecond),
                //1 tick is 100ns, so 1 microsecond is 10 ticks and 1 nanosecond is 1/100th of a tick
                TimeUnitPrecision.Micros => (object)(time.Ticks / 10L),
                TimeUnitPrecision.Nanos => (object)(time.Ticks * 100L),
                _ => throw new NotSupportedException($"Unsupported TIME precision '{timeField.Precision}' for field `{field.Path}`")
            }, index);
        }

        return converted;
    }
}