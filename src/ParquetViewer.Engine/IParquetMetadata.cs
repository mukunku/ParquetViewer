namespace ParquetViewer.Engine
{
    public interface IParquetMetadata
    {
        int ParquetVersion { get; }
        int RowGroupCount { get; }
        int RowCount { get; }
        string CreatedBy { get; }
        ICollection<IRowGroupMetadata> RowGroups { get; }
        IParquetSchemaElement SchemaTree { get; }
    }

    public interface IRowGroupMetadata
    {
        int Ordinal { get; }
        int RowCount { get; }
        ICollection<ISortingColumnMetadata>? SortingColumns { get; }
        long FileOffset { get; }
        long TotalByteSize { get; }
        long TotalCompressedSize { get; }
    }

    public interface ISortingColumnMetadata
    {
        public int ColumnIdx { get; }
        public bool Descending { get; }
        public bool NullsFirst { get; }
    }
}
