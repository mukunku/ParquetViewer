namespace ParquetViewer.Engine;

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
    int ColumnIdx { get; }
    bool Descending { get; }
    bool NullsFirst { get; }
}

public interface IRowGroupColumnMetadata
{
    int? ColumnId { get; }
    string? PathInSchema { get; }
    string? Type { get; }
    int? NumValues { get; }
    long? TotalUncompressedSize { get; }
    long? TotalCompressedSize { get; }
    long? DataPageOffset { get; }
    long? IndexPageOffset { get; }
    long? DictionaryPageOffset { get; }
    IRowGroupColumnStatistics? Statistics { get; }
    long? BloomFilterOffset { get; }
    long? BloomFilterLength { get; }
}

public interface IRowGroupColumnStatistics
{
    object? Min { get; }
    object? Max { get; }
    long? NullCount { get; }
    long? DistinctCount { get; }
    object? MinValue { get; }
    object? MaxValue { get; }
    bool? IsMinValueExact { get; }
    bool? IsMaxValueExact { get; }
}