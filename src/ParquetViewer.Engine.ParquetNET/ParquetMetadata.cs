using Parquet.Meta;

namespace ParquetViewer.Engine.ParquetNET
{
    public class ParquetMetadata : IParquetMetadata
    {
        public int ParquetVersion { get; }

        public int RowGroupCount { get; }

        public string CreatedBy { get; }

        public ICollection<IRowGroupMetadata> RowGroups { get; }

        public IParquetSchemaElement SchemaTree { get; }

        public int RowCount { get; }

        public ParquetMetadata(FileMetaData thriftMetadata, ParquetSchemaElement schemaTree, int recordCount)
        {
            RowCount = recordCount;
            RowGroupCount = thriftMetadata.RowGroups.Count;
            ParquetVersion = thriftMetadata.Version;
            CreatedBy = thriftMetadata.CreatedBy ?? string.Empty;
            SchemaTree = schemaTree;

            List<RowGroupMetadata> rowGroupMetadataList = new();
            foreach (var rowGroup in thriftMetadata.RowGroups)
            {
                List<RowGroupColumnMetadata> columnMetadataList = new();
                foreach (var column in rowGroup.Columns)
                {
                    if (column.MetaData is null)
                        continue;

                    var columnMetadata = new RowGroupColumnMetadata(-1,
                        string.Join("/", column.MetaData.PathInSchema),
                        column.MetaData.Type.ToString(),
                        (int)column.MetaData.NumValues,
                        column.MetaData.TotalUncompressedSize,
                        column.MetaData.TotalCompressedSize,
                        column.MetaData.DataPageOffset,
                        column.MetaData.IndexPageOffset,
                        column.MetaData.DictionaryPageOffset,
                        column.MetaData.Statistics is not null ? new RowGroupColumnStatistics(
                            column.MetaData.Statistics.Min,
                            column.MetaData.Statistics.Max,
                            column.MetaData.Statistics.NullCount,
                            column.MetaData.Statistics.DistinctCount,
                            column.MetaData.Statistics.MinValue,
                            column.MetaData.Statistics.MaxValue,
                            column.MetaData.Statistics.IsMinValueExact,
                            column.MetaData.Statistics.IsMaxValueExact
                        ) : null,
                        column.MetaData.BloomFilterOffset,
                        column.MetaData.BloomFilterLength);

                    columnMetadataList.Add(columnMetadata);
                }

                rowGroupMetadataList.Add(new RowGroupMetadata(
                    rowGroup.Ordinal.HasValue ? (int)rowGroup.Ordinal.Value : -1,
                    (int)rowGroup.NumRows,
                    rowGroup.Columns.Count,
                    rowGroup.SortingColumns?.Select(sc => new SortingColumnMetadata(sc.ColumnIdx, sc.Descending, sc.NullsFirst))
                        .Cast<ISortingColumnMetadata>().ToList(),
                    columnMetadataList.ToList<IRowGroupColumnMetadata>(),
                    rowGroup.FileOffset ?? 0,
                    rowGroup.TotalByteSize,
                    rowGroup.TotalCompressedSize ?? 0));
            }

            RowGroups = rowGroupMetadataList.ToList<IRowGroupMetadata>();
        }
    }

    public class RowGroupMetadata : IRowGroupMetadata
    {
        public int Ordinal { get; }

        public int RowCount { get; }

        public int ColumnCount { get; }

        public ICollection<ISortingColumnMetadata>? SortingColumns { get; }

        public ICollection<IRowGroupColumnMetadata>? Columns { get; }

        public long FileOffset { get; }

        public long TotalByteSize { get; }

        public long TotalCompressedSize { get; }

        public RowGroupMetadata(int ordinal, int rowCount, int columnCount, ICollection<ISortingColumnMetadata>? sortingColumnMetadata,
            ICollection<IRowGroupColumnMetadata>? columns, long fileOffset, long totalByteSize, long totalCompressedSize)
        {
            Ordinal = ordinal;
            RowCount = rowCount;
            ColumnCount = columnCount;
            SortingColumns = sortingColumnMetadata;
            Columns = columns;
            FileOffset = fileOffset;
            TotalByteSize = totalByteSize;
            TotalCompressedSize = totalCompressedSize;
        }
    }

    public class SortingColumnMetadata : ISortingColumnMetadata
    {
        public int ColumnIdx { get; }
        public bool Descending { get; }
        public bool NullsFirst { get; }

        public SortingColumnMetadata(int columnIdx, bool descending, bool nullsFirst)
        {
            ColumnIdx = columnIdx;
            Descending = descending;
            NullsFirst = nullsFirst;
        }
    }

    public class RowGroupColumnMetadata : IRowGroupColumnMetadata
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

        public RowGroupColumnMetadata(
            int? columnId,
            string? pathInSchema,
            string? type,
            int? numValues,
            long? totalUncompressedSize,
            long? totalCompressedSize,
            long? dataPageOffset,
            long? indexPageOffset,
            long? dictionaryPageOffset,
            RowGroupColumnStatistics? statistics,
            long? bloomFilterOffset,
            long? bloomFilterLength)
        {
            ColumnId = columnId;
            PathInSchema = pathInSchema;
            Type = type;
            NumValues = numValues;
            TotalUncompressedSize = totalUncompressedSize;
            TotalCompressedSize = totalCompressedSize;
            DataPageOffset = dataPageOffset;
            IndexPageOffset = indexPageOffset;
            DictionaryPageOffset = dictionaryPageOffset;
            Statistics = statistics;
            BloomFilterOffset = bloomFilterOffset;
            BloomFilterLength = bloomFilterLength;
        }
    }

    public class RowGroupColumnStatistics : IRowGroupColumnStatistics
    {
        public object? Min { get; }
        public object? Max { get; }
        public long? NullCount { get; }
        public long? DistinctCount { get; }
        public object? MinValue { get; }
        public object? MaxValue { get; }
        public bool? IsMinValueExact { get; }
        public bool? IsMaxValueExact { get; }

        public RowGroupColumnStatistics(object? min, object? max, long? nullCount, long? distinctCount, object? minValue, object? maxValue, bool? isMinValueExact, bool? isMaxValueExact)
        {
            Min = min;
            Max = max;
            NullCount = nullCount;
            DistinctCount = distinctCount;
            MinValue = minValue;
            MaxValue = maxValue;
            IsMinValueExact = isMinValueExact;
            IsMaxValueExact = isMaxValueExact;
        }
    }
}
