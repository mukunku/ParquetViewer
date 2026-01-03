using DuckDB.NET.Data;
using DuckDB.NET.Native;
using ParquetViewer.Engine.DuckDB.Types;
using ParquetViewer.Engine.Exceptions;
using System.Security.Cryptography;
using static ParquetViewer.Engine.IParquetSchemaElement;

namespace ParquetViewer.Engine.DuckDB
{
    public class ParquetMetadata : IParquetMetadata
    {
        public int ParquetVersion { get; }

        public int RowGroupCount { get; }

        public int RowCount { get; }

        public string CreatedBy { get; }

        public ICollection<IRowGroupMetadata> RowGroups { get; }

        public IParquetSchemaElement SchemaTree { get; }

        private ParquetMetadata(IParquetSchemaElement schemaTree, ICollection<IRowGroupMetadata> rowGroups, 
            int recordCount, int parquetVersion, string createdBy, int rowGroupCount)
        {
            this.SchemaTree = schemaTree;
            this.RowGroups = rowGroups;
            this.RowCount = recordCount;
            this.ParquetVersion = parquetVersion;
            this.CreatedBy = createdBy;
            this.RowGroupCount = rowGroupCount;
        }

        public static async Task<ParquetMetadata> FromDuckDBAsync(DuckDBHandle db)
        {
            var schemaTree = await DuckDBHelper.GetParquetSchemaTreeAsync(db);

            #region RowGroups
            var rowGroups = new List<IRowGroupMetadata>();
            using var result = await db.Connection.QueryAsync($"SELECT * FROM parquet_metadata('{db.ParquetFilePath}');");
            await foreach (var row in result)
            {
                string fileName = row.GetString(0);

                long rowGroupId = row.GetInt64(1);
                long rowGroupNumRows = row.GetInt64(2);
                long rowGroupNumColumns = row.GetInt64(3);
                long rowGroupBytes = row.GetInt64(4);

                long columnId = row.GetInt64(5);
                long? fileOffset = row.IsDBNull(6) ? null : row.GetInt64(6);
                long numValues = row.GetInt64(7);

                string pathInSchema = row.GetString(8);
                string type = row.GetString(9);

                string? statsMin = row.IsDBNull(10) ? null : row.GetString(10);
                string? statsMax = row.IsDBNull(11) ? null : row.GetString(11);

                long statsNullCount = row.GetInt64(12);
                long? statsDistinctCount = row.IsDBNull(13) ? null : row.GetInt64(13);

                string? statsMinValue = row.IsDBNull(14) ? null : row.GetString(14);
                string? statsMaxValue = row.IsDBNull(15) ? null : row.GetString(15);

                string compression = row.GetString(16);
                string encodings = row.GetString(17);

                long? indexPageOffset = row.IsDBNull(18) ? null : row.GetInt64(18);
                long? dictionaryPageOffset = row.IsDBNull(19) ? null : row.GetInt64(19);
                long dataPageOffset = row.GetInt64(20);

                long totalCompressedSize = row.GetInt64(21);
                long totalUncompressedSize = row.GetInt64(22);

                long? bloomFilterOffset = row.IsDBNull(24) ? null : row.GetInt64(24);
                long? bloomFilterLength = row.IsDBNull(25) ? null : row.GetInt64(25);

                long? rowGroupCompressedBytes = row.IsDBNull(28) ? null : row.GetInt64(28);

                if (rowGroups.Exists(rg => rg.Ordinal == (int)rowGroupId))
                {
                    //DuckDB returns rows/offsets for each column in the rowgroup. So we only keep the first one.
                    continue;
                }

                rowGroups.Add(new RowGroupMetadata(
                    (int)rowGroupId,
                    (int)rowGroupNumRows,
                    fileOffset ?? 0,
                    rowGroupBytes,
                    totalCompressedSize));
            }
            #endregion

            #region File Metadata
            using var metadataResult = await db.Connection.QueryAsync($"SELECT * FROM parquet_file_metadata('{db.ParquetFilePath}');");
            var fileMetadata = await metadataResult.GetSingleAsync();
            var createdBy = fileMetadata.IsDBNull(1) ? null : fileMetadata.GetString(1);
            var numRows = fileMetadata.GetInt64(2);
            var numRowGroups = fileMetadata.GetInt64(3);
            var parquetVersion = fileMetadata.GetInt64(4);
            var encryptionAlgorithm = fileMetadata.IsDBNull(5) ? null : fileMetadata.GetString(5);
            var footerSigningKeyMetadata = fileMetadata.IsDBNull(6) ? null : fileMetadata.GetString(6);
            #endregion

            var metadata = new ParquetMetadata(schemaTree, rowGroups, (int)numRows, (int)parquetVersion, createdBy ?? string.Empty, (int)numRowGroups);
            return metadata;
        }
    }    
}
