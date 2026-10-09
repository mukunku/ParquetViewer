using Parquet.Data;
using Parquet.Schema;
using System.Numerics;

namespace ParquetViewer.Engine.ParquetNET;

/// <summary>
/// Replaces <c>Parquet.Data.DataColumn</c>, which was removed in Parquet.Net v6.
/// V6 hands back pooled <see cref="Data.Span{T}"/> buffers that must be disposed of and
/// cannot be held onto, so we copy the values out into a managed array instead.
/// </summary>
internal sealed class ParquetColumnData
{
    private readonly Array _data;
    private readonly ParquetSchemaElement _field;
    private DataField DataField => _field.DataField!; //we check nullability in the constructor

    public IEnumerable<object> Data
    {
        get
        {
            if (_field.ClrType == typeof(TimeOnly))
            {
                if (_data is int[] intValues)
                    return Expand(intValues, DataField.MaxDefinitionLevel, DefinitionLevels, value => ConvertTimeValue(value));

                if (_data is long[] longValues)
                    return Expand(longValues, DataField.MaxDefinitionLevel, DefinitionLevels, value => ConvertTimeValue(value));

                //Not sure if it makes sense to try read anyway at this point but going to for now.
                return ExpandUntyped(_data, DataField.MaxDefinitionLevel, DefinitionLevels);
            }

            return _data switch
            {
                bool[] values => Expand(values, DataField.MaxDefinitionLevel, DefinitionLevels, null),
                sbyte[] values => Expand(values, DataField.MaxDefinitionLevel, DefinitionLevels, null),
                byte[] values => Expand(values, DataField.MaxDefinitionLevel, DefinitionLevels, null),
                short[] values => Expand(values, DataField.MaxDefinitionLevel, DefinitionLevels, null),
                ushort[] values => Expand(values, DataField.MaxDefinitionLevel, DefinitionLevels, null),
                int[] values => Expand(values, DataField.MaxDefinitionLevel, DefinitionLevels, null),
                uint[] values => Expand(values, DataField.MaxDefinitionLevel, DefinitionLevels, null),
                long[] values => Expand(values, DataField.MaxDefinitionLevel, DefinitionLevels, null),
                ulong[] values => Expand(values, DataField.MaxDefinitionLevel, DefinitionLevels, null),
                float[] values => Expand(values, DataField.MaxDefinitionLevel, DefinitionLevels, null),
                double[] values => Expand(values, DataField.MaxDefinitionLevel, DefinitionLevels, null),
                decimal[] values => Expand(values, DataField.MaxDefinitionLevel, DefinitionLevels, null),
                DateTime[] values => Expand(values, DataField.MaxDefinitionLevel, DefinitionLevels, null),
                DateOnly[] values => Expand(values, DataField.MaxDefinitionLevel, DefinitionLevels, null),
                Guid[] values => Expand(values, DataField.MaxDefinitionLevel, DefinitionLevels, null),
                BigDecimal[] values => Expand(values, DataField.MaxDefinitionLevel, DefinitionLevels, null), //TODO: Is it possible to handle these? Seems we get regular `decimal` as the ClrType so this is never called

                ReadOnlyMemory<char>[] values => Expand(values, DataField.MaxDefinitionLevel, DefinitionLevels,
                    static memory => new string(memory.Span)),
                ReadOnlyMemory<byte>[] values => Expand(values, DataField.MaxDefinitionLevel, DefinitionLevels,
                    static memory => memory.ToArray()),

                //An element type we have no case for. Try to read generically
                _ => ExpandUntyped(_data, DataField.MaxDefinitionLevel, DefinitionLevels)
            };
        }
    }

    public int[]? DefinitionLevels { get; }

    public int[]? RepetitionLevels { get; }

    public ParquetColumnData(ParquetSchemaElement field, Array data, int[]? definitionLevels, int[]? repetitionLevels)
    {
        ArgumentNullException.ThrowIfNull(field);
        ArgumentNullException.ThrowIfNull(data);

        if (field.DataField is null)
            throw new ArgumentException("No data field found", nameof(field));
        if (definitionLevels is not null && definitionLevels.Length != data.Length)
            throw new ArgumentException("Data length doesn't match definition level length", nameof(data));
        if (repetitionLevels is not null && repetitionLevels.Length != data.Length)
            throw new ArgumentException("Data length doesn't match repetition level length", nameof(data));

        _data = data;
        _field = field;
        DefinitionLevels = definitionLevels;
        RepetitionLevels = repetitionLevels;
    }

    /// <summary>
    /// Parquet stores nulls implicitly (they are simply absent from the data pages), so the raw values are
    /// "compacted". This logic re-inserts them using the definition levels so that values line up 1:1 with
    /// the definition/repetition levels, which is what the rest of the engine expects.
    /// </summary>
    private static IEnumerable<object> Expand<T>(T[] values, int maxDefinitionLevel, int[]? definitionLevels,
        Func<T, object?>? convert)
    {
        var valueIndex = 0;
        var numValues = definitionLevels?.Length ?? values.Length;
        for (var index = 0; index < numValues; index++)
        {
            if (definitionLevels is not null && definitionLevels[index] < maxDefinitionLevel)
            {
                yield return DBNull.Value; //Null or empty
                continue;
            }

            var value = values[valueIndex++];
            object? converted = convert is null ? value : convert(value);
            yield return converted ?? DBNull.Value;
        }
    }

    /// <summary>
    /// Fallback for element types not covered above. Just in-case.
    /// </summary>
    private static IEnumerable<object> ExpandUntyped(Array values, int maxDefinitionLevel, int[]? definitionLevels)
    {
        var valueIndex = 0;
        var numValues = definitionLevels?.Length ?? values.Length;
        for (var index = 0; index < numValues; index++)
        {
            if (definitionLevels is not null && definitionLevels[index] < maxDefinitionLevel)
            {
                yield return DBNull.Value; //Null or empty
                continue;
            }

            var value = values.GetValue(valueIndex++);
            yield return value ?? DBNull.Value; //We must replace all nulls with DBNull
        }
    }

    /// <summary>
    /// TIME columns are read as a raw <see cref="int"/> (millis) or <see cref="long"/> (micros/nanos)
    /// count of time since midnight. Parquet.Net v6 removed the options that used to do this mapping,
    /// so we do it here to keep <see cref="TimeOnly"/> flowing through the rest of the app.
    /// </summary>
    private TimeOnly ConvertTimeValue<T>(T value) where T : INumber<T>
    {
        if (DataField is not TimeDataField timeField)
            throw new InvalidOperationException("Not a time type.");

        return timeField.Precision switch
        {
            TimeUnitPrecision.Millis => TimeOnly.FromTimeSpan(TimeSpan.FromMilliseconds(int.CreateChecked(value))),
            TimeUnitPrecision.Micros => TimeOnly.FromTimeSpan(TimeSpan.FromMicroseconds(long.CreateChecked(value))),
            TimeUnitPrecision.Nanos => TimeOnly.FromTimeSpan(TimeSpan.FromTicks(long.CreateChecked(value) / 100)),
            _ => throw new InvalidDataException($"'{timeField.Precision}' is not a recognized time unit.")
        };
    }
}