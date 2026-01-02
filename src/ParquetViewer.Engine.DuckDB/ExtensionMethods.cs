using DuckDB.NET.Data;
using DuckDB.NET.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

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
    }
}
