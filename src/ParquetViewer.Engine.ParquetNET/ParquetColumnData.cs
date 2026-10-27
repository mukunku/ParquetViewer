using Parquet.Schema;
using ParquetViewer.Engine.Types;
using System.Collections;
using System.Numerics;

namespace ParquetViewer.Engine.ParquetNET;

/// <summary>
/// Replaces <c>Parquet.Data.DataColumn</c>, which was removed in Parquet.Net v6.
/// V6 hands back pooled <see cref="Data.Span{T}"/> buffers that must be disposed of and
/// cannot be held onto, so we copy the values out into a managed array instead.
/// </summary>
/// <remarks>
/// The column is exposed as a list with one entry per definition/repetition level, which is what the rest
/// of the engine expects. Values are converted (and boxed) only when they are read, so a partial read
/// doesn't allocate anything for the values it doesn't touch.
/// </remarks>
internal abstract class ParquetColumnData : IReadOnlyList<object>
{
    private readonly int _maxDefinitionLevel;

    public int[]? DefinitionLevels { get; }

    public int[]? RepetitionLevels { get; }

    public int Count { get; }

    /// <summary>
    /// The value at <paramref name="index"/>, or <see cref="DBNull"/> if it's null or empty.
    /// </summary>
    public object this[int index]
    {
        get
        {
            if (DefinitionLevels is not null && DefinitionLevels[index] < _maxDefinitionLevel)
                return DBNull.Value; //Null or empty

            return GetDataValue(index);
        }
    }

    /// <summary>
    /// The type of the values this column hands out (after conversion), e.g. <see cref="ByteArrayValue"/> for binary columns.
    /// </summary>
    public abstract Type ValueType { get; }

    protected abstract object GetDataValue(int index);

    protected ParquetColumnData(ParquetSchemaElement field, int dataLength, int[]? definitionLevels, int[]? repetitionLevels)
    {
        ArgumentNullException.ThrowIfNull(field);

        if (field.DataField is null)
            throw new ArgumentException("No data field found", nameof(field));
        if (definitionLevels is not null && definitionLevels.Length != dataLength)
            throw new ArgumentException("Data length doesn't match definition level length", nameof(definitionLevels));
        if (repetitionLevels is not null && repetitionLevels.Length != dataLength)
            throw new ArgumentException("Data length doesn't match repetition level length", nameof(repetitionLevels));

        Count = dataLength;
        _maxDefinitionLevel = field.DataField.MaxDefinitionLevel;
        DefinitionLevels = definitionLevels;
        RepetitionLevels = repetitionLevels;
    }

    /// <summary>
    /// Wraps the raw values read for <paramref name="field"/> in the column type that knows how to convert them.
    /// </summary>
    public static ParquetColumnData Create(ParquetSchemaElement field, Array data, int[]? definitionLevels, int[]? repetitionLevels)
    {
        ArgumentNullException.ThrowIfNull(field);
        ArgumentNullException.ThrowIfNull(data);

        var isTime = field.ClrType == typeof(TimeOnly);

        //Use an exact type check instead of `is`/type-pattern checks. The CLR considers signed and unsigned
        //integer arrays of the same width mutually compatible (`uint[] is int[]` is true), so a `uint[]`
        //would match an `int[]` arm and get boxed as `int`.
        return data.GetType() switch
        {
            var t when t == typeof(int[]) && isTime => new TimeColumnData<int>(field, (int[])data, definitionLevels, repetitionLevels),
            var t when t == typeof(long[]) && isTime => new TimeColumnData<long>(field, (long[])data, definitionLevels, repetitionLevels),
            var t when t == typeof(ReadOnlyMemory<char>[]) => new StringColumnData(field, (ReadOnlyMemory<char>[])data, definitionLevels, repetitionLevels),
            var t when t == typeof(ReadOnlyMemory<byte>[]) => new BinaryColumnData(field, (ReadOnlyMemory<byte>[])data, definitionLevels, repetitionLevels),

            var t when t == typeof(bool[]) => new ParquetColumnData<bool>(field, (bool[])data, definitionLevels, repetitionLevels),
            var t when t == typeof(sbyte[]) => new ParquetColumnData<sbyte>(field, (sbyte[])data, definitionLevels, repetitionLevels),
            var t when t == typeof(byte[]) => new ParquetColumnData<byte>(field, (byte[])data, definitionLevels, repetitionLevels),
            var t when t == typeof(short[]) => new ParquetColumnData<short>(field, (short[])data, definitionLevels, repetitionLevels),
            var t when t == typeof(ushort[]) => new ParquetColumnData<ushort>(field, (ushort[])data, definitionLevels, repetitionLevels),
            var t when t == typeof(int[]) => new ParquetColumnData<int>(field, (int[])data, definitionLevels, repetitionLevels),
            var t when t == typeof(uint[]) => new ParquetColumnData<uint>(field, (uint[])data, definitionLevels, repetitionLevels),
            var t when t == typeof(long[]) => new ParquetColumnData<long>(field, (long[])data, definitionLevels, repetitionLevels),
            var t when t == typeof(ulong[]) => new ParquetColumnData<ulong>(field, (ulong[])data, definitionLevels, repetitionLevels),
            var t when t == typeof(float[]) => new ParquetColumnData<float>(field, (float[])data, definitionLevels, repetitionLevels),
            var t when t == typeof(double[]) => new ParquetColumnData<double>(field, (double[])data, definitionLevels, repetitionLevels),
            var t when t == typeof(decimal[]) => new ParquetColumnData<decimal>(field, (decimal[])data, definitionLevels, repetitionLevels),
            var t when t == typeof(DateTime[]) => new ParquetColumnData<DateTime>(field, (DateTime[])data, definitionLevels, repetitionLevels),
            var t when t == typeof(DateOnly[]) => new ParquetColumnData<DateOnly>(field, (DateOnly[])data, definitionLevels, repetitionLevels),
            var t when t == typeof(Guid[]) => new ParquetColumnData<Guid>(field, (Guid[])data, definitionLevels, repetitionLevels),

            //An element type we have no case for. Try to read generically, just in-case.
            _ => (ParquetColumnData)Activator.CreateInstance(
                typeof(ParquetColumnData<>).MakeGenericType(data.GetType().GetElementType()!),
                field, data, definitionLevels, repetitionLevels)!
        };
    }

