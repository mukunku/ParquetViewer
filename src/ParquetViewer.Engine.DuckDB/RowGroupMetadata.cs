namespace ParquetViewer.Engine.DuckDB
{
    public class RowGroupMetadata : IRowGroupMetadata
    {
        public int Ordinal { get; }
        public int RowCount { get; }
        public ICollection<ISortingColumnMetadata>? SortingColumns { get; }
        public long FileOffset { get; }
        public long TotalByteSize { get; }
        public long TotalCompressedSize { get; }
        public RowGroupMetadata(int ordinal, int rowCount, long fileOffset, long totalByteSize, long totalCompressedSize)
        {
            this.Ordinal = ordinal;
            this.RowCount = rowCount;
            this.FileOffset = fileOffset;
            this.TotalByteSize = totalByteSize;
            this.TotalCompressedSize = totalCompressedSize;
            this.SortingColumns = null; //DuckDB doesn't seem to have info on this
        }
    }
}
