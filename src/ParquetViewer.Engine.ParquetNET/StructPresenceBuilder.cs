namespace ParquetViewer.Engine.ParquetNET;

/// <summary>
/// For a List of Structs field, computes for each row the presence of each struct instance from one of the
/// struct's leaf columns. A struct is present when the definition level of its leaf reaches the struct's
/// <see cref="FieldLevels.MaxDefinitionLevel"/>.
/// </summary>
internal sealed class StructPresenceBuilder(FieldLevels structLevels, int[] leafRepetitionLevels, int[] leafDefinitionLevels)
    : RepeatedValueBuilder<List<bool>>(parentRepetitionBoundary: 0, structLevels, leafRepetitionLevels, leafDefinitionLevels)
{
    protected override List<bool> Build(Range range)
    {
        var presence = new List<bool>();
        for (var i = range.Start.Value; i < range.End.Value; i++)
        {
            //Entries with a repetition level at or below the struct's own repetition nesting are the
            //beginning of a new struct instance; deeper entries belong to nested lists within the same
            //struct instance.
            if (RepetitionLevels[i] <= Levels.MaxRepetitionLevel)
            {
                presence.Add(DefinitionLevels[i] >= Levels.MaxDefinitionLevel);
            }
        }
        return presence;
    }
}
