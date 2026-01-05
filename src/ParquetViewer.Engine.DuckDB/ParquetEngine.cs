using DuckDB.NET.Data;
using ParquetViewer.Engine.DuckDB.Types;
using ParquetViewer.Engine.Exceptions;
using System.Collections;
using System.Data;
using static ParquetViewer.Engine.DuckDB.DuckDBHelper;

namespace ParquetViewer.Engine.DuckDB
{
    public class ParquetEngine : IParquetEngine
    {
        private readonly List<DuckDBHandle> _dbs;
        private readonly List<ParquetMetadata> _metadatas;

        public string Path { get; set; }

        public List<string> Fields => this._fields.Select(f => f.Name).ToList();

        public long RecordCount { get; }

        public int NumberOfPartitions => this._dbs.Count;

        public Dictionary<string, string> CustomMetadata { get; }

        public IParquetMetadata Metadata => this._metadatas.First();

        private List<DuckDBField> _fields;

        private static int GetFieldsHashCode(List<DuckDBField> fields)
        {
            var hashCode = new HashCode();
            foreach (var field in fields)
            {
                hashCode.Add(field.Name);
                hashCode.Add(field.Type);
                hashCode.Add(field.DuckDBType);
            }
            return hashCode.ToHashCode();
        }

        private ParquetEngine(string filePath, DuckDBHandle db, ParquetMetadata metadata, List<DuckDBField> fields, long recordCount, Dictionary<string, string> customMetadata)
        {
            this._dbs = [db];
            this.Path = filePath;
            this._metadatas = [metadata];
            this._fields = FilterOutFieldsThatDontExist(fields, metadata);
            this.RecordCount = recordCount;
            this.CustomMetadata = customMetadata;
        }

        private ParquetEngine(string folderPath, List<DuckDBHandle> dbs, List<ParquetMetadata> metadatas, List<DuckDBField> fields, long recordCount, Dictionary<string, string> customMetadata)
        {
            this._dbs = dbs;
            this.Path = folderPath;
            this._metadatas = metadatas;
            this._fields = FilterOutFieldsThatDontExist(fields, this._metadatas.First());
            this.RecordCount = recordCount;
            this.CustomMetadata = customMetadata;
        }

        /// <summary>
        /// DuckDB sometimes returns fields from the DESCRIBE TABLE query that don't actually exist in the Parquet file.
        /// </summary>
        /// <returns>Returns a new list with fields that actually exist in the parquet file.</returns>
        /// <remarks>Fixes PARTITIONED_PARQUET_FILE_TEST</remarks>
        private static List<DuckDBField> FilterOutFieldsThatDontExist(List<DuckDBField> fields, ParquetMetadata metadata)
        {
            var fieldsThatExist = new List<DuckDBField>();
            foreach (var field in fields)
            {
                if (metadata.SchemaTree.Children.Cast<IParquetSchemaElement>().Any(f => f.Path == field.Name))
                {
                    fieldsThatExist.Add(field);
                }
            }
            return fieldsThatExist;
        }

        public static Task<ParquetEngine> OpenFileOrFolderAsync(string parquetFilePath, CancellationToken cancellationToken)
        {
            if (File.Exists(parquetFilePath)) //Handles null
            {
                return OpenFileAsync(parquetFilePath, cancellationToken);
            }
            else if (Directory.Exists(parquetFilePath)) //Handles null
            {
                return OpenFolderAsync(parquetFilePath, cancellationToken);
            }
            else
            {
                throw new FileNotFoundException(parquetFilePath);
            }
        }

        public static async Task<ParquetEngine> OpenFileAsync(string parquetFilePath, CancellationToken cancellationToken)
        {
            if (!File.Exists(parquetFilePath)) //Handles null
            {
                throw new FileNotFoundException($"Could not find parquet file at: {parquetFilePath}");
            }

            var db = await DuckDBHandle.OpenAsync(parquetFilePath);
            try
            {
                var parquetMetadata = await ParquetMetadata.FromDuckDBAsync(db);
                var fields = await DuckDBHelper.GetFields(db);
                var customMetadata = await DuckDBHelper.GetCustomMetadataAsync(db);
                return new ParquetEngine(parquetFilePath, db, parquetMetadata, fields.ToList(), parquetMetadata.RowCount, customMetadata);
            }
            catch (Exception)
            {
                db.Dispose();
                throw;
            }
        }

        public static async Task<ParquetEngine> OpenFolderAsync(string folderPath, CancellationToken cancellationToken)
        {
            if (!Directory.Exists(folderPath)) //Handles null
            {
                throw new DirectoryNotFoundException($"Directory doesn't exist: {folderPath}");
            }

            var skippedFiles = new Dictionary<string, Exception>();
            var fileGroups = new Dictionary<int, List<DuckDBHandle>>();
            foreach (var file in Helpers.ListParquetFiles(folderPath))
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    var db = await DuckDBHandle.OpenAsync(file);
                    var fileFields = await DuckDBHelper.GetFields(db);
                    var fieldsHashCode = GetFieldsHashCode(fileFields);
                    if (!fileGroups.ContainsKey(fieldsHashCode))
                    {
                        fileGroups.Add(fieldsHashCode, new List<DuckDBHandle>());
                    }

                    fileGroups[fieldsHashCode].Add(db);
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
                    Helpers.EZDispose(fileGroupList);
                }

