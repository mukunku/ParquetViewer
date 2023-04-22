using Apache.Arrow.Ipc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Parquet.Thrift;
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
                    [nameof(thriftMetadata.Num_rows)] = recordCount,
                    ["Num_row_groups"] = thriftMetadata.Row_groups?.Count ?? -1, //What about partitioned files?
                    ["Num_fields"] = fieldCount,
                    [nameof(thriftMetadata.Created_by)] = thriftMetadata.Created_by
                };

                var schemas = new JArray();
                foreach (var schema in thriftMetadata.Schema)
                {
                    if ("schema".Equals(schema.Name) && schemas.Count == 0)
                        continue;

                    var schemaObject = new JObject
                    {
                        [nameof(schema.Field_id)] = schema.Field_id,
                        [nameof(schema.Name)] = schema.Name,
                        [nameof(schema.Type)] = schema.Type.ToString(),
                        [nameof(schema.Type_length)] = schema.Type_length,
                        [nameof(schema.LogicalType)] = schema.LogicalType?.ToString(),
                        [nameof(schema.Scale)] = schema.Scale,
                        [nameof(schema.Precision)] = schema.Precision,
                        [nameof(schema.Repetition_type)] = schema.Repetition_type.ToString(),
                        [nameof(schema.Converted_type)] = schema.Converted_type.ToString()
                    };

                    schemas.Add(schemaObject);
                }
                jsonObject[nameof(thriftMetadata.Schema)] = schemas;

                var rowGroups = new JArray();
                foreach (var rowGroup in thriftMetadata.Row_groups ?? Enumerable.Empty<RowGroup>())
                {
                    var rowGroupObject = new JObject();
                    rowGroupObject[nameof(rowGroup.Ordinal)] = rowGroup.Ordinal;
                    rowGroupObject[nameof(rowGroup.Num_rows)] = rowGroup.Num_rows;

                    var sortingColumns = new JArray();
                    foreach (var sortingColumn in rowGroup.Sorting_columns ?? Enumerable.Empty<SortingColumn>())
                    {
                        var sortingColumnObject = new JObject();
                        sortingColumnObject[nameof(sortingColumn.Column_idx)] = sortingColumn.Column_idx;
                        sortingColumnObject[nameof(sortingColumn.Descending)] = sortingColumn.Descending;
                        sortingColumnObject[nameof(sortingColumn.Nulls_first)] = sortingColumn.Nulls_first;

                        sortingColumns.Add(sortingColumnObject);
                    }

                    rowGroupObject[nameof(rowGroup.Sorting_columns)] = sortingColumns;
                    rowGroupObject[nameof(rowGroup.File_offset)] = rowGroup.File_offset;
                    rowGroupObject[nameof(rowGroup.Total_byte_size)] = rowGroup.Total_byte_size;
                    rowGroupObject[nameof(rowGroup.Total_compressed_size)] = rowGroup.Total_compressed_size;

                    rowGroups.Add(rowGroupObject);
                }
                jsonObject[nameof(thriftMetadata.Row_groups)] = rowGroups;

                return jsonObject.ToString(Formatting.Indented);
            }
            catch (Exception ex)
            {
                return $"Something went wrong while processing the schema:{Environment.NewLine}{Environment.NewLine}{ex.ToString()}";
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
