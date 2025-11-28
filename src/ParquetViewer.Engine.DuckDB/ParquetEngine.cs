using DuckDB.NET.Data;
using ParquetViewer.Engine.Exceptions;
using System.Data;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using static ParquetViewer.Engine.DuckDB.DuckDBHelper;

namespace ParquetViewer.Engine.DuckDB
{
    public class ParquetEngine : IParquetEngine, IDisposable
    {
        private readonly DuckDBConnection _inMemoryDB;

        private long? _recordCount;

        public string Path { get; set; }

        public List<string> Fields => this._fields.Select(f => f.Name).ToList();

        public long RecordCount { get; }

        public int NumberOfPartitions => throw new NotImplementedException();

        public Dictionary<string, string> CustomMetadata => throw new NotImplementedException();

        public IParquetMetadata Metadata { get; }

        private List<DuckDBField> _fields;

        private ParquetEngine(string path, DuckDBConnection db, ParquetMetadata metadata, List<DuckDBField> fields, long recordCount)
        {
            this._inMemoryDB = db;
            this.Path = path;
            this.Metadata = metadata;
            this._fields = fields;
            this.RecordCount = recordCount;
        }

        public static async Task<ParquetEngine> OpenFileAsync(string parquetFilePath, CancellationToken cancellationToken)
        {
            if (!File.Exists(parquetFilePath)) //Handles null
            {
                throw new FileNotFoundException($"Could not find parquet file at: {parquetFilePath}");
            }

            var db = new DuckDBConnection("Data Source=:memory:");
            try
            {
                await db.OpenAsync();
                var parquetMetadata = await ParquetMetadata.FromDuckDBAsync(db, parquetFilePath);
                var fields = await DuckDBHelper.GetFields(db, parquetFilePath);
                var fileMetadata = await DuckDBHelper.GetFileMetadata(db, parquetFilePath);
                var parquetSchema = await DuckDBHelper.GetParquetSchema(db, parquetFilePath);
                return new ParquetEngine(parquetFilePath, db, parquetMetadata, fields.ToList(), fileMetadata.NumRows);
            }
            catch (Exception)
            {
                db.Dispose();
                throw;
            }
        }

        public void Dispose()
        {
            this._inMemoryDB.Dispose();
        }

        public async Task<Func<bool, DataTable>> ReadRowsAsync(List<string> selectedFields, int offset, int recordCount, CancellationToken cancellationToken, IProgress<int>? progress = null)
        {
            EnsurePathExists();

            var query = $"SELECT {string.Join(", ", selectedFields.Select(DuckDBHelper.MakeColumnSafe))} " +
                $"FROM '{this.Path}' " +
                $"LIMIT {recordCount} " +
                $"OFFSET {offset};";

            var result = CreateEmptyDataTable(selectedFields);
            result.BeginLoadData();
            await foreach (var row in this._inMemoryDB.QueryAsync(query))
            {
                cancellationToken.ThrowIfCancellationRequested();
                
                var values = new object[row.FieldCount];
                row.GetValues(values);

                //supposedly this is the fastest way to load data into a datatable https://stackoverflow.com/a/17123914/1458738
                result.LoadDataRow(values, false);
                
                progress?.Report(row.FieldCount);
            }
            result.EndLoadData();

            return (bool logProgress) =>
            {
                if (logProgress)
                {
                    //We don't have any post-processing. So just report the total.
                    progress?.Report(result.Rows.Count * result.Columns.Count);
                }
                return result;
            };
        }

        private DataTable CreateEmptyDataTable(List<string> selectedFields)
        {
            var dataTable = new DataTable();
            foreach (var field in this._fields)
            {
                if (!selectedFields.Contains(field.Name))
                    continue;

                dataTable.Columns.Add(new DataColumn(field.Name, field.Type));
            }
            return dataTable;
        }

        private void EnsurePathExists()
        {
            if (!File.Exists(this.Path))
            {
                throw new FileNotFoundException($"Parquet file no longer exists at: {this.Path}");
            }
        }

    }
}
