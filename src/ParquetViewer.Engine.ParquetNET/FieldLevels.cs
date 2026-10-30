using Parquet.Meta;

namespace ParquetViewer.Engine.ParquetNET;

/// <summary>
/// The repetition and definition level arithmetic for a single schema element, derived purely from
/// the repetition types of the nodes on the path from the root.
/// </summary>
/// <remarks>
/// Derives the maximum definition and repetition levels of any node in our schema tree. Only leaves
/// carry Parquet.Net's DataField (and its levels); group nodes such as lists, maps and structs don't,
/// so their levels are worked out here. The value builders compare these against a leaf's per-value
/// levels to decide whether an instance is null, empty or present.
/// </remarks>
public sealed class FieldLevels
{
    /// <summary>
    /// Derives the levels for <paramref name="element"/> by walking the parent chain (root -> element,
    /// outermost first) and accumulating levels from the repetition types.
    /// </summary>
    public static FieldLevels For(ParquetSchemaElement element)
    {
        ArgumentNullException.ThrowIfNull(element);

        var maxDefinitionLevel = 0;
        var maxRepetitionLevel = 0;
        for (var node = element; node?.Parent is not null /*exclude root node*/; node = node.Parent)
        {
            if (node.SchemaElement.RepetitionType != FieldRepetitionType.REQUIRED)
                maxDefinitionLevel++;

            //Any repeated node on the path is a collection frame. Standard lists, maps and legacy
            //two-tier repeated fields all encode their collection as a repeated node, so this single
            //rule covers them all.
            if (node.SchemaElement.RepetitionType == FieldRepetitionType.REPEATED)
                maxRepetitionLevel++;
        }

        /*
          Example: a list<list<int>> in the standard three-tier layout.

          ┌────────────────┬────────────┬─────────────────────────┬─────────────────────────┐
          │  Schema node   │ Repetition │ Definition level so far │ Repetition level so far │
          ├────────────────┼────────────┼─────────────────────────┼─────────────────────────┤
          │ a (LIST)       │ optional   │ 1                       │ 0                       │
          ├────────────────┼────────────┼─────────────────────────┼─────────────────────────┤
          │ list           │ repeated   │ 2                       │ 1                       │
          ├────────────────┼────────────┼─────────────────────────┼─────────────────────────┤
          │ element (LIST) │ optional   │ 3                       │ 1                       │
          ├────────────────┼────────────┼─────────────────────────┼─────────────────────────┤
          │ list           │ repeated   │ 4                       │ 2                       │
          ├────────────────┼────────────┼─────────────────────────┼─────────────────────────┤
          │ element (int)  │ optional   │ 5                       │ 2                       │
          └────────────────┴────────────┴─────────────────────────┴─────────────────────────┘

          The result is MaxDefinitionLevel = 5 and MaxRepetitionLevel = 2.

          A repeated node is empty (zero occurrences) one definition level below the level at which it
          becomes defined. So reading a leaf's definition level against that:
          - 0: the outer list is null.
          - 1: the outer list is empty (its repeated node is defined at 2).
          - 2: the inner list is null.
          - 3: the inner list is empty (its repeated node is defined at 4).
          - 4: the int element itself is null.
          - 5: a real value.
         */

        return new FieldLevels(maxDefinitionLevel, maxRepetitionLevel);
    }

    internal FieldLevels(int maxDefinitionLevel, int maxRepetitionLevel)
    {
        MaxDefinitionLevel = maxDefinitionLevel;
        MaxRepetitionLevel = maxRepetitionLevel;
    }

    /// <summary>
    /// The definition level a value must reach to be fully present. Equal to the number of
    /// non-required (OPTIONAL or REPEATED) nodes from the root down to and including the element.
    /// Matches Parquet's <c>max_definition_level</c>.
    /// </summary>
    public int MaxDefinitionLevel { get; }

    /// <summary>
    /// How deeply nested in repetitions this element is, i.e. the number of repeated nodes from the root
    /// down to and including the element. Matches Parquet's <c>max_repetition_level</c>.
    /// </summary>
    public int MaxRepetitionLevel { get; }

    public override string ToString()
        => $"MaxDefinitionLevel={MaxDefinitionLevel}, MaxRepetitionLevel={MaxRepetitionLevel}";
}
