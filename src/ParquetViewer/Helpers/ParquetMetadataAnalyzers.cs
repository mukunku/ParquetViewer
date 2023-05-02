using Apache.Arrow.Ipc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Parquet.Meta;
using System;
using System.Linq;

namespace ParquetViewer.Helpers
{
    public static class ParquetMetadataAnalyzers
    {
        public static string ApacheArrowToJSON(string base64)
        {
            try
            {
                byte[] bytes = Convert.FromBase64String(base64);
                using (ArrowStreamReader reader = new ArrowStreamReader(bytes))
                {
                    reader.ReadNextRecordBatch();
                    return JsonConvert.SerializeObject(reader.Schema, Formatting.Indented);
                }
            }
            catch (Exception ex)
            {
                return $"Something went wrong while processing the schema:{Environment.NewLine}{Environment.NewLine}{ex.ToString()}";
            }
        }

        public static string ThriftMetadataToJSON(FileMetaData thriftMetadata, long recordCount, int fieldCount)
        {
            try
            {
                var jsonObject = new JObject
                {
                    [nameof(thriftMetadata.Version)] = thriftMetadata.Version,
                    [nameof(thriftMetadata.NumRows)] = recordCount,
                    ["NumRowGroups"] = thriftMetadata.RowGroups?.Count ?? -1, //What about partitioned files?
                    ["NumFields"] = fieldCount,
                    [nameof(thriftMetadata.CreatedBy)] = thriftMetadata.CreatedBy
                };

                var schemas = new JArray();
                foreach (var schema in thriftMetadata.Schema)
                {
                    if ("schema".Equals(schema.Name) && schemas.Count == 0)
                        continue;

                    var schemaObject = new JObject
                    {
                        [nameof(schema.FieldId)] = schema.FieldId,
                        [nameof(schema.Name)] = schema.Name,
                        [nameof(schema.Type)] = schema.Type.ToString(),
                        [nameof(schema.TypeLength)] = schema.TypeLength,
                        [nameof(schema.LogicalType)] = schema.LogicalType?.ToString(),
                        [nameof(schema.Scale)] = schema.Scale,
                        [nameof(schema.Precision)] = schema.Precision,
                        [nameof(schema.RepetitionType)] = schema.RepetitionType.ToString(),
                        [nameof(schema.ConvertedType)] = schema.ConvertedType.ToString()
                    };

                    schemas.Add(schemaObject);
                }
                jsonObject[nameof(thriftMetadata.Schema)] = schemas;

                var rowGroups = new JArray();
                foreach (var rowGroup in thriftMetadata.RowGroups ?? Enumerable.Empty<RowGroup>())
                {
                    var rowGroupObject = new JObject();
                    rowGroupObject[nameof(rowGroup.Ordinal)] = rowGroup.Ordinal;
                    rowGroupObject[nameof(rowGroup.NumRows)] = rowGroup.NumRows;

                    var sortingColumns = new JArray();
                    foreach (var sortingColumn in rowGroup.SortingColumns ?? Enumerable.Empty<SortingColumn>())
                    {
                        var sortingColumnObject = new JObject();
                        sortingColumnObject[nameof(sortingColumn.ColumnIdx)] = sortingColumn.ColumnIdx;
                        sortingColumnObject[nameof(sortingColumn.Descending)] = sortingColumn.Descending;
                        sortingColumnObject[nameof(sortingColumn.NullsFirst)] = sortingColumn.NullsFirst;

                        sortingColumns.Add(sortingColumnObject);
                    }

                    rowGroupObject[nameof(rowGroup.SortingColumns)] = sortingColumns;
                    rowGroupObject[nameof(rowGroup.FileOffset)] = rowGroup.FileOffset;
                    rowGroupObject[nameof(rowGroup.TotalByteSize)] = rowGroup.TotalByteSize;
                    rowGroupObject[nameof(rowGroup.TotalCompressedSize)] = rowGroup.TotalCompressedSize;

                    rowGroups.Add(rowGroupObject);
                }
                jsonObject[nameof(thriftMetadata.RowGroups)] = rowGroups;

                return jsonObject.ToString(Formatting.Indented);
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
                return JToken.Parse(possibleJSON).ToString(Formatting.Indented);
            }
            catch (Exception)
            {
                //malformed json detected
                return possibleJSON;
            }
        }
    }
}
