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
            var rowGroupColumns = new List<(RowGroupMetadataResult RowGroup, RowGroupColumnMetadata Column)>();
            using var result = await db.Connection.QueryAsync($"SELECT * FROM parquet_metadata('{db.ParquetFilePath}');");
            await foreach (var row in result)
            {
                string fileName = row.GetString(0);

                long rowGroupId = row.GetInt64(1);
                long rowGroupNumRows = row.GetInt64(2);
                long rowGroupNumColumns = row.GetInt64(3);
                long rowGroupBytes = row.GetInt64(4);
                long? rowGroupCompressedBytes = row.IsDBNull(28) ? null : row.GetInt64(28);
                var rowGroupMetadataResult = new RowGroupMetadataResult(rowGroupId, rowGroupNumRows, rowGroupNumColumns, rowGroupBytes, rowGroupCompressedBytes ?? -1);

                long columnId = row.GetInt64(5);
                long? fileOffset = row.IsDBNull(6) ? null : row.GetInt64(6);
                long numValues = row.GetInt64(7);

                string pathInSchema = row.GetString(8);
                string type = row.GetString(9);

                string? statsMin = row.IsDBNull(10) ? null : row.GetString(10);
                string? statsMax = row.IsDBNull(11) ? null : row.GetString(11);

                long? statsNullCount = row.IsDBNull(12) ? null : row.GetInt64(12);
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

                bool? minIsExact = row.IsDBNull(26) ? null : row.GetBoolean(26);
                bool? maxIsExact = row.IsDBNull(27) ? null : row.GetBoolean(27);

                var rowGroupColumnMetadata = new RowGroupColumnMetadata(
                    (int)columnId,
                    pathInSchema,
                    type,
                    (int)numValues,
                    totalUncompressedSize,
                    totalCompressedSize,
                    dataPageOffset,
                    indexPageOffset,
                    dictionaryPageOffset,
                    new RowGroupColumnStatistics(
                        statsMin,
                        statsMax,
                        statsNullCount,
                        statsDistinctCount,
                        statsMinValue,
                        statsMaxValue,
                        minIsExact,
                        maxIsExact),
                    bloomFilterOffset,
                    bloomFilterLength);

                rowGroupColumns.Add((rowGroupMetadataResult, rowGroupColumnMetadata));
            }

            List<IRowGroupMetadata> rowGroups = rowGroupColumns.GroupBy(rgc => rgc.RowGroup.rowGroupId).Select(group =>
            {
                var rowGroupId = group.Key;
                RowGroupMetadataResult? rowGroupMetadataResult = null;
                List<RowGroupColumnMetadata> columnMetadatas = new();
                foreach (var column in group)
                {
                    rowGroupMetadataResult = column.RowGroup;
                    columnMetadatas.Add(column.Column);
                }

                if (rowGroupMetadataResult is null)
                    return null;

                return new RowGroupMetadata(
                    (int)rowGroupId,
                    (int)rowGroupMetadataResult.rowGroupNumRows,
                    (int)rowGroupMetadataResult.rowGroupNumColumns,
                    rowGroupColumns.First(rgc => rgc.RowGroup.rowGroupId == group.Key).Column.DataPageOffset ?? 0,
                    rowGroupMetadataResult.rowGroupBytes,
                    columnMetadatas.Sum(cm => cm.TotalCompressedSize ?? 0), 
                    columnMetadatas);
            }).Where(rg => rg is not null)!.ToList<IRowGroupMetadata>();
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

        private record RowGroupMetadataResult(long rowGroupId, long rowGroupNumRows, long rowGroupNumColumns, long rowGroupBytes, long rowGroupCompressedBytes);
    }

    public class RowGroupMetadata : IRowGroupMetadata
    {
        public int Ordinal { get; }
        public int RowCount { get; }
        public int ColumnCount { get; }
        public ICollection<ISortingColumnMetadata>? SortingColumns { get; }
        public ICollection<IRowGroupColumnMetadata>? Columns { get; }
        public long FileOffset { get; }
        public long TotalByteSize { get; }
        public long TotalCompressedSize { get; }

        public RowGroupMetadata(int ordinal, int rowCount, int columnCount, long fileOffset, long totalByteSize, long totalCompressedSize, List<RowGroupColumnMetadata> columnMetadatas)
        {
            this.Ordinal = ordinal;
            this.RowCount = rowCount;
            this.ColumnCount = columnCount;
            this.FileOffset = fileOffset;
            this.TotalByteSize = totalByteSize;
            this.TotalCompressedSize = totalCompressedSize;
            this.SortingColumns = null; //DuckDB doesn't seem to have info on this
            this.Columns = columnMetadatas.ToList<IRowGroupColumnMetadata>();
        }
    }

    public class RowGroupColumnMetadata : IRowGroupColumnMetadata
    {
        public int? ColumnId { get; }

        public string? PathInSchema { get; }

        public string? Type { get; }

        public int? NumValues { get; }

        public long? TotalUncompressedSize { get; }

        public long? TotalCompressedSize { get; }

        public long? DataPageOffset { get; }

        public long? IndexPageOffset { get; }

        public long? DictionaryPageOffset { get; }

        public IRowGroupColumnStatistics? Statistics { get; }

        public long? BloomFilterOffset { get; }

        public long? BloomFilterLength { get; }

        public RowGroupColumnMetadata(
            int? columnId, 
            string? pathInSchema, 
            string? type, 
            int? numValues, 
            long? totalUncompressedSize, 
            long? totalCompressedSize, 
            long? dataPageOffset, 
            long? indexPageOffset, 
            long? dictionaryPageOffset,
            RowGroupColumnStatistics? statistics, 
            long? bloomFilterOffset, 
            long? bloomFilterLength)
        {
            ColumnId = columnId;
            PathInSchema = pathInSchema;
            Type = type;
            NumValues = numValues;
            TotalUncompressedSize = totalUncompressedSize;
            TotalCompressedSize = totalCompressedSize;
            DataPageOffset = dataPageOffset;
            IndexPageOffset = indexPageOffset;
            DictionaryPageOffset = dictionaryPageOffset;
            Statistics = statistics;
            BloomFilterOffset = bloomFilterOffset;
            BloomFilterLength = bloomFilterLength;
        }
    }

    public class RowGroupColumnStatistics : IRowGroupColumnStatistics
    {
        public object? Min { get; }
        public object? Max { get; }
        public long? NullCount { get; }
        public long? DistinctCount { get; }
        public object? MinValue { get; }
        public object? MaxValue { get; }
        public bool? IsMinValueExact { get; }
        public bool? IsMaxValueExact { get; }

        public RowGroupColumnStatistics(object? min, object? max, long? nullCount, long? distinctCount, object? minValue, object? maxValue, bool? isMinValueExact, bool? isMaxValueExact)
        {
            Min = min;
            Max = max;
            NullCount = nullCount;
            DistinctCount = distinctCount;
            MinValue = minValue;
            MaxValue = maxValue;
            IsMinValueExact = isMinValueExact;
            IsMaxValueExact = isMaxValueExact;
        }
    }
}
