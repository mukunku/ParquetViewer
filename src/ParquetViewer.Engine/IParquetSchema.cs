namespace ParquetViewer.Engine
{
    public interface IParquetSchema
    {
        IReadOnlyList<string> Fields { get; }
    }
}
