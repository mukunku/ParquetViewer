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
/// Null/empty classification is derived from the key leaf's <see cref="FieldLevels"/>: a definition
/// level below <c>MaxDefinitionLevel - 1</c> means the map is null, exactly <c>MaxDefinitionLevel - 1</c>
/// means it is present but empty, and <c>&gt;= MaxDefinitionLevel</c> means the entry exists.
/// </para>
/// </remarks>
public class MapValueBuilder
{
    private readonly int[] _repetitionLevels;
    private readonly int[] _definitionLevels;
    private readonly object[] _keyValues;
    private readonly Type _keyType;
    private readonly Type _valueType;

    //The primitive value side. Mutually exclusive with _valueBuilder.
    private readonly object[]? _valueValues;

    //The nested map value side. _valueRanges is the nested builder's parent-element ranges; it is
    //indexed by this builder's key-record index (each key record corresponds to one value element).
    private readonly MapValueBuilder? _valueBuilder;
    private readonly Range[]? _valueRanges;

    private readonly int _entryDefinitionLevel;
    private readonly int _parentRepetitionBoundary;

    public MapValueBuilder(
        int parentRepetitionBoundary,
        FieldLevels keyLevels,
        int[] repetitionLevels,
        int[] definitionLevels,
        object[] keyValues,
        Type keyType,
        Type valueType,
        object[]? valueValues = null,
        MapValueBuilder? valueBuilder = null)
    {
        ArgumentNullException.ThrowIfNull(keyLevels);
        ArgumentNullException.ThrowIfNull(repetitionLevels);
        ArgumentNullException.ThrowIfNull(definitionLevels);
        ArgumentNullException.ThrowIfNull(keyValues);
        ArgumentNullException.ThrowIfNull(keyType);
        ArgumentNullException.ThrowIfNull(valueType);

        if (valueValues is null == valueBuilder is null)
            throw new ArgumentException("Exactly one of the primitive value or the nested value builder must be provided");

        if (repetitionLevels.Length != definitionLevels.Length || definitionLevels.Length != keyValues.Length)
            throw new ParquetEngineException("Map key data, definition levels and repetition levels must all be the same length");

        _parentRepetitionBoundary = parentRepetitionBoundary;
        _repetitionLevels = repetitionLevels;
        _definitionLevels = definitionLevels;
        _keyValues = keyValues;
        _keyType = keyType;
        _valueType = valueType;
        _valueValues = valueValues;
        _valueBuilder = valueBuilder;

        //A key leaf sits one definition level below its own repeated key_value group, so the group's
        //"present" level is this leaf's max definition level and the map's null/empty levels sit just below it.
        _entryDefinitionLevel = keyLevels.MaxDefinitionLevel;

        _valueRanges = valueBuilder?.GetParentRanges();
    }

    /// <summary>
    /// Reads one <see cref="MapValue"/> (or <c>null</c>) per row, skipping and taking whole rows.
    /// </summary>
    /// <remarks>Callers will need to convert nulls to DBNull</remarks>
    public IEnumerable<MapValue?> ReadRows(long skipRecords, long takeRecords, CancellationToken cancellationToken)
    {
        var parentRanges = GetParentRanges();
        long rowIndex = 0;
        foreach (var range in parentRanges)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (rowIndex >= skipRecords + takeRecords)
                yield break;

            if (rowIndex >= skipRecords)
                yield return BuildMapValue(range);

            rowIndex++;
        }
    }

    /// <summary>
    /// Splits the key records into parent elements. A new element starts at every record whose
    /// repetition level is at or below <see cref="_parentRepetitionBoundary"/> (0 = a database row;
    /// deeper = an entry of a parent map).
    /// </summary>
    private Range[] GetParentRanges()
    {
        if (_repetitionLevels.Length == 0)
            return [];

        var ranges = new List<Range>();
        int start = 0;
        for (int i = 1; i < _repetitionLevels.Length; i++)
        {
            if (_repetitionLevels[i] <= _parentRepetitionBoundary)
            {
                ranges.Add(new Range(start, i));
                start = i;
            }
        }
        ranges.Add(new Range(start, _repetitionLevels.Length));
        return ranges.ToArray();
    }

    private MapValue? BuildMapValue(Range range)
    {
        var start = range.Start.Value;
        var end = range.End.Value;
        var definitionLevel = _definitionLevels[start];

        if (definitionLevel < _entryDefinitionLevel - 1)
        {
            //The map itself is null
            return null;
        }

        if (definitionLevel == _entryDefinitionLevel - 1)
        {
            //The map is present but has no entries
            return new MapValue([], _keyType, [], _valueType);
        }

        var keys = new ArrayList(end - start);
        var values = new ArrayList(end - start);
        for (var index = start; index < end; index++)
        {
            keys.Add(_keyValues[index]);
            values.Add(GetEntryValue(index));
        }

        return new MapValue(keys, _keyType, values, _valueType);
    }

    private object GetEntryValue(int index)
    {
        if (_valueBuilder is not null)
        {
            var nestedRange = _valueRanges![index];
            return _valueBuilder.BuildMapValue(nestedRange) as object ?? DBNull.Value;
        }

        return _valueValues![index];
    }
}