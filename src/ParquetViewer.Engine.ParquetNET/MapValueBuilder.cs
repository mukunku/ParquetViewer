using ParquetViewer.Engine.Exceptions;
using ParquetViewer.Engine.Types;
using System.Collections;

namespace ParquetViewer.Engine.ParquetNET;

/// <summary>
/// Builds <see cref="MapValue"/>s from the leaf columns of a Parquet MAP, the map-shaped sibling of
/// <see cref="ListValueBuilder"/>.
/// </summary>
/// <remarks>
/// <para>
/// A map is a repeated <c>key_value</c> group, so its keys and values are two parallel streams. For
/// primitive values the key leaf drives the entries and the value leaf is index-aligned with it. When
/// the value is itself a map its leaf columns sit one repetition level deeper than the key leaf, so it
/// is built by a nested <see cref="MapValueBuilder"/> where each outer entry becomes a "parent element"
/// of the nested builder.
/// </para>
/// <para>
/// Null/empty classification is derived from the key leaf's <see cref="FieldLevels"/>, at the depth of
/// the map's own repeated <c>key_value</c> group.
/// </para>
/// </remarks>
public class MapValueBuilder : RepeatedValueBuilder<MapValue>
{
    private readonly IReadOnlyList<object> _keys;
    private readonly Type _keyType;
    private readonly Type _valueType;

    //The primitive value side. Mutually exclusive with _valueBuilder.
    private readonly IReadOnlyList<object>? _values;

    //The nested map value side. Each of this builder's key records corresponds to one element of the nested
    //builder, in order, so we walk the nested builder's ranges in lockstep with our own key records.
    private readonly MapValueBuilder? _valueBuilder;
    private IEnumerator<Range>? _valueRanges;

    public MapValueBuilder(
        int parentRepetitionBoundary,
        FieldLevels keyLevels,
        int[] repetitionLevels,
        int[] definitionLevels,
        IReadOnlyList<object> keys,
        Type keyType,
        Type valueType,
        IReadOnlyList<object>? values = null,
        MapValueBuilder? valueBuilder = null)
        : base(parentRepetitionBoundary, keyLevels, repetitionLevels, definitionLevels)
    {
        ArgumentNullException.ThrowIfNull(keys);
        ArgumentNullException.ThrowIfNull(keyType);
        ArgumentNullException.ThrowIfNull(valueType);

        if (values is null == valueBuilder is null)
            throw new ArgumentException("Exactly one of the primitive value or the nested value builder must be provided");

        if (keys.Count != definitionLevels.Length || (values is not null && values.Count != keys.Count))
            throw new ParquetEngineException("Map key data, value data and levels must all be the same length");

        _keys = keys;
        _keyType = keyType;
        _valueType = valueType;
        _values = values;
        _valueBuilder = valueBuilder;
    }

    protected override MapValue? Build(Range range)
    {
        //The key leaf's innermost repeated ancestor is this map's key_value group
        var mapDepth = Levels.MaxRepetitionLevel;
        var definitionLevel = DefinitionLevels[range.Start.Value];

        if (Levels.IsNull(definitionLevel, mapDepth))
        {
            Skip(range);
            return null;
        }

        if (Levels.IsEmpty(definitionLevel, mapDepth))
        {
            Skip(range);
            return new MapValue([], _keyType, [], _valueType);
        }

        var keys = new ArrayList(range.End.Value - range.Start.Value);
        var values = new ArrayList(range.End.Value - range.Start.Value);
        for (var index = range.Start.Value; index < range.End.Value; index++)
        {
            keys.Add(_keys[index]);
            values.Add(_values is not null
                ? _values[index]
                : _valueBuilder!.Build(NextValueRange()) as object ?? DBNull.Value);
        }

        return new MapValue(keys, _keyType, values, _valueType);
    }

    protected override void Skip(Range range)
    {
        if (_valueBuilder is null)
            return;

        for (var index = range.Start.Value; index < range.End.Value; index++)
        {
            _valueBuilder.Skip(NextValueRange());
        }
    }

    private Range NextValueRange()
    {
        _valueRanges ??= _valueBuilder!.GetRanges().GetEnumerator();
        if (!_valueRanges.MoveNext())
            throw new ParquetEngineException("Nested map values ended before their parent map's entries. Your file might be malformed.");

        return _valueRanges.Current;
    }
}
