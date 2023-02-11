using System;
using System.Collections.Generic;
using System.Linq;
using Apache.Arrow.Ipc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Parquet.Thrift;

namespace Utilities
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

        public static string ThriftMetadataToJSON(FileMetaData thriftMetadata)
        {
            try
            {
                var jsonObject = new JObject();
                jsonObject[nameof(thriftMetadata.Version)] = thriftMetadata.Version;
                jsonObject[nameof(thriftMetadata.Num_rows)] = thriftMetadata.Num_rows;
                jsonObject["Num_row_groups"] = thriftMetadata.Row_groups?.Count ?? 0;
                jsonObject[nameof(thriftMetadata.Created_by)] = thriftMetadata.Created_by;

                var schemas = new JArray();
                foreach (var schema in thriftMetadata.Schema)
                {
                    if ("schema".Equals(schema.Name) && schemas.Count == 0)
                        continue;

                    var schemaObject = new JObject();
                    schemaObject[nameof(schema.Field_id)] = schema.Field_id;
                    schemaObject[nameof(schema.Name)] = schema.Name;
                    schemaObject[nameof(schema.Type)] = schema.Type.ToString();
                    schemaObject[nameof(schema.Type_length)] = schema.Type_length;
                    schemaObject[nameof(schema.LogicalType)] = schema.LogicalType?.ToString();
                    schemaObject[nameof(schema.Scale)] = schema.Scale;
                    schemaObject[nameof(schema.Precision)] = schema.Precision;
                    schemaObject[nameof(schema.Repetition_type)] = schema.Repetition_type.ToString();
                    schemaObject[nameof(schema.Converted_type)] = schema.Converted_type.ToString();

                    schemas.Add(schemaObject);
                }
                jsonObject[nameof(thriftMetadata.Schema)] = schemas;

                var rowGroups = new JArray();
                foreach(var rowGroup in thriftMetadata.Row_groups ?? Enumerable.Empty<RowGroup>())
                {
                    var rowGroupObject = new JObject();
                    rowGroupObject[nameof(rowGroup.Ordinal)] = rowGroup.Ordinal;
                    rowGroupObject[nameof(rowGroup.Num_rows)] = rowGroup.Num_rows;
                    
                    var sortingColumns = new JArray();
                    foreach(var sortingColumn in rowGroup.Sorting_columns ?? Enumerable.Empty<SortingColumn>())
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

        public static string PandasSchemaToJSON(string pandas)
        {
            try
            {
                //Pandas is already json; so just make it pretty.
                return JValue.Parse(pandas).ToString(Formatting.Indented);
            }
            catch (Exception)
            {
                //malformed json detected
                return pandas;
            }
        }
    }
}
