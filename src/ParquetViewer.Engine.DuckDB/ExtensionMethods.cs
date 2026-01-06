using DuckDB.NET.Data;

namespace ParquetViewer.Engine.DuckDB
{
    internal static class ExtensionMethods
    {
        public static async Task<QueryResult> QueryAsync(this DuckDBConnection db, string sql)
        {
            ArgumentNullException.ThrowIfNull(db);
            ArgumentNullException.ThrowIfNull(sql);

            if (db.State == System.Data.ConnectionState.Closed)
            {
                await db.OpenAsync();
            }

            using var command = db.CreateCommand();
            command.CommandText = sql;

            var reader = command.ExecuteReader();
            return new QueryResult(reader);
        }
    }
}