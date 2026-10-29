namespace ParquetViewer.Engine.ParquetNET;

/// <summary>
/// Reads the values of a single leaf column.
/// </summary>
internal sealed class PrimitiveValueBuilder(LeafCursor leaf, Type valueType) : FieldValueBuilder
{
    private readonly LeafCursor[] _leaves = [leaf];

    public override Type ValueType => valueType;

    public override LeafCursor FirstLeaf => leaf;

    public override IReadOnlyList<LeafCursor> Leaves => _leaves;

    public override object Read()
    {
        var value = leaf.Value;
        leaf.Advance();
        return value;
    }
}
