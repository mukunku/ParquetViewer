using DuckDB.NET.Data;

namespace ParquetViewer.Engine.DuckDB
{
    public class DuckDBHandle : IDisposable
    {
        public string ParquetFilePath { get; }
        public DuckDBConnection Connection { get; }

        private DuckDBHandle(DuckDBConnection connection, string parquetPath)
        {
            ParquetFilePath = parquetPath;
            Connection = connection;
        }

        public static async Task<DuckDBHandle> OpenAsync(string parquetPath)
        {
            if (!File.Exists(parquetPath)) //handles null
                throw new FileNotFoundException(parquetPath);

            var connection = new DuckDBConnection("Data Source=:memory:");
            try
            {
                await connection.OpenAsync();
                return new DuckDBHandle(connection, parquetPath);
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