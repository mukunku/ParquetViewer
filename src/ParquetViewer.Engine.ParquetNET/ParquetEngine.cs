using Parquet;
using Parquet.Meta;
using Parquet.Schema;
using ParquetViewer.Engine.Exceptions;

namespace ParquetViewer.Engine.ParquetNET
{
    public partial class ParquetEngine : IParquetEngine, IDisposable
    {
        private readonly ParquetReader[] _parquetFiles;
        private long? _recordCount;

        private ParquetReader _defaultReader => _parquetFiles.FirstOrDefault() ?? throw new ParquetEngineException("No parquet readers available");

        private FileMetaData _thriftMetadata => _defaultReader.Metadata ?? throw new ParquetEngineException("No thrift metadata was found");

        private ParquetSchema _schema => _defaultReader.Schema;

        public Dictionary<string, string> CustomMetadata => _defaultReader.CustomMetadata;

        public long RecordCount => _recordCount ??= _parquetFiles.Sum(pf => pf.Metadata?.NumRows ?? 0);

        public int NumberOfPartitions => _parquetFiles.Length;

        public List<string> Fields => _defaultReader.Schema.Fields.Select(f => f.Name).ToList();

        public string Path { get; }

        ParquetMetadata? _metadata = null;
        public IParquetMetadata Metadata => _metadata ??= new ParquetMetadata(_thriftMetadata, BuildParquetSchemaTree(), (int)RecordCount);

        private ParquetEngine(string fileOrFolderPath, params ParquetReader[] parquetFiles)
        {
            _parquetFiles = parquetFiles ?? throw new ArgumentNullException(nameof(parquetFiles), "No parquet readers provided");
            Path = fileOrFolderPath;
        }

        private ParquetSchemaElement BuildParquetSchemaTree()
        {
            var thriftSchema = _thriftMetadata.Schema ?? throw new ParquetException("No thrift metadata was found");
            var schemaElements = thriftSchema.GetEnumerator();
            var thriftSchemaTree = ReadSchemaTree(ref schemaElements);

            foreach (var dataField in _schema.GetDataFields())
            {
                var field = thriftSchemaTree.GetChild(dataField.Path.FirstPart ?? throw new MalformedFieldException($"Field has no schema path: `{dataField.Name}`"));
                for (var i = 1; i < dataField.Path.Length; i++)
                {
                    field = field.GetChild(dataField.Path[i]);
                }
                field.DataField = dataField; //if it doesn't have a child it's a datafield (I hope)
            }

            return thriftSchemaTree;
        }

        private static ParquetSchemaElement ReadSchemaTree(ref List<SchemaElement>.Enumerator schemaElements)
        {
            if (!schemaElements.MoveNext())
                throw new ParquetException("Invalid parquet schema");

            var current = schemaElements.Current;
            var parquetSchemaElement = new ParquetSchemaElement(current);
            for (int i = 0; i < current.NumChildren; i++)
            {
                parquetSchemaElement.AddChild(ReadSchemaTree(ref schemaElements));
            }
            return parquetSchemaElement;
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
                var parquetReader = await ParquetReader.CreateAsync(parquetFilePath, new() { UseDateOnlyTypeForDates = true }, cancellationToken);
                return new ParquetEngine(parquetFilePath, parquetReader);
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
            foreach (var file in Engine.Helpers.ListParquetFiles(folderPath))
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    var parquetReader = await ParquetReader.CreateAsync(file, new() { UseDateOnlyTypeForDates = true }, cancellationToken);
                    if (!fileGroups.ContainsKey(parquetReader.Schema))
                    {
                        fileGroups.Add(parquetReader.Schema, new List<ParquetReader>());
                    }

                    fileGroups[parquetReader.Schema].Add(parquetReader);
                }
                catch (Exception ex)
                {
                    skippedFiles.Add(System.IO.Path.GetRelativePath(folderPath, file), ex);
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
                    Engine.Helpers.EZDispose(fileGroupList);
                }

                throw new MultipleSchemasFoundException(fileGroups.Keys.ToList()
                    .Select(schema => schema.Fields.Select(f => f.Name).ToList()).ToList());
            }
            else if (skippedFiles.Count > 0)
            {
                //We found one schema but some files couldn't be read
                Engine.Helpers.EZDispose(fileGroups.Values.First());
                throw new SomeFilesSkippedException(skippedFiles);
            }

            cancellationToken.ThrowIfCancellationRequested();

            return new ParquetEngine(folderPath, fileGroups.Values.First().ToArray());
        }

        private IEnumerable<(long RemainingOffset, ParquetReader ParquetReader)> GetReaders(long offset)
        {
            foreach (var parquetFile in _parquetFiles)
            {
                if (offset >= parquetFile.Metadata?.NumRows)
                {
                    offset -= parquetFile.Metadata.NumRows;
                    continue;
                }

                yield return (offset, parquetFile);
                offset = 0;
            }
        }

        public void Dispose() => Engine.Helpers.EZDispose(_parquetFiles);
    }
}
