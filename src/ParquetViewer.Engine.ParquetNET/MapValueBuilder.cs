using ParquetViewer.Engine.Types;
using System.Collections;

namespace ParquetViewer.Engine.ParquetNET;

/// <summary>
/// Builds <see cref="MapValue"/>s, the map-shaped sibling of <see cref="ListValueBuilder"/>. A map is a
/// repeated <c>key_value</c> group whose every occurrence is one entry, read by the key and value builders.
/// </summary>
/// <param name="keyValueDefinitionLevel">The definition level at which the <c>key_value</c> group exists, i.e.
/// the map has at least one entry. One level below that the map is empty, and anything lower means it's null.</param>
/// <param name="keyValueRepetitionLevel">The repetition level of the <c>key_value</c> group.</param>
internal sealed class MapValueBuilder(int keyValueDefinitionLevel, int keyValueRepetitionLevel,
    FieldValueBuilder key, FieldValueBuilder value) : FieldValueBuilder
{
    private readonly LeafCursor[] _leaves = [.. key.Leaves, .. value.Leaves];

    public override Type ValueType => typeof(MapValue);

    public override LeafCursor FirstLeaf => key.FirstLeaf;

    public override IReadOnlyList<LeafCursor> Leaves => _leaves;

    public override object Read()
    {
        var definitionLevel = FirstLeaf.DefinitionLevel;
        if (definitionLevel < keyValueDefinitionLevel)
        {
            SkipEmptyInstance();
            return definitionLevel < keyValueDefinitionLevel - 1
                ? DBNull.Value
                : new MapValue([], key.ValueType, [], value.ValueType);
        }

        var keys = new ArrayList();
        var values = new ArrayList();
        do
        {
            keys.Add(key.Read());
            values.Add(value.Read());
        }
        while (!FirstLeaf.IsAtEnd && FirstLeaf.RepetitionLevel == keyValueRepetitionLevel);

        return new MapValue(keys, key.ValueType, values, value.ValueType);
    }
}
