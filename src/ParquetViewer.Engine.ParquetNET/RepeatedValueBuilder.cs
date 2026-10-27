using ParquetViewer.Engine.Exceptions;

namespace ParquetViewer.Engine.ParquetNET;

/// <summary>
/// Base for builders that assemble one value per row (or per element of a parent collection) out of a leaf
/// column's repetition and definition levels.
/// </summary>
/// <remarks>
/// A new element starts at every level whose repetition level is at or below the parent repetition boundary
/// (0 = a database row; deeper = an entry of a parent collection). Element ranges are streamed rather than
/// collected so memory use doesn't grow with the size of the row group.
/// </remarks>
public abstract class RepeatedValueBuilder<T> where T : class
{
    private readonly int _parentRepetitionBoundary;

    protected int[] RepetitionLevels { get; }

    protected int[] DefinitionLevels { get; }

    protected FieldLevels Levels { get; }

    protected RepeatedValueBuilder(int parentRepetitionBoundary, FieldLevels levels, int[] repetitionLevels, int[] definitionLevels)
    {
        ArgumentNullException.ThrowIfNull(levels);
        ArgumentNullException.ThrowIfNull(repetitionLevels);
        ArgumentNullException.ThrowIfNull(definitionLevels);

        if (repetitionLevels.Length != definitionLevels.Length)
            throw new ParquetEngineException("Definition levels and repetition levels must be the same length");

        _parentRepetitionBoundary = parentRepetitionBoundary;
        Levels = levels;
        RepetitionLevels = repetitionLevels;
        DefinitionLevels = definitionLevels;
    }

    /// <summary>
    /// Reads one value (or <c>null</c>) per row, skipping and taking whole rows.
    /// </summary>
    /// <remarks>Callers will need to convert nulls to DBNull</remarks>
    public IEnumerable<T?> ReadRows(long skipRecords, long takeRecords, CancellationToken cancellationToken)
    {
        long rowIndex = 0;
        foreach (var range in GetRanges())
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (rowIndex >= skipRecords + takeRecords)
                yield break;

            if (rowIndex >= skipRecords)
                yield return Build(range);
            else
                Skip(range);

            rowIndex++;
        }
    }

    /// <summary>
    /// Builds the value whose levels span <paramref name="range"/>.
    /// </summary>
    protected abstract T? Build(Range range);

    /// <summary>
    /// Called instead of <see cref="Build"/> for ranges that aren't read, for builders that need to keep track of their position.
    /// </summary>
    protected virtual void Skip(Range range) { }

    /// <summary>
    /// Splits the levels into one range per element.
    /// </summary>
    protected IEnumerable<Range> GetRanges()
    {
        if (RepetitionLevels.Length == 0)
            yield break;

        int start = 0;
        for (int i = 1; i < RepetitionLevels.Length; i++)
        {
            if (RepetitionLevels[i] <= _parentRepetitionBoundary)
            {
                yield return new Range(start, i);
                start = i;
            }
        }
        yield return new Range(start, RepetitionLevels.Length);
    }
}