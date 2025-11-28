using Apache.Arrow.Ipc;
using Parquet.Meta;
using ParquetViewer.Engine;
using System;
using System.Linq;
using System.Text.Json;

namespace ParquetViewer.Helpers
{
    public static class ParquetMetadataAnalyzers
    {
        public static string ApacheArrowToJSON(string base64)
        {
            try
            {
                byte[] bytes = Convert.FromBase64String(base64);
                using (ArrowStreamReader reader = new(bytes))
                {
                    reader.ReadNextRecordBatch();
                    return JsonSerializer.Serialize(reader.Schema, new JsonSerializerOptions { WriteIndented = true });
                }
            }
            catch (Exception ex)
            {
                return $"Something went wrong while processing the schema:{Environment.NewLine}{Environment.NewLine}{ex}";
            }
        }

        public static string ThriftMetadataToJSON(IParquetEngine parquetEngine, long recordCount, int fieldCount)
        {
            try
            {
                var jsonObject = new
                {
                    parquetEngine.Metadata.ParquetVersion,
                    NumRows = recordCount,
                    NumRowGroups = parquetEngine.Metadata.RowGroupCount, //We assume partitioned files all have the same row group count
                    NumFields = fieldCount,
                    parquetEngine.Metadata.CreatedBy,
                    Schema = parquetEngine.Metadata.SchemaTree,
                    RowGroups = (parquetEngine.Metadata.RowGroups ?? Enumerable.Empty<IRowGroupMetadata>()).Select(rowGroup => new
                    {
                        rowGroup.Ordinal,
                        rowGroup.RowCount,
                        SortingColumns = (rowGroup.SortingColumns ?? Enumerable.Empty<ISortingColumnMetadata>()).Select(sortingColumn => new
                        {
                            sortingColumn.ColumnIdx,
                            sortingColumn.Descending,
                            sortingColumn.NullsFirst
                        }).ToArray(),
                        rowGroup.FileOffset,
                        rowGroup.TotalByteSize,
                        rowGroup.TotalCompressedSize
                    }).ToArray()
                };

                return JsonSerializer.Serialize(jsonObject, new JsonSerializerOptions { WriteIndented = true });
            }
            catch (Exception ex)
            {
                return $"Something went wrong while processing the schema:{Environment.NewLine}{Environment.NewLine}{ex}";
            }

            static object? LogicalTypeToJSONObject(LogicalType? logicalType)
            {
                if (logicalType is null)
                {
                    return null;
                }
                else if (logicalType.STRING is not null)
                {
                    return new { Name = nameof(logicalType.STRING) };
                }
                else if (logicalType.MAP is not null)
                {
                    return new { Name = nameof(logicalType.MAP) };
                }
                else if (logicalType.LIST is not null)
                {
                    return new { Name = nameof(logicalType.LIST) };
                }
                else if (logicalType.ENUM is not null)
                {
                    return new { Name = nameof(logicalType.ENUM) };
                }
                else if (logicalType.DECIMAL is not null)
                {
                    return new
                    {
                        Name = nameof(logicalType.DECIMAL),
                        logicalType.DECIMAL.Scale,
                        logicalType.DECIMAL.Precision
                    };
                }
                else if (logicalType.DATE is not null)
                {
                    return new { Name = nameof(logicalType.DATE) };
                }
                else if (logicalType.TIME is not null)
                {
                    return new
                    {
                        Name = nameof(logicalType.TIME),
                        logicalType.TIME.IsAdjustedToUTC,
                        Unit = TimeUnitToString(logicalType.TIME.Unit)
                    };
                }
                else if (logicalType.TIMESTAMP is not null)
                {
                    return new
                    {
                        Name = nameof(logicalType.TIMESTAMP),
                        logicalType.TIMESTAMP.IsAdjustedToUTC,
                        Unit = TimeUnitToString(logicalType.TIMESTAMP.Unit)
                    };
                }
                else if (logicalType.INTEGER is not null)
                {
                    return new
                    {
                        Name = nameof(logicalType.INTEGER),
                        logicalType.INTEGER.BitWidth,
                        logicalType.INTEGER.IsSigned
                    };
                }
                else if (logicalType.JSON is not null)
                {
                    return new { Name = nameof(logicalType.JSON) };
                }
                else if (logicalType.BSON is not null)
                {
                    return new { Name = nameof(logicalType.BSON) };
                }
                else if (logicalType.UUID is not null)
                {
                    return new { Name = nameof(logicalType.UUID) };
                }
                else if (logicalType.UNKNOWN is not null)
                {
                    return new { Name = $"{logicalType.UNKNOWN.GetType().Name}" };
                }
                else
                {
                    return new { Name = nameof(logicalType.UNKNOWN) };
                }
            }

            static string TimeUnitToString(TimeUnit? timeUnit)
            {
                var timeUnitString = string.Empty;
                if (timeUnit?.MILLIS is not null)
                {
                    timeUnitString = nameof(timeUnit.MILLIS);
                }
                else if (timeUnit?.MICROS is not null)
                {
                    timeUnitString = nameof(timeUnit.MICROS);
                }
                else if (timeUnit?.NANOS is not null)
                {
                    timeUnitString = nameof(timeUnit.NANOS);
                }
                return timeUnitString;
            }
        }

        public static string TryFormatJSON(string possibleJSON)
        {
            try
            {
                var jsonElement = JsonSerializer.Deserialize<JsonElement>(possibleJSON);
                return JsonSerializer.Serialize(jsonElement, new JsonSerializerOptions { WriteIndented = true });
            }
            catch (Exception)
            {
                //malformed json detected
                return possibleJSON;
            }
        }
    }
}
