using ParquetViewer.Engine.Types;
using System.Collections;

namespace ParquetViewer.Engine.ParquetNET;

/// <summary>
/// Builds <see cref="ListValue"/>s. A list is a repeated node whose every occurrence is one element, read
/// by the element's own builder.
/// </summary>
/// <param name="repeatedDefinitionLevel">The definition level at which the repeated node exists, i.e. the list
/// has at least one element. One level below that the list is empty, and anything lower means it's null.</param>
/// <param name="repeatedRepetitionLevel">The repetition level of the repeated node. A level entry repeating at
/// exactly this level is the next element of the same list.</param>
internal sealed class ListValueBuilder(int repeatedDefinitionLevel, int repeatedRepetitionLevel, FieldValueBuilder element)
    : FieldValueBuilder
{
    public override Type ValueType => typeof(ListValue);

    public override LeafCursor FirstLeaf => element.FirstLeaf;

    public override IReadOnlyList<LeafCursor> Leaves => element.Leaves;

    public override object Read()
    {
        var definitionLevel = FirstLeaf.DefinitionLevel;
        if (definitionLevel < repeatedDefinitionLevel)
        {
            SkipEmptyInstance();
            return definitionLevel < repeatedDefinitionLevel - 1
                ? DBNull.Value
                : new ListValue([], element.ValueType);
        }

        var items = new ArrayList();
        do
        {
            items.Add(element.Read());
        }
        while (!FirstLeaf.IsAtEnd && FirstLeaf.RepetitionLevel == repeatedRepetitionLevel);

        return new ListValue(items, element.ValueType);
    }
}