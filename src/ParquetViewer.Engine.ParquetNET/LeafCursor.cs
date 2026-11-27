using ParquetViewer.Engine.Exceptions;

namespace ParquetViewer.Engine.ParquetNET;

/// <summary>
/// A position in one leaf column's values and repetition/definition levels. Builders move it forward as
/// they consume level entries.
/// </summary>
internal sealed class LeafCursor
{
    private readonly IReadOnlyList<object> _values;
    private readonly int[]? _repetitionLevels;
    private readonly int[]? _definitionLevels;
    private readonly int _maxDefinitionLevel;

    public int Position { get; private set; }

    public bool IsAtEnd => Position >= _values.Count;

    public int RepetitionLevel => _repetitionLevels?[Position] ?? 0;

    //A column without definition levels is required all the way down, so every entry is fully defined
    public int DefinitionLevel => _definitionLevels?[Position] ?? _maxDefinitionLevel;

    /// <summary>
    /// The value at the current position, or <see cref="DBNull"/> if it's null.
    /// </summary>
    public object Value => _values[Position];

    public LeafCursor(IReadOnlyList<object> values, int[]? repetitionLevels, int[]? definitionLevels, int maxDefinitionLevel)
    {
        ArgumentNullException.ThrowIfNull(values);

        if ((repetitionLevels is not null && repetitionLevels.Length != values.Count)
            || (definitionLevels is not null && definitionLevels.Length != values.Count))
            throw new ParquetEngineException("Leaf data and its levels must be the same length");

        _values = values;
        _repetitionLevels = repetitionLevels;
        _definitionLevels = definitionLevels;
        _maxDefinitionLevel = maxDefinitionLevel;
    }

    public void Advance() => Position++;

    /// <summary>
    /// Moves past <paramref name="rows"/> whole rows. A new row starts at every repetition level of 0.
    /// </summary>
    public void SkipRows(long rows)
    {
        if (_repetitionLevels is null)
        {
            Position = (int)Math.Min(_values.Count, Position + rows);
            return;
        }

        while (rows > 0 && !IsAtEnd)
        {
            Position++;
            if (IsAtEnd || _repetitionLevels[Position] == 0)
                rows--;
        }
    }
}