                var fieldsByFile = new List<List<string>>();
                foreach (var db in fileGroups.Values)
                {
                    var groupFields = await DuckDBHelper.GetFields(db.First());
                    fieldsByFile.Add(groupFields.Select(f => f.Name).ToList());
                }

                throw new MultipleSchemasFoundException(fieldsByFile);
            }
            else if (skippedFiles.Count > 0)
            {
                //We found one schema but some files couldn't be read
                Helpers.EZDispose(fileGroups.Values.First());
                throw new SomeFilesSkippedException(skippedFiles);
            }

            cancellationToken.ThrowIfCancellationRequested();

            //We have only one schema across all files and are good to go
            List<DuckDBHandle> dbs = fileGroups.Values.First();

            var metadatas = new List<ParquetMetadata>();
            foreach (var db in dbs)
            {
                var metadata = await ParquetMetadata.FromDuckDBAsync(db);
                metadatas.Add(metadata);
            }

            var totalRecordCount = metadatas.Sum(m => m.RowCount);
            var fields = await DuckDBHelper.GetFields(dbs.First());
            var customMetadata = await DuckDBHelper.GetCustomMetadataAsync(dbs.First());

            return new ParquetEngine(folderPath, dbs, metadatas, fields, totalRecordCount, customMetadata);
        }

        public void Dispose()
        {
            Helpers.EZDispose(this._dbs);
        }

        private async IAsyncEnumerable<DuckDBDataReader> QueryDataAsync(List<string> selectedFields, int offset, int recordCount)
        {
            var fields = string.Join(", ", selectedFields.Select(MakeColumnSafe));
            foreach ((DuckDBHandle db, ParquetMetadata metadata) in Helpers.PairEnumerables(this._dbs, this._metadatas))
            {
                EnsureFileExists(db.ParquetFilePath);

                if (recordCount <= 0)
                    yield break;

                if (offset >= metadata.RowCount)
                {
                    offset -= metadata.RowCount;
                    continue;
                }

                var query = $"SELECT {fields} " +
                    $"FROM '{db.ParquetFilePath}' " +
                    $"LIMIT {recordCount} " +
                    $"OFFSET {offset};";

                offset = 0;

                using var result = await db.Connection.QueryAsync(query);
                await foreach (var row in result)
                {
                    yield return row;
                    recordCount--;
                }
            }
        }

        public async Task<Func<bool, DataTable>> ReadRowsAsync(List<string> selectedFields, int offset, int recordCount, CancellationToken cancellationToken, IProgress<int>? progress = null)
        {
            var result = CreateEmptyDataTable(selectedFields);
            result.BeginLoadData();
            await foreach (var row in this.QueryDataAsync(selectedFields, offset, recordCount))
            {
                cancellationToken.ThrowIfCancellationRequested();

                var values = new object[row.FieldCount];
                row.GetValues(values);

                //Convert values to our types
                for (var columnIndex = 0; columnIndex < row.FieldCount; columnIndex++)
                {
                    var fieldName = selectedFields.ElementAt(columnIndex);
                    var parquetSchemaElement = (ParquetSchemaElement)this._metadatas.First().SchemaTree.Children.First(f => f.Path == fieldName);
                    values[columnIndex] = ConvertValueTypeIfNeeded(values[columnIndex], parquetSchemaElement);
                }

                //supposedly this is the fastest way to load data into a datatable https://stackoverflow.com/a/17123914/1458738
                result.LoadDataRow(values, false);

                progress?.Report(row.FieldCount);
            }
            result.EndLoadData();

            return (bool shouldLogProgress) =>
            {
                if (shouldLogProgress)
                {
                    //We don't have any post-processing. So just report the total.
                    progress?.Report(result.Rows.Count * result.Columns.Count);
                }
                return result;
            };

            object ConvertValueTypeIfNeeded(object? value, ParquetSchemaElement? parquetSchemaElement)
            {
                if (value is null || value == DBNull.Value || parquetSchemaElement is null)
                    return DBNull.Value;

                if (parquetSchemaElement.FieldType == FieldTypeId.List)
                {
                    var list = (IList)value;
                    ParquetSchemaElement? listItemField = null;
                    if (parquetSchemaElement.Children.Count > 0)
                    {
                        var listField = parquetSchemaElement.GetListField();
                        if (listField.Children.Count == 0) //Assume 2-tier list variation (fixes: TWO_TIER_TEPEATED_LIST_FIELDS_TEST)
                        {
                            listItemField = listField;
                        }
                        else
                        {
                            listItemField = listField.GetListItemField();
                        }
                    }
                    else if (parquetSchemaElement.IsPrimitive) //2-tier list (fixes: TWO_TIER_TEPEATED_LIST_FIELDS_TEST)
                    {
                        var newestList = new ArrayList(list.Count);
                        foreach (var item in list)
                        {
                            newestList.Add(item);
                        }
                        return new ListValue(newestList, parquetSchemaElement.ClrType);
                    }

                    var newList = new ArrayList(list.Count);
                    foreach (var item in list)
                    {
                        newList.Add(ConvertValueTypeIfNeeded(item, listItemField));
                    }

                    return new ListValue(newList, listItemField!.ClrType!);
                }
                else if (parquetSchemaElement.FieldType == FieldTypeId.Struct)
                {
                    var @struct = (Dictionary<string, object?>)value;
                    var dataTable = new DataTableLite(1);
                    foreach (var fieldName in @struct.Keys)
                    {
                        var field = parquetSchemaElement.GetSingleOrByName(fieldName);
                        if (field.FieldType == FieldTypeId.List)
                        {
                            dataTable.AddColumn(fieldName, typeof(ListValue), field);
                        }
                        else if (field.FieldType == FieldTypeId.Struct)
                        {
                            dataTable.AddColumn(fieldName, typeof(StructValue), field);
                        }
                        else if (field.FieldType == FieldTypeId.Map)
                        {
                            dataTable.AddColumn(fieldName, typeof(MapValue), field);
                        }
                        else //Primitive
                        {
                            dataTable.AddColumn(fieldName, field.ClrType, field);
                        }
                    }
                    dataTable.NewRow();
                    var fieldIndex = 0;
                    foreach (var keyValuePair in @struct)
                    {
                        var field = parquetSchemaElement.GetSingleOrByName(keyValuePair.Key);
                        dataTable.Rows[0][fieldIndex] = ConvertValueTypeIfNeeded(keyValuePair.Value ?? DBNull.Value, field);
                        fieldIndex++;
                    }

                    return new StructValue(parquetSchemaElement.Path, dataTable.GetRowAt(0));
                }
                else if (parquetSchemaElement.FieldType == FieldTypeId.Map)
                {
                    var map = (IDictionary)value;
                    var mapField = parquetSchemaElement.GetMapKeyValueField();
                    var mapKeyField = mapField.GetMapKeyField();
                    var mapValueField = mapField.GetMapValueField();

                    var count = Math.Max(map.Keys.Count, map.Values.Count);
                    var keys = new ArrayList(count);
                    var values = new ArrayList(count);
                    foreach ((object? key, object? value) pair in
                        Helpers.PairEnumerables(map.Keys.Cast<object?>(), map.Values.Cast<object?>(), DBNull.Value))
                    {
                        keys.Add(ConvertValueTypeIfNeeded(pair.key, mapKeyField));
                        values.Add(ConvertValueTypeIfNeeded(pair.value, mapValueField));
                    }

                    return new MapValue(keys, mapKeyField.ClrType,
                        values, mapValueField.ClrType);
                }
                else if (parquetSchemaElement.FieldType == FieldTypeId.Primitive //2-tier list
                    && parquetSchemaElement.RepetitionType == RepetitionTypeId.Repeated)
                {
                    var list = (IList)value;

                    var newList = new ArrayList(list.Count);
                    foreach (var item in list)
                    {
                        newList.Add(ConvertValueTypeIfNeeded(item, null));
                    }

                    return new ListValue(newList, parquetSchemaElement.ClrType);
                }
                else if (parquetSchemaElement.IsByteArrayType)
                {
                    using var ms = new MemoryStream();
                    ((Stream)value).CopyTo(ms);
                    return new ByteArrayValue(parquetSchemaElement.Path, ms.ToArray());
                }
                else //primitive value
                {
                    return value;
                }
            }
        }

        private DataTable CreateEmptyDataTable(List<string> selectedFields)
        {
            var dataTable = new DataTable();
            foreach (var field in this._fields)
            {
                if (!selectedFields.Contains(field.Name))
                    continue;

                var schemaField = (ParquetSchemaElement)this.Metadata.SchemaTree.GetChild(field.Name);
                if (schemaField.FieldType == FieldTypeId.Struct)
                {
                    dataTable.Columns.Add(new DataColumn(field.Name, typeof(StructValue)));
                }
                else if (schemaField.FieldType == FieldTypeId.List)
                {
                    dataTable.Columns.Add(new DataColumn(field.Name, typeof(ListValue)));
                }
                else if (schemaField.FieldType == FieldTypeId.Map)
                {
                    dataTable.Columns.Add(new DataColumn(field.Name, typeof(MapValue)));
                }
                else if (schemaField.IsByteArrayType)
                {
                    dataTable.Columns.Add(new DataColumn(field.Name, typeof(ByteArrayValue)));
                }
                else //Primitive type
                {
                    dataTable.Columns.Add(new DataColumn(field.Name, field.Type));
                }
            }
            return dataTable;
        }

        private void EnsureFileExists(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Parquet file no longer exists at: {this.Path}");
            }
        }

        private static string MakeColumnSafe(string columnName)
        {
            // Enclose in double quotes and escape existing double quotes
            var safeName = columnName.Replace("\"", "\"\"");
            return $"\"{safeName}\"";
        }

    }
}