    public IEnumerator<object> GetEnumerator()
    {
        for (var index = 0; index < Count; index++)
        {
            yield return this[index];
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

internal class ParquetColumnData<T> : ParquetColumnData where T : struct
{
    private readonly T[] _data;

    public ParquetColumnData(ParquetSchemaElement field, T[] data, int[]? definitionLevels, int[]? repetitionLevels)
        : base(field, data.Length, definitionLevels, repetitionLevels)
    {
        if (definitionLevels is not null)
        {
            //Parquet stores nulls implicitly (they are simply absent from the data pages), so the reader packs
            //the values at the front of the array. Move each value to its level index so the two line up 1:1.
            //Walking backwards is safe because a value only ever moves to the same or a later slot.
            var maxDefinitionLevel = field.DataField!.MaxDefinitionLevel;
            var dataIndex = definitionLevels.Count(level => level >= maxDefinitionLevel) - 1;
            for (var index = data.Length - 1; index >= 0; index--)
            {
                data[index] = definitionLevels[index] >= maxDefinitionLevel ? data[dataIndex--] : default;
            }
        }

        _data = data;
    }

    public override Type ValueType => typeof(T);

    protected override object GetDataValue(int index) => Convert(_data[index]);

    protected virtual object Convert(T value) => value;
}

internal sealed class StringColumnData(ParquetSchemaElement field, ReadOnlyMemory<char>[] data, int[]? definitionLevels, int[]? repetitionLevels)
    : ParquetColumnData<ReadOnlyMemory<char>>(field, data, definitionLevels, repetitionLevels)
{
    public override Type ValueType => typeof(string);

    protected override object Convert(ReadOnlyMemory<char> value) => new string(value.Span);
}

internal sealed class BinaryColumnData(ParquetSchemaElement field, ReadOnlyMemory<byte>[] data, int[]? definitionLevels, int[]? repetitionLevels)
    : ParquetColumnData<ReadOnlyMemory<byte>>(field, data, definitionLevels, repetitionLevels)
{
    public override Type ValueType => typeof(ByteArrayValue);

    protected override object Convert(ReadOnlyMemory<byte> value) => new ByteArrayValue(value.ToArray());
}

/// <summary>
/// TIME columns are read as a raw <see cref="int"/> (millis) or <see cref="long"/> (micros/nanos)
/// count of time since midnight. Parquet.Net v6 removed the options that used to do this mapping,
/// so we do it here to keep <see cref="TimeOnly"/> flowing through the rest of the app.
/// </summary>
internal sealed class TimeColumnData<T> : ParquetColumnData<T> where T : struct, INumber<T>
{
    private readonly TimeUnitPrecision _precision;

    public TimeColumnData(ParquetSchemaElement field, T[] data, int[]? definitionLevels, int[]? repetitionLevels)
        : base(field, data, definitionLevels, repetitionLevels)
    {
        if (field.DataField is not TimeDataField timeField)
            throw new InvalidOperationException("Not a time type.");

        _precision = timeField.Precision;
    }

    public override Type ValueType => typeof(TimeOnly);

    protected override object Convert(T value) => _precision switch
    {
        TimeUnitPrecision.Millis => TimeOnly.FromTimeSpan(TimeSpan.FromMilliseconds(int.CreateChecked(value))),
        TimeUnitPrecision.Micros => TimeOnly.FromTimeSpan(TimeSpan.FromMicroseconds(long.CreateChecked(value))),
        TimeUnitPrecision.Nanos => TimeOnly.FromTimeSpan(TimeSpan.FromTicks(long.CreateChecked(value) / 100)),
        _ => throw new InvalidDataException($"'{_precision}' is not a recognized time unit.")
    };
}