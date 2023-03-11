using Parquet;
using Parquet.Schema;
using ParquetFileViewer.Exceptions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ParquetFileViewer.Helpers
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

        private ParquetEngine(List<ParquetReader> parquetFiles)
        {
            _parquetFiles = parquetFiles ?? new List<ParquetReader>();
        }

        public static Task<ParquetEngine> OpenFileOrFolderAsync(string fileOrFolderPath)
        {
            if (File.Exists(fileOrFolderPath)) //Handles null
            {
                return OpenFileAsync(fileOrFolderPath);
            }
            else if (Directory.Exists(fileOrFolderPath)) //Handles null
            {
                return OpenFolderAsync(fileOrFolderPath);
            }
            else
            {
                throw new FileNotFoundException($"Could not find file or folder at location: {fileOrFolderPath}");
            }
        }

        public static async Task<ParquetEngine> OpenFileAsync(string parquetFilePath)
        {
            if (!File.Exists(parquetFilePath)) //Handles null
            {
                throw new FileNotFoundException($"Could not find parquet file at: {parquetFilePath}");
            }

            ParquetReader parquetReader;
            try
            {
                parquetReader = await ParquetReader.CreateAsync(parquetFilePath);
            }
            catch (Exception ex)
            {
                throw new FileReadException(ex);
            }

            return new ParquetEngine(new List<ParquetReader> { parquetReader });
        }

        public static async Task<ParquetEngine> OpenFolderAsync(string folderPath)
        {
            if (!Directory.Exists(folderPath)) //Handles null
            {
                throw new DirectoryNotFoundException($"Directory doesn't exist: {folderPath}");
            }

            var skippedFiles = new Dictionary<string, Exception>();
            var fileGroups = new Dictionary<Parquet.Schema.ParquetSchema, List<ParquetReader>>();
            foreach (var file in ListParquetFiles(folderPath))
            {
                try
                {
                    var parquetReader = await ParquetReader.CreateAsync(file);
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

            return new ParquetEngine(fileGroups.Values.First());
        }

        public IEnumerable<(long RemainingOffset, ParquetReader ParquetReader)> GetReaders(long offset)
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
            var parquetFiles = Directory.EnumerateFiles(folderPath)
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
