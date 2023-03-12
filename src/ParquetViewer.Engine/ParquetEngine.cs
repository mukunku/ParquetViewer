using Parquet;
using Parquet.Schema;
using ParquetViewer.Engine.Exceptions;

namespace ParquetViewer.Engine
{
    public partial class ParquetEngine : IDisposable
    {
        private readonly List<ParquetReader> _parquetFiles;
        private long? _recordCount;

        public long RecordCount => _recordCount ??= _parquetFiles.Sum(pf => pf.ThriftMetadata.Num_rows);

        public List<string> Fields => _parquetFiles.FirstOrDefault()?.Schema.Fields.Select(f => f.Name).ToList() ?? new();

        public Parquet.Thrift.FileMetaData ThriftMetadata => _parquetFiles?.FirstOrDefault()?.ThriftMetadata ?? new();

        public Dictionary<string, string> CustomMetadata => _parquetFiles?.FirstOrDefault()?.CustomMetadata ?? new();

        public ParquetSchema Schema => _parquetFiles?.FirstOrDefault()?.Schema ?? new();

        public string OpenFileOrFolderPath { get; }

        private ParquetEngine(string fileOrFolderPath, List<ParquetReader> parquetFiles)
        {
            _parquetFiles = parquetFiles ?? new List<ParquetReader>();
            OpenFileOrFolderPath = fileOrFolderPath;
        }

        public Task<ParquetEngine> CloneAsync(CancellationToken cancellationToken)
        {
            return OpenFileOrFolderAsync(this.OpenFileOrFolderPath, cancellationToken);
        }

        public static Task<ParquetEngine> OpenFileOrFolderAsync(string fileOrFolderPath, CancellationToken cancellationToken)
        {
            if (File.Exists(fileOrFolderPath)) //Handles null
            {
                return OpenFileAsync(fileOrFolderPath, cancellationToken);
            }
            else if (Directory.Exists(fileOrFolderPath)) //Handles null
            {
                return OpenFolderAsync(fileOrFolderPath, cancellationToken);
            }
            else
            {
                throw new FileNotFoundException($"Could not find file or folder at location: {fileOrFolderPath}");
            }
        }

        public static async Task<ParquetEngine> OpenFileAsync(string parquetFilePath, CancellationToken cancellationToken)
        {
            if (!File.Exists(parquetFilePath)) //Handles null
            {
                throw new FileNotFoundException($"Could not find parquet file at: {parquetFilePath}");
            }

            try
            {
                var parquetReader = await ParquetReader.CreateAsync(parquetFilePath, null, cancellationToken);
                return new ParquetEngine(parquetFilePath, new List<ParquetReader> { parquetReader });
            }
            catch (Exception ex)
            {
                throw new FileReadException(ex);
            }
        }

        public static async Task<ParquetEngine> OpenFolderAsync(string folderPath, CancellationToken cancellationToken)
        {
            if (!Directory.Exists(folderPath)) //Handles null
            {
                throw new DirectoryNotFoundException($"Directory doesn't exist: {folderPath}");
            }

            var skippedFiles = new Dictionary<string, Exception>();
            var fileGroups = new Dictionary<ParquetSchema, List<ParquetReader>>();
            foreach (var file in ListParquetFiles(folderPath))
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    var parquetReader = await ParquetReader.CreateAsync(file, null, cancellationToken);
                    if (!fileGroups.ContainsKey(parquetReader.Schema))
                    {
                        fileGroups.Add(parquetReader.Schema, new List<ParquetReader>());
                    }

                    fileGroups[parquetReader.Schema].Add(parquetReader);
                }
                catch (Exception ex)
                {
                    skippedFiles.Add(Path.GetFileName(file), ex);
                }
            }

            if (fileGroups.Keys.Count == 0)
            {
                if (skippedFiles.Count == 0)
                {
                    throw new FileNotFoundException("Directory is empty");
                }
                else
                {
                    throw new AllFilesSkippedException(skippedFiles);
                }
            }
            else if (fileGroups.Keys.Count > 1)
            {
                //We found more than one type of schema.
                foreach (var fileGroupList in fileGroups.Values)
                {
                    EZDispose(fileGroupList);
                }

                throw new MultipleSchemasFoundException(fileGroups.Keys.ToList());
            }
            else if (skippedFiles.Count > 0)
            {
                //We found one schema but some files couldn't be read
                EZDispose(fileGroups.Values.First());
                throw new SomeFilesSkippedException(skippedFiles);
            }

            cancellationToken.ThrowIfCancellationRequested();

            return new ParquetEngine(folderPath, fileGroups.Values.First());
        }

        private IEnumerable<(long RemainingOffset, ParquetReader ParquetReader)> GetReaders(long offset)
        {
            foreach (var parquetFile in _parquetFiles)
            {
                if (offset >= parquetFile.ThriftMetadata.Num_rows)
                {
                    offset -= parquetFile.ThriftMetadata.Num_rows;
                    continue;
                }

                yield return (offset, parquetFile);
                offset = 0;
            }
        }

        private static IEnumerable<string> ListParquetFiles(string folderPath)
        {
            var parquetFiles = Directory.EnumerateFiles(folderPath, "*", SearchOption.AllDirectories)
                .Where(file =>
                        file.EndsWith(".parquet") ||
                        file.EndsWith(".parquet.gzip") ||
                        file.EndsWith(".parquet.gz")
                );

            if (parquetFiles.Count() == 0)
            {
                //Check for extensionless files
                parquetFiles = Directory.EnumerateFiles(folderPath);
            }

            return parquetFiles.OrderBy(filename => filename);
        }

        private static void EZDispose(IEnumerable<IDisposable> disposables)
        {
            if (disposables is null)
            {
                return;
            }

            foreach (var disposable in disposables)
            {
                try
                {
                    disposable?.Dispose();
                }
                catch { /* Swallow */ }
            }
        }

        public void Dispose()
        {
            if (_parquetFiles is not null)
            {
                foreach (var parquetFile in _parquetFiles)
                {
                    try
                    {
                        parquetFile?.Dispose();
                    }
                    catch { /* Swallow */ }
                }
            }
        }
    }
}
