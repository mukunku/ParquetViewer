using System.Data;

namespace ParquetViewer.Engine
{
    public interface IParquetEngine : IDisposable
    {
        List<string> Fields { get; }
        long RecordCount { get; }
        int NumberOfPartitions { get; }
        Dictionary<string, string> CustomMetadata { get; }
        string Path { get; }
        IParquetMetadata Metadata { get; }

        Task<Func<bool, DataTable>> ReadRowsAsync(List<string> selectedFields, int offset, int recordCount,
            CancellationToken cancellationToken, IProgress<int>? progress = null);
    }
}