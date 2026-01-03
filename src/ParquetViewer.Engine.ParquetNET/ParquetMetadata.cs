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
            RowGroups = thriftMetadata.RowGroups
                .Select((rg, index) => new RowGroupMetadata((int)(rg.Ordinal ?? -1), (int)rg.NumRows, 
                    rg.SortingColumns?.Select(sc => new SortingColumnMetadata(sc.ColumnIdx, sc.Descending, sc.NullsFirst))
                    .Cast<ISortingColumnMetadata>().ToList(), rg.FileOffset ?? 0, rg.TotalByteSize, rg.TotalCompressedSize ?? 0))
                .ToList<IRowGroupMetadata>();
            CreatedBy = thriftMetadata.CreatedBy ?? string.Empty;
            SchemaTree = schemaTree;
        }
    }

    public class RowGroupMetadata : IRowGroupMetadata
    {
        public int Ordinal { get; }

        public int RowCount { get; }

        public ICollection<ISortingColumnMetadata>? SortingColumns { get; }

        public long FileOffset { get; }

        public long TotalByteSize { get; }

        public long TotalCompressedSize { get; }

        public RowGroupMetadata(int ordinal, int rowCount, ICollection<ISortingColumnMetadata>? sortingColumnMetadata,
            long fileOffset, long totalByteSize, long totalCompressedSize)
        {
            Ordinal = ordinal;
            RowCount = rowCount;
            SortingColumns = sortingColumnMetadata;
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
}
