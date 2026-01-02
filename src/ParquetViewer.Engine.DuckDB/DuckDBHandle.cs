using DuckDB.NET.Data;

namespace ParquetViewer.Engine.DuckDB
{
    public class DuckDBHandle : IDisposable
    {
        public string ParquetFilePath { get; }
        public DuckDBConnection Connection { get; }
        public int RecordCount { get; }

        private DuckDBHandle(DuckDBConnection connection, string parquetPath, int recordCount)
        {
            ParquetFilePath = parquetPath;
            Connection = connection;
            RecordCount = recordCount;
        }

        public static async Task<DuckDBHandle> OpenAsync(string parquetPath)
        {
            if (!File.Exists(parquetPath)) //handles null
                throw new FileNotFoundException(parquetPath);

            var connection = new DuckDBConnection("Data Source=:memory:");
            try
            {
                await connection.OpenAsync();
                var fileMetadata = await DuckDBHelper.GetFileMetadata(connection, parquetPath);
                return new DuckDBHandle(connection, parquetPath, (int)fileMetadata.NumRows);
            }
            catch
            {
                connection.Dispose();
                throw;
            }
        }

        public void Dispose()
        {
            try
            {
                Connection.Dispose();
            }
            catch { }
        }
    }
}
