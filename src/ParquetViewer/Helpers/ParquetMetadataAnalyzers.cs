using Apache.Arrow.Ipc;
using Parquet.Meta;
using System;
using System.Linq;
using System.Text.Json;

#nullable enable

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

        public static string ThriftMetadataToJSON(Engine.ParquetEngine parquetEngine, long recordCount, int fieldCount)
        {
            try
            {
                object ProcessSchemaTree(Engine.ParquetSchemaElement parquetSchemaElement)
                {
                    return new
                    {
                        parquetSchemaElement.Path,
                        Type = parquetSchemaElement.SchemaElement.Type.ToString(),
                        parquetSchemaElement.SchemaElement.TypeLength,
                        LogicalType = LogicalTypeToJSONObject(parquetSchemaElement.SchemaElement.LogicalType),
                        RepetitionType = parquetSchemaElement.SchemaElement.RepetitionType.ToString(),
                        ConvertedType = parquetSchemaElement.SchemaElement.ConvertedType.ToString(),
                        Children = parquetSchemaElement.Children.Select(pse => ProcessSchemaTree(pse)).ToArray()
                    };
                }

                var jsonObject = new
                {
                    parquetEngine.ThriftMetadata.Version,
                    NumRows = recordCount,
                    NumRowGroups = parquetEngine.ThriftMetadata.RowGroups?.Count ?? -1, //What about partitioned files?
                    NumFields = fieldCount,
                    parquetEngine.ThriftMetadata.CreatedBy,
                    Schema = ProcessSchemaTree(parquetEngine.ParquetSchemaTree),
                    RowGroups = (parquetEngine.ThriftMetadata.RowGroups ?? Enumerable.Empty<RowGroup>()).Select(rowGroup => new
                    {
                        rowGroup.Ordinal,
                        rowGroup.NumRows,
                        SortingColumns = (rowGroup.SortingColumns ?? Enumerable.Empty<SortingColumn>()).Select(sortingColumn => new
                        {
                            sortingColumn.ColumnIdx,
                            sortingColumn.Descending,
                            sortingColumn.NullsFirst
                        }).ToArray(),
                        rowGroup.FileOffset,
                        rowGroup.TotalByteSize,
                        rowGroup.TotalCompressedSize,
                        Columns = (rowGroup.Columns ?? Enumerable.Empty<ColumnChunk>()).Select(column => new
                        {
                            column.FilePath,
                            column.FileOffset,
                            Metadata = new
                            {
                                column.MetaData?.PathInSchema,
                                Type = column.MetaData?.Type.ToString(),
                                column.MetaData?.NumValues,
                                column.MetaData?.TotalUncompressedSize,
                                column.MetaData?.TotalCompressedSize,
                                column.MetaData?.KeyValueMetadata,
                                column.MetaData?.DataPageOffset,
                                column.MetaData?.IndexPageOffset,
                                column.MetaData?.DictionaryPageOffset,
                                column.MetaData?.Statistics,
                                column.MetaData?.EncodingStats
                            }
                        }).ToArray()
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
