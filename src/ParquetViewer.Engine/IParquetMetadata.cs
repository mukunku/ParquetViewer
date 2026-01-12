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
        int ColumnCount { get; }
        ICollection<ISortingColumnMetadata>? SortingColumns { get; }
        ICollection<IRowGroupColumnMetadata>? Columns { get; }
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

    public interface IRowGroupColumnMetadata
    {
        public int? ColumnId { get; }
        public string? PathInSchema { get; }
        public string? Type { get; }
        public int? NumValues { get; }
        public long? TotalUncompressedSize { get; }
        public long? TotalCompressedSize { get; }
        public long? DataPageOffset { get; }
        public long? IndexPageOffset { get; }
        public long? DictionaryPageOffset { get; }
        public IRowGroupColumnStatistics? Statistics { get; }
        public long? BloomFilterOffset { get; }
        public long? BloomFilterLength { get; }
    }

    public interface IRowGroupColumnStatistics
    {
        public object? Min { get; }
        public object? Max { get; }
        public long? NullCount { get; }
        public long? DistinctCount { get; }
        public object? MinValue { get; }
        public object? MaxValue { get; }
        public bool? IsMinValueExact { get; }
        public bool? IsMaxValueExact { get; }
    }
}