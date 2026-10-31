namespace ParquetViewer.Engine.ParquetNET;

/// <summary>
/// Builds the values of one schema node from the leaf columns beneath it. Builders mirror the schema
/// (a list builder holds its element's builder, a struct builder its fields' builders, etc.) so any
/// nesting of lists, maps and structs can be assembled.
/// </summary>
/// <remarks>
/// <para>
/// Every leaf has a <see cref="LeafCursor"/>, and <see cref="Read"/> consumes exactly one instance of the
/// node from each leaf beneath it. Instances are read in order, so each builder's leaves stay in step.
/// </para>
/// <para>
/// Whether an instance is null or empty, and where a collection ends, is decided from the levels of the
/// node's <see cref="FirstLeaf"/>: every leaf beneath a node agrees on that. A null or empty instance takes
/// up exactly one level entry in every leaf beneath it.
/// </para>
/// </remarks>
internal abstract class FieldValueBuilder
{
    /// <summary>
    /// The type of the (non-null) values <see cref="Read"/> returns.
    /// </summary>
    public abstract Type ValueType { get; }

    /// <summary>
    /// The leaf whose levels decide whether an instance is null or empty, and where a collection ends.
    /// </summary>
    public abstract LeafCursor FirstLeaf { get; }

    /// <summary>
    /// Every leaf beneath this node.
    /// </summary>
    public abstract IReadOnlyList<LeafCursor> Leaves { get; }

    /// <summary>
    /// Builds the next instance (or <see cref="DBNull"/>) and moves past it.
    /// </summary>
    public abstract object Read();

    /// <summary>
    /// Reads one value per row, skipping and taking whole rows.
    /// </summary>
    public IEnumerable<object> ReadRows(long skipRecords, long takeRecords, CancellationToken cancellationToken)
    {
        foreach (var leaf in Leaves)
        {
            leaf.SkipRows(skipRecords);
        }

        for (long row = 0; row < takeRecords && !FirstLeaf.IsAtEnd; row++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return Read();
        }
    }

    /// <summary>
    /// Moves past a null or empty instance, which takes up exactly one level entry in every leaf.
    /// </summary>
    protected void SkipEmptyInstance()
    {
        foreach (var leaf in Leaves)
        {
            leaf.Advance();
        }
    }
}