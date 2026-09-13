using System.Data;

namespace ParquetViewer.Engine;

public interface IParquetEngine : IDisposable
{
    List<string> Fields { get; }
    long RecordCount { get; }
    int NumberOfPartitions { get; }
    Dictionary<string, string> CustomMetadata { get; }
    string Path { get; }
    IParquetMetadata Metadata { get; }

    Task<Func<bool, DataTable>> ReadRowsAsync(List<string> selectedFields, int offset, int recordCount,
        IProgress<int>? progress = null, CancellationToken cancellationToken = default);

    Task WriteDataToParquetFileAsync(DataTable dataTable, string path, IProgress<int> progress,
        Dictionary<string, string>? customMetadata, CancellationToken cancellationToken);

    IEnumerable<string> GetOpenParquetFilePaths();
}