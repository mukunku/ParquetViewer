using DuckDB.NET.Data;
using DuckDB.NET.Native;
using System.Numerics;
using System.Text;
using static ParquetViewer.Engine.IParquetSchemaElement;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ParquetViewer.Engine.DuckDB
{
    internal static class DuckDBHelper
    {
        internal record DuckDBField(string Name, DuckDBType DuckDBType, Type Type);

        public static async Task<List<DuckDBField>> GetFields(DuckDBHandle db)
        {
            var fields = new List<DuckDBField>();
            using var result = await db.Connection.QueryAsync($"DESCRIBE TABLE '{db.ParquetFilePath}';");
            await foreach (var row in result)
            {
                var columnName = row.GetString(0);
                var columnTypeName = row.GetString(1);
                var (duckDBType, clrType) = ParseDuckDBType(columnTypeName);

                fields.Add(new(columnName, duckDBType, clrType));
            }
            return fields;
        }

        public static (DuckDBType DuckDBType, Type Type) ParseDuckDBType(string duckDBTypeName)
        {
            // This mapping is based on https://duckdb.net/docs/type-mapping.html
            // It handles simple types and parameterized types by checking the start of the string.
            if (duckDBTypeName.EndsWith("[]")) return (DuckDBType.List, typeof(List<>));

            if (duckDBTypeName.StartsWith("DECIMAL")) return (DuckDBType.Decimal, typeof(decimal));
            if (duckDBTypeName.StartsWith("VARCHAR")) return (DuckDBType.Varchar, typeof(string));
            if (duckDBTypeName.StartsWith("LIST")) return (DuckDBType.List, typeof(List<>));
            if (duckDBTypeName.StartsWith("MAP")) return (DuckDBType.Map, typeof(Dictionary<,>));
            if (duckDBTypeName.StartsWith("STRUCT")) return (DuckDBType.Struct, typeof(ValueTuple));
            if (duckDBTypeName.StartsWith("ENUM")) return (DuckDBType.Enum, typeof(string));
            if (duckDBTypeName.StartsWith("TIMESTAMP")) return (DuckDBType.Timestamp, typeof(DateTime));

            return duckDBTypeName switch
            {
                "BOOLEAN" => (DuckDBType.Boolean, typeof(bool)),
                "TINYINT" => (DuckDBType.TinyInt, typeof(sbyte)),
                "SMALLINT" => (DuckDBType.SmallInt, typeof(short)),
                "INTEGER" => (DuckDBType.Integer, typeof(int)),
                "BIGINT" => (DuckDBType.BigInt, typeof(long)),
                "HUGEINT" => (DuckDBType.BigInt, typeof(BigInteger)),
                "UTINYINT" => (DuckDBType.UnsignedTinyInt, typeof(byte)),
                "USMALLINT" => (DuckDBType.UnsignedSmallInt, typeof(ushort)),
                "UINTEGER" => (DuckDBType.UnsignedInteger, typeof(uint)),
                "UBIGINT" => (DuckDBType.UnsignedBigInt, typeof(ulong)),
                "UHUGEINT" => (DuckDBType.HugeInt, typeof(BigInteger)),
                "DOUBLE" => (DuckDBType.Double, typeof(double)),
                "FLOAT" or "REAL" => (DuckDBType.Float, typeof(float)),
                "BLOB" => (DuckDBType.Blob, typeof(byte[])),
                "DATE" => (DuckDBType.Date, typeof(DateOnly)),
                "TIME" => (DuckDBType.Time, typeof(TimeSpan)),
                "INTERVAL" => (DuckDBType.Interval, typeof(TimeSpan)),
                "UUID" => (DuckDBType.Uuid, typeof(Guid)),
                _ => throw new ArgumentOutOfRangeException(nameof(duckDBTypeName), $"Unsupported DuckDB type: {duckDBTypeName}")
            };
        }

        //DuckDB flattens the schema so we need to rebuild it into a tree structure.
        public static async Task<ParquetSchemaElement> GetParquetSchemaTreeAsync(DuckDBHandle db)
        {
            var result = await db.Connection.QueryAsync($"SELECT * FROM parquet_schema('{db.ParquetFilePath}');");
            var enumerator = result.GetAsyncEnumerator();

            if (!await enumerator.MoveNextAsync())
            {
                throw new InvalidDataException("Failed to retrieve Parquet schema.");
            }

            var rootNode = ParquetSchemaElement.FromRow(enumerator.Current);
            await ReadChildrenAsync(rootNode, enumerator);
            return rootNode;

            async Task ReadChildrenAsync(ParquetSchemaElement parent, IAsyncEnumerator<DuckDBDataReader> enumerator)
            {
                for (int i = 0; i < parent.NumChildren; i++)
                {
                    if (!await enumerator.MoveNextAsync())
                    {
                        throw new InvalidDataException($"Premature end to parquet schema for field `{parent.Path}`.");
                    }

                    var childNode = ParquetSchemaElement.FromRow(enumerator.Current);
                    parent.Children.Add(childNode);
                    await ReadChildrenAsync(childNode, enumerator);
                }
            }
        }

        public static async Task<Dictionary<string, string>> GetCustomMetadataAsync(DuckDBHandle db)
        {
            var query = $"SELECT * FROM parquet_kv_metadata('{db.ParquetFilePath}');";
            var metadata = new Dictionary<string, string>();
            using var result = await db.Connection.QueryAsync(query);
            await foreach (var row in result)
            {
                var keyStream = await row.GetFieldValueAsync<Stream>(1);
                var valueStream = await row.GetFieldValueAsync<Stream>(2);

                using var keyReader = new StreamReader(keyStream, Encoding.UTF8);
                string key = await keyReader.ReadToEndAsync();

                using var valueReader = new StreamReader(valueStream, Encoding.UTF8);
                string value = await valueReader.ReadToEndAsync();
                metadata[key] = value;
            }
            return metadata;
        }
    }
}
