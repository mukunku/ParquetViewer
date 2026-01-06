using DuckDB.NET.Data;

namespace ParquetViewer.Engine.DuckDB
{
    internal class QueryResult : IAsyncEnumerable<DuckDBDataReader>, IDisposable
    {
        private readonly DuckDBDataReader _reader;

        public QueryResult(DuckDBDataReader reader)
        {
            _reader = reader;
        }

        public void Dispose()
        {
            try
            {
                _reader.DisposeAsync();
            }
            catch { }
        }

        public async Task<DuckDBDataReader> GetSingleAsync()
        {
            if (await _reader.ReadAsync())
            {
                return _reader;
            }
            throw new InvalidOperationException("No rows found.");
        }

        public async IAsyncEnumerator<DuckDBDataReader> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            if (!await _reader.ReadAsync())
            {
                yield break;
            }

            yield return _reader;

            while (await _reader.ReadAsync())
            {
                cancellationToken.ThrowIfCancellationRequested();

                yield return _reader;
            }
        }
    }
}