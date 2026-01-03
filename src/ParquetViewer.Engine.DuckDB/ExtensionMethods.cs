using DuckDB.NET.Data;

namespace ParquetViewer.Engine.DuckDB
{
    internal static class ExtensionMethods
    {
        public static async IAsyncEnumerable<DuckDBDataReader> QueryAsync(this DuckDBConnection db, string sql)
        {
            ArgumentNullException.ThrowIfNull(db);
            ArgumentNullException.ThrowIfNull(sql);

            if (db.State == System.Data.ConnectionState.Closed)
            {
                await db.OpenAsync();
            }

            using var command = db.CreateCommand();
            command.CommandText = sql;

            using var reader = command.ExecuteReader();
            if (!reader.Read())
            {
                yield break;
            }

            yield return reader;

            while (reader.Read())
            {
                yield return reader;
            }
        }

        public static async Task<DuckDBDataReader> QuerySingleAsync(this DuckDBConnection db, string sql)
        {
            await foreach (var row in db.QueryAsync(sql))
            {
                return row;
            }

            throw new InvalidOperationException("The query returned no results.");
        }
    }
}
