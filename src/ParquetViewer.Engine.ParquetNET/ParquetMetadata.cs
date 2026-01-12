using Parquet.Meta;

namespace ParquetViewer.Engine.ParquetNET
{
    public class ParquetMetadata : IParquetMetadata
    {
        public int ParquetVersion { get; }

        public int RowGroupCount { get; }

        public string CreatedBy { get; }

        public ICollection<IRowGroupMetadata> RowGroups { get; }

        public IParquetSchemaElement SchemaTree { get; }

        public int RowCount { get; }

        public ParquetMetadata(FileMetaData thriftMetadata, ParquetSchemaElement schemaTree, int recordCount)
        {
            RowCount = recordCount;
            RowGroupCount = thriftMetadata.RowGroups.Count;
            ParquetVersion = thriftMetadata.Version;
            CreatedBy = thriftMetadata.CreatedBy ?? string.Empty;
            SchemaTree = schemaTree;

            List<RowGroupMetadata> rowGroupMetadataList = new();
            var rowGroupIndex = -1;
            foreach (var rowGroup in thriftMetadata.RowGroups)
            {
                rowGroupIndex++;

                List<RowGroupColumnMetadata> columnMetadataList = new();
                var columnIndex = -1;
                foreach (var column in rowGroup.Columns)
                {
                    columnIndex++;
                    if (column.MetaData is null)
                        continue;

                    ParquetSchemaElement? field = null;
                    try
                    {
                        var currentNode = schemaTree;
                        foreach (var path in column.MetaData.PathInSchema)
                        {
                            currentNode = currentNode.GetChild(path);
                        }
                        field = currentNode;
                    }
                    catch
                    {
                        /*swallow*/
                    }

                    var columnMetadata = new RowGroupColumnMetadata(
                        columnIndex,
                        string.Join("/", column.MetaData.PathInSchema),
                        column.MetaData.Type.ToString(),
                        (int)column.MetaData.NumValues,
                        column.MetaData.TotalUncompressedSize,
                        column.MetaData.TotalCompressedSize,
                        column.MetaData.DataPageOffset,
                        column.MetaData.IndexPageOffset,
                        column.MetaData.DictionaryPageOffset,
                        column.MetaData.Statistics is not null ? new RowGroupColumnStatistics(
                            column.MetaData.Statistics.Min,
                            column.MetaData.Statistics.Max,
                            column.MetaData.Statistics.NullCount,
                            column.MetaData.Statistics.DistinctCount,
                            column.MetaData.Statistics.MinValue,
                            column.MetaData.Statistics.MaxValue,
                            column.MetaData.Statistics.IsMinValueExact,
                            column.MetaData.Statistics.IsMaxValueExact,
                            field
                        ) : null,
                        column.MetaData.BloomFilterOffset,
                        column.MetaData.BloomFilterLength);

                    columnMetadataList.Add(columnMetadata);
                }

                rowGroupMetadataList.Add(new RowGroupMetadata(
                    rowGroup.Ordinal.HasValue ? (int)rowGroup.Ordinal.Value : rowGroupIndex,
                    (int)rowGroup.NumRows,
                    rowGroup.Columns.Count,
                    rowGroup.SortingColumns?.Select(sc => new SortingColumnMetadata(sc.ColumnIdx, sc.Descending, sc.NullsFirst))
                        .Cast<ISortingColumnMetadata>().ToList(),
                    columnMetadataList.ToList<IRowGroupColumnMetadata>(),
                    rowGroup.FileOffset ?? 0,
                    rowGroup.TotalByteSize,
                    rowGroup.TotalCompressedSize ?? 0));
            }

            RowGroups = rowGroupMetadataList.ToList<IRowGroupMetadata>();
        }
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

        public RowGroupMetadata(int ordinal, int rowCount, int columnCount, ICollection<ISortingColumnMetadata>? sortingColumnMetadata,
            ICollection<IRowGroupColumnMetadata>? columns, long fileOffset, long totalByteSize, long totalCompressedSize)
        {
            Ordinal = ordinal;
            RowCount = rowCount;
            ColumnCount = columnCount;
            SortingColumns = sortingColumnMetadata;
            Columns = columns;
            FileOffset = fileOffset;
            TotalByteSize = totalByteSize;
            TotalCompressedSize = totalCompressedSize;
        }
    }

