using Parquet;
using Parquet.Meta;
using Parquet.Schema;
using ParquetViewer.Engine.Exceptions;
using ParquetViewer.Engine.Types;
using System.Data;

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

        public async Task WriteDataToParquetFileAsync(DataTable dataTable, string path,
            CancellationToken cancellationToken, IProgress<int> progress, Dictionary<string, string>? customMetadata)
        {
            var fields = new List<Field>(dataTable.Columns.Count);
            foreach (DataColumn column in dataTable.Columns)
            {
                fields.Add(this._schema.Fields
                    .Where(field => field.Name.Equals(column.ColumnName, StringComparison.InvariantCulture))
                    .First());
            }
            var parquetSchema = new ParquetSchema(fields);

            using var fs = new FileStream(path, FileMode.OpenOrCreate);
            using var parquetWriter = await ParquetWriter.CreateAsync(parquetSchema, fs, cancellationToken: cancellationToken);
            parquetWriter.CompressionLevel = System.IO.Compression.CompressionLevel.Optimal;
            if (customMetadata is not null)
                parquetWriter.CustomMetadata = customMetadata;

            const int MAX_ROWS_PER_ROWGROUP = 100_000; //Without batching we sometimes get "OverflowException: Array dimensions exceeded supported range" from Parquet.NET
            var batchIndex = 0;
            var isLastBatch = false;
            while (!isLastBatch)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                using var rowGroup = parquetWriter.CreateRowGroup();
                foreach (var dataField in parquetSchema.DataFields)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }

                    var type = dataField.IsNullable ? GetNullableVersion(dataField.ClrType) : dataField.ClrType;
                    var values = GetColumnValues(dataTable, type, dataField.Name, batchIndex * MAX_ROWS_PER_ROWGROUP, MAX_ROWS_PER_ROWGROUP);
                    var dataColumn = new Parquet.Data.DataColumn(dataField, values);
                    await rowGroup.WriteColumnAsync(dataColumn, cancellationToken);
                    progress.Report(values.Length); //No way to report progress for each row, so do it by column
                    isLastBatch = values.Length < MAX_ROWS_PER_ROWGROUP;
                }
                batchIndex++;
            }
        }

        public void Dispose() => Engine.Helpers.EZDispose(_parquetFiles);

        private static System.Type GetNullableVersion(System.Type sourceType) => sourceType == null
                ? throw new ArgumentNullException(nameof(sourceType))
                : !sourceType.IsValueType
                    || (sourceType.IsGenericType
                        && sourceType.GetGenericTypeDefinition() == typeof(Nullable<>))
                ? sourceType
                : typeof(Nullable<>).MakeGenericType(sourceType);

        private static Array GetColumnValues(DataTable dataTable, System.Type type, string columnName, int skipCount, int fetchCount)
        {
            ArgumentNullException.ThrowIfNull(dataTable);
            ArgumentNullException.ThrowIfNull(type);
            ArgumentOutOfRangeException.ThrowIfLessThan(skipCount, 0);
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(fetchCount, 0);

            if (!dataTable.Columns.Contains(columnName))
                throw new ArgumentException($"Column `{columnName}` does not exist in the datatable");

            var recordCountAfterSkip = dataTable.Rows.Count - skipCount;
            var recordCountToRead = fetchCount > recordCountAfterSkip ? recordCountAfterSkip : fetchCount;
            var values = Array.CreateInstance(type, recordCountToRead);
            var index = 0;
            foreach (DataRow row in dataTable.Rows)
            {
                if (skipCount-- > 0)
                {
                    continue;
                }

                var value = row[columnName];
                if (value == DBNull.Value)
                    value = null;
                else if (value is IByteArrayValue byteArray)
                    value = byteArray.Data;
                else if (value is IListValue || value is IMapValue || value is IStructValue)
                    throw new NotSupportedException("List, Map, and Struct types are currently not supported.");

                values.SetValue(value, index++);

                if (--fetchCount <= 0)
                {
                    break;
                }
            }

            return values;
        }
    }
}