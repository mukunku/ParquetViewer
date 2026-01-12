using Apache.Arrow.Ipc;
using ParquetViewer.Engine;
using System;
using System.Collections.Generic;
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
                    Schema = new
                    {
                        parquetEngine.Metadata.SchemaTree.Path,
                        RepetitionType = parquetEngine.Metadata.SchemaTree.RepetitionType?.ToString().ToUpper(),
                        Children = ProcessChildren(parquetEngine.Metadata.SchemaTree)
                    },
                    RowGroups = (parquetEngine.Metadata.RowGroups ?? Enumerable.Empty<IRowGroupMetadata>()).Select(rowGroup => new
                    {
                        rowGroup.Ordinal,
                        rowGroup.RowCount,
                        rowGroup.SortingColumns,
                        rowGroup.Columns,
                        rowGroup.FileOffset,
                        rowGroup.TotalByteSize,
                        rowGroup.TotalCompressedSize
                    }).ToArray()
                };

                IEnumerable<object> ProcessChildren(IParquetSchemaElement schemaElement)
                {
                    foreach (var child in (schemaElement.Children ?? Enumerable.Empty<IParquetSchemaElement>()))
                    {
                        yield return new
                        {
                            child.Path,
                            child.Type,
                            child.TypeLength,
                            child.LogicalType,
                            RepetitionType = child.RepetitionType?.ToString().ToUpper(),
                            child.ConvertedType,
                            child.Scale,
                            child.Precision,
                            child.NumChildren,
                            Children = child.Children.Select(ProcessChildren)
                        };
                    }
                }

                return JsonSerializer.Serialize(jsonObject, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                });
            }
            catch (Exception ex)
            {
                return $"Something went wrong while processing the schema:{Environment.NewLine}{Environment.NewLine}{ex}";
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