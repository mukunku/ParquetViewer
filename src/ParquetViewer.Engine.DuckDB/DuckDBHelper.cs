using DuckDB.NET.Data;
using DuckDB.NET.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static ParquetViewer.Engine.IParquetSchemaElement;

namespace ParquetViewer.Engine.DuckDB
{
    internal static class DuckDBHelper
    {
        internal record DuckDBField(string Name, DuckDBType DuckDBType, Type Type);

        public static async Task<List<DuckDBField>> GetFields(DuckDBConnection db, string parquetFilePath)
        {
            var fields = new List<DuckDBField>();
            await foreach (var row in db.QueryAsync($"DESCRIBE TABLE '{parquetFilePath}';"))
            {
                var columnName = row.GetString(0);
                var columnTypeName = row.GetString(1);
                var (duckDBType, clrType) = ParseDuckDBType(columnTypeName);

                fields.Add(new(columnName, duckDBType, clrType));
            }
            return fields;
        }

        internal record DuckDBParquetMetadata(string CreatedBy, long NumRows, long NumRowGroups, 
            long ParquetVersion, string? EncryptionAlgorithm, string? FooterSigningKeyMetadata);
        public static async Task<DuckDBParquetMetadata> GetFileMetadata(DuckDBConnection db, string parquetFilePath)
        {
            await foreach (var row in db.QueryAsync($"SELECT * FROM parquet_file_metadata('{parquetFilePath}');"))
            {
                var createdBy = row.GetString(1);
                var numRows = row.GetInt64(2);
                var numRowGroups = row.GetInt64(3);
                var parquetVersion = row.GetInt64(4);
                var encryptionAlgorithm = row.IsDBNull(5) ? null : row.GetString(5);
                var footerSigningKeyMetadata = row.IsDBNull(6) ? null : row.GetString(6);

                return new DuckDBParquetMetadata(createdBy, numRows, numRowGroups, parquetVersion, encryptionAlgorithm, footerSigningKeyMetadata);
            }

            throw new InvalidOperationException("Failed to retrieve Parquet file metadata.");
        }

        public static string MakeColumnSafe(string columnName)
        {
            // Enclose in double quotes and escape existing double quotes
            var safeName = columnName.Replace("\"", "\"\"");
            return $"\"{safeName}\"";
        }

        private static (DuckDBType DuckDBType, Type Type) ParseDuckDBType(string duckDBTypeName)
        {
            // This mapping is based on https://duckdb.net/docs/type-mapping.html
            // It handles simple types and parameterized types by checking the start of the string.
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

        internal record ParquetFieldSchema(
            string FileName,
            string Name,
            string Type,
            string? TypeLength,
            RepetitionTypeId RepetitionType,
            long? NumChildren,
            string? ConvertedType,
            long? Scale,
            long? Precision,
            string FieldId,
            string LogicalType);

        internal static async Task<List<DuckDBField>> GetParquetSchema(DuckDBConnection db, string parquetFilePath)
        {
            //TODO: generate schema tree here
            var fields = new List<DuckDBField>();
            await foreach (var row in db.QueryAsync($"SELECT * FROM parquet_schema('{parquetFilePath}');"))
            {
                string fileName = row.GetString(0);
                string columnName = row.GetString(1);
                string columnTypeName = row.GetString(2);
                string repetitionTypeName = row.GetString(3);
                long? numChildren = row.IsDBNull(4) ? null : row.GetInt64(4);
                string? convertedType = row.IsDBNull(5) ? null : row.GetString(5);
                long? scale = row.IsDBNull(6) ? null : row.GetInt64(6);
                long? precision = row.IsDBNull(7) ? null : row.GetInt64(7);
                string fieldId = row.GetString(8);
                string logicalType = row.GetString(9);

                var (duckDBType, clrType) = ParseDuckDBType(columnTypeName);

                fields.Add(new(columnName, duckDBType, clrType));
            }
            return fields;
        }
    }
}
