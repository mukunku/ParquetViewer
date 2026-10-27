using Parquet.Meta;

namespace ParquetViewer.Engine.ParquetNET;

/// <summary>
/// The repetition and definition level arithmetic for a single schema element, derived purely from
/// the repetition types of the nodes on the path from the root.
/// </summary>
/// <remarks>
/// This is the one place in the engine that reasons about Dremel levels. Everything else can ask a
/// <see cref="FieldLevels"/> what a given definition level means for a field.
/// </remarks>
public sealed class FieldLevels
{
    public static readonly FieldLevels None = new(0, []);

    /// <summary>
    /// Derives the levels for <paramref name="element"/> by walking the parent chain (root -> element,
    /// outermost first) and accumulating definition levels from the repetition types.
    /// </summary>
    public static FieldLevels For(ParquetSchemaElement element)
    {
        ArgumentNullException.ThrowIfNull(element);

        //We capture definition levels starting from the root and walking towards the leaf node.
        //So we need to first walk towards the root from this element and then reverse the order.
        var path = new List<ParquetSchemaElement>();
        for (var node = element; node?.Parent is not null /*exclude root node*/; node = node.Parent)
        {
            path.Add(node);
        }
        path.Reverse(); //Now we have a root-first list

        var maxDefinitionLevel = 0;
        var repeatedAncestorDefinitionLevels = new List<int>();
        foreach (var node in path)
        {
            if (node.SchemaElement.RepetitionType != FieldRepetitionType.REQUIRED)
                maxDefinitionLevel++;

            //Any repeated node on the path is a collection frame. Standard lists, maps and legacy
            //two-tier repeated fields all encode their collection as a repeated node, so this single
            //rule covers them all. A repeated node is empty (zero occurrences) one definition level
            //below the level at which the repeated node itself becomes defined.
            if (node.SchemaElement.RepetitionType == FieldRepetitionType.REPEATED)
                repeatedAncestorDefinitionLevels.Add(maxDefinitionLevel - 1);
        }

        /*
           Example: a list<list<int>> in the standard three-tier layout.

          ┌────────────────┬────────────┬─────────────────────────┬─────────────────────────┐
          │  Schema node   │ Repetition │ Definition level so far │ Entry added to the list │
          ├────────────────┼────────────┼─────────────────────────┼─────────────────────────┤
          │ a (LIST)       │ optional   │ 1                       │                         │
          ├────────────────┼────────────┼─────────────────────────┼─────────────────────────┤
          │ list           │ repeated   │ 2                       │ 2 - 1 = 1               │
          ├────────────────┼────────────┼─────────────────────────┼─────────────────────────┤
          │ element (LIST) │ optional   │ 3                       │                         │
          ├────────────────┼────────────┼─────────────────────────┼─────────────────────────┤
          │ list           │ repeated   │ 4                       │ 4 - 1 = 3               │
          ├────────────────┼────────────┼─────────────────────────┼─────────────────────────┤
          │ element (int)  │ optional   │ 5                       │                         │
          └────────────────┴────────────┴─────────────────────────┴─────────────────────────┘

          The result is MaxDefinitionLevel = 5 and RepeatedAncestorDefinitionLevels = [1, 3]. 
          That's outer list first, then the inner list.

          Reading a definition level against that:
          - 0: the outer list is null (below [0] = 1).
          - 1: the outer list is empty.
          - 2: the inner list is null (below [1] = 3).
          - 3: the inner list is empty.
          - 4: the int element itself is null.
          - 5: a real value.
         */

        return new FieldLevels(maxDefinitionLevel, repeatedAncestorDefinitionLevels /* from root to*/);
    }

    internal FieldLevels(int maxDefinitionLevel, IReadOnlyList<int> repeatedAncestorDefinitionLevels)
    {
        MaxDefinitionLevel = maxDefinitionLevel;
        RepeatedAncestorDefinitionLevels = repeatedAncestorDefinitionLevels;
    }

    /// <summary>
    /// The definition level a value must reach to be fully present. Equal to the number of
    /// non-required (OPTIONAL or REPEATED) nodes from the root down to and including the element.
    /// Matches Parquet's <c>max_definition_level</c>.
    /// </summary>
    public int MaxDefinitionLevel { get; }

    /// <summary>
    /// For every repeated ancestor on the path (outermost first), the definition level at which
    /// that collection is "present but empty". A definition level below
    /// <c>RepeatedAncestorDefinitionLevels[definitionLevel]</c> means the collection at depth <c>definitionLevel</c> is null;
    /// equal to it means the collection is empty.
    /// </summary>
    public IReadOnlyList<int> RepeatedAncestorDefinitionLevels { get; }

    /// <summary>
    /// How deeply nested in repetitions this element is, i.e. the number of repeated ancestors.
    /// Matches the element's maximum repetition level (Parquet's <c>max_repetition_level</c>).
    /// </summary>
    /// <remarks>Used to be called "NumberOfListParents"</remarks>
    public int MaxRepetitionLevel => RepeatedAncestorDefinitionLevels.Count;

    /// <summary>
    /// True when the collection nested <paramref name="depth"/> levels deep (1-based, outermost first)
    /// is null for the given definition level. The default depth (1) is the value's outermost collection.
    /// </summary>
    public bool IsNull(int definitionLevel, int depth = 1)
        => depth <= RepeatedAncestorDefinitionLevels.Count
            && definitionLevel < RepeatedAncestorDefinitionLevels[depth - 1];

    /// <summary>
    /// True when the collection nested <paramref name="depth"/> levels deep (1-based, outermost first)
    /// is present but has no entries for the given definition level.
    /// </summary>
    public bool IsEmpty(int definitionLevel, int depth = 1)
        => depth <= RepeatedAncestorDefinitionLevels.Count
            && definitionLevel == RepeatedAncestorDefinitionLevels[depth - 1]
            && definitionLevel < MaxDefinitionLevel;

    public override string ToString()
        => $"MaxDefinitionLevel={MaxDefinitionLevel}, MaxRepetitionLevel={MaxRepetitionLevel}, RepeatedAncestorDefinitionLevels=[{string.Join(',', RepeatedAncestorDefinitionLevels)}]";
}