    public class SortingColumnMetadata : ISortingColumnMetadata
    {
        public int ColumnIdx { get; }
        public bool Descending { get; }
        public bool NullsFirst { get; }

        public SortingColumnMetadata(int columnIdx, bool descending, bool nullsFirst)
        {
            ColumnIdx = columnIdx;
            Descending = descending;
            NullsFirst = nullsFirst;
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

        public RowGroupColumnStatistics(object? min, object? max, long? nullCount, long? distinctCount,
            object? minValue, object? maxValue, bool? isMinValueExact, bool? isMaxValueExact, ParquetSchemaElement? field)
        {
            if (min is not null && minValue is not null && Engine.Helpers.ByteArraysEqual(min as byte[], minValue as byte[]) == 0)
                min = null; //don't show the same data twice in the deprecated field
            if (max is not null && maxValue is not null && Engine.Helpers.ByteArraysEqual(max as byte[], maxValue as byte[]) == 0)
                max = null; //don't show the same data twice in the deprecated field

            Min = field is not null ? TryDeserializeValue(min as byte[], field) : min;
            Max = field is not null ? TryDeserializeValue(max as byte[], field) : max;
            NullCount = nullCount;
            DistinctCount = distinctCount;
            MinValue = field is not null ? TryDeserializeValue(minValue as byte[], field) : minValue;
            MaxValue = field is not null ? TryDeserializeValue(maxValue as byte[], field) : maxValue;
            IsMinValueExact = isMinValueExact;
            IsMaxValueExact = isMaxValueExact;
        }

        private object? TryDeserializeValue(byte[]? value, ParquetSchemaElement field)
        {
            try
            {
                if (value == null || value.Length == 0)
                    return value;

                var type = field.ClrType;

                if (type == typeof(string))
                    return System.Text.Encoding.UTF8.GetString(value);

                if (type == typeof(byte))
                    return BitConverter.ToUInt32(value, 0);

                if (type == typeof(sbyte))
                    return BitConverter.ToInt32(value, 0);

                if (type == typeof(short))
                    return BitConverter.ToInt16(value, 0);

                if (type == typeof(ushort))
                    return BitConverter.ToUInt16(value, 0);

                if (type == typeof(int))
                    return BitConverter.ToInt32(value, 0);

                if (type == typeof(uint))
                    return BitConverter.ToUInt32(value, 0);

                if (type == typeof(long))
                    return BitConverter.ToInt64(value, 0);

                if (type == typeof(ulong))
                    return BitConverter.ToUInt64(value, 0);

                if (type == typeof(float))
                    return BitConverter.ToSingle(value, 0);

                if (type == typeof(double))
                    return BitConverter.ToDouble(value, 0);

                if (type == typeof(bool))
                    return BitConverter.ToBoolean(value, 0);

                if (type == typeof(DateTime))
                {
                    var ticks = BitConverter.ToInt64(value, 0);
                    var timeUnit = field.SchemaElement.LogicalType?.TIMESTAMP?.Unit;

                    if (timeUnit?.MILLIS is not null)
                        return DateTime.UnixEpoch.AddMilliseconds(ticks);
                    else if (timeUnit?.MICROS is not null)
                        return DateTime.UnixEpoch.AddMicroseconds(ticks);
                    else if (timeUnit?.NANOS is not null)
                        return DateTime.UnixEpoch.AddMicroseconds(ticks / 1000);
                    else
                        return ticks;
                }

                if (type == typeof(DateOnly))
                    return DateOnly.FromDateTime(DateTime.UnixEpoch)
                        .AddDays(BitConverter.ToInt32(value, 0));

                if (type == typeof(Guid))
                    return new Guid(value);

                //give up
                return value;
            }
            catch
            {
                return value;
            }
        }
    }
}