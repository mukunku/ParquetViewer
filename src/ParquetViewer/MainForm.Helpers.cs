using ParquetViewer.Analytics;
using ParquetViewer.Engine.Exceptions;
using ParquetViewer.Engine.Types;
using ParquetViewer.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ParquetViewer
{
    public partial class MainForm
    {
        private LoadingIcon ShowLoadingIcon(string message, long loadingBarMax = 0)
        {
            var loadingIcon = new LoadingIcon(this, message, loadingBarMax);
            loadingIcon.OnShow += (object? sender, EventArgs e) =>
            {
                this.mainTableLayoutPanel.Enabled = false;
                this.mainMenuStrip.Enabled = false;
            };
            loadingIcon.OnHide += (object? sender, EventArgs e) =>
            {
                this.mainTableLayoutPanel.Enabled = true;
                this.mainMenuStrip.Enabled = true;
            };

            loadingIcon.Show();
            return loadingIcon;
        }

        private async void ExportResults(FileType defaultFileType)
        {
            string? filePath = null;
            LoadingIcon? loadingIcon = null;
            try
            {
                if (this.mainGridView?.RowCount > 0)
                {
                    this.exportFileDialog.Title = string.Format("{0} records will be exported", this.mainGridView.RowCount);
                    this.exportFileDialog.Filter = "CSV file (*.csv)|*.csv|JSON file (*.json)|*.json|Excel file (*.xls)|*.xls";
                    this.exportFileDialog.FilterIndex = (int)defaultFileType + 1;

                    if (this._openParquetEngine?.ParquetSchemaTree?.Children.All(s => s.FieldType() == Engine.ParquetSchemaElement.FieldTypeId.Primitive) == true)
                    {
                        this.exportFileDialog.Filter += "|Parquet file (*.parquet)|*.parquet";
                    }

                    if (this.exportFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        filePath = this.exportFileDialog.FileName;
                        CleanupFile(filePath); //Delete any existing file (user already confirmed any overwrite)

                        var fileExtension = Path.GetExtension(filePath);
                        FileType? selectedFileType = UtilityMethods.ExtensionToFileType(fileExtension);

                        var stopWatch = Stopwatch.StartNew();
                        loadingIcon = this.ShowLoadingIcon("Exporting Data");
                        if (selectedFileType == FileType.CSV)
                        {
                            await Task.Run(() => this.WriteDataToCSVFile(filePath, loadingIcon.CancellationToken));
                        }
                        else if (selectedFileType == FileType.XLS)
                        {
                            const int MAX_XLS_COLUMN_COUNT = 256; //.xls format has a hard limit on 256 columns
                            if (this.MainDataSource!.Columns.Count > MAX_XLS_COLUMN_COUNT)
                            {
                                MessageBox.Show($"the .xls file format supports a maximum of {MAX_XLS_COLUMN_COUNT} columns.{Environment.NewLine}{Environment.NewLine}" +
                                    $"Please try another file format or reduce the amount of columns you are exporting. Your columns: {this.MainDataSource.Columns.Count}",
                                    "Too many columns", MessageBoxButtons.OK, MessageBoxIcon.Error);

                                return;
                            }

                            await Task.Run(() => this.WriteDataToExcelFile(filePath, loadingIcon.CancellationToken));
                        }
                        else if (selectedFileType == FileType.JSON)
                        {
                            await Task.Run(() => this.WriteDataToJSONFile(filePath, loadingIcon.CancellationToken));
                        }
                        else if (selectedFileType == FileType.PARQUET)
                        {
                            await Task.Run(async () => await this.WriteDataToParquetFile(filePath, loadingIcon.CancellationToken));
                        }
                        else
                        {
                            throw new Exception(string.Format("Unsupported export type: '{0}'", fileExtension));
                        }

                        if (loadingIcon.CancellationToken.IsCancellationRequested)
                        {
                            CleanupFile(filePath);
                            MessageBox.Show("Export has been cancelled", "Export Cancelled", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            long fileSizeInBytes = new FileInfo(filePath).Length;
                            FileExportEvent.FireAndForget(selectedFileType.Value, fileSizeInBytes,
                                this.mainGridView.RowCount, this.mainGridView.ColumnCount, stopWatch.ElapsedMilliseconds);
                            MessageBox.Show($"Export successful!{Environment.NewLine}{Environment.NewLine}File size: {Math.Round((fileSizeInBytes / 1024.0) / 1024.0, 2)} MB", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }
            catch (IOException ex)
            {
                CleanupFile(filePath);
                ShowError(ex.Message, "File export failed");
            }
            catch (Exception)
            {
                CleanupFile(filePath);
                throw;
            }
            finally
            {
                loadingIcon?.Dispose();
            }

            static void CleanupFile(string? filePath)
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(filePath))
                        File.Delete(filePath);
                }
                catch (Exception) { /*Swallow*/ }
            }
        }

        private void WriteDataToCSVFile(string path, CancellationToken cancellationToken)
        {
            using var writer = new StreamWriter(path, false, Encoding.UTF8);

            var rowBuilder = new StringBuilder();
            bool isFirst = true;
            foreach (DataColumn column in this.MainDataSource!.Columns)
            {
                if (!isFirst)
                {
                    rowBuilder.Append(',');
                }
                else
                {
                    isFirst = false;
                }

                rowBuilder.Append(
                    column.ColumnName
                        .Replace("\r", string.Empty)
                        .Replace("\n", string.Empty)
                        .Replace(",", string.Empty));
            }
            writer.WriteLine(rowBuilder.ToString());

            foreach (DataRowView row in this.MainDataSource.DefaultView)
            {
                rowBuilder.Clear();

                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                isFirst = true;
                string dateFormat = AppSettings.DateTimeDisplayFormat.GetDateFormat();
                foreach (object? value in row.Row.ItemArray)
                {
                    if (!isFirst)
                    {
                        rowBuilder.Append(',');
                    }
                    else
                    {
                        isFirst = false;
                    }

                    if (value is DateTime dt)
                    {
                        rowBuilder.Append(UtilityMethods.CleanCSVValue(dt.ToString(dateFormat)));
                    }
                    else
                    {
                        var stringValue = value!.ToString()!; //we never have `null` only `DBNull.Value`
                        rowBuilder.Append(UtilityMethods.CleanCSVValue(stringValue));
                    }
                }

                writer.WriteLine(rowBuilder.ToString());
            }
        }

        private void WriteDataToExcelFile(string path, CancellationToken cancellationToken)
        {
            string dateFormat = AppSettings.DateTimeDisplayFormat.GetDateFormat();
            using var fs = new FileStream(path, FileMode.OpenOrCreate);
            var excelWriter = new ExcelWriter(fs);
            excelWriter.BeginWrite();

            //Write headers
            for (int i = 0; i < this.MainDataSource!.Columns.Count; i++)
            {
                excelWriter.WriteCell(0, i, this.MainDataSource.Columns[i].ColumnName);
            }

            //Write data
            for (int i = 0; i < this.MainDataSource.DefaultView.Count; i++)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                for (int j = 0; j < this.MainDataSource.Columns.Count; j++)
                {
                    var value = this.MainDataSource.DefaultView[i][j];
                    if (value is null || value == DBNull.Value)
                    {
                        excelWriter.WriteCell(i + 1, j); //empty cell
                    }
                    else if (IsIntCastSafe(value))
                    {
                        excelWriter.WriteCell(i + 1, j, Convert.ToInt32(value));
                    }
                    else if (IsDoubleCastSafe(value))
                    {
                        excelWriter.WriteCell(i + 1, j, Convert.ToDouble(value));
                    }
                    else if (value is DateTime dt)
                    {
                        excelWriter.WriteCell(i + 1, j, dt.ToString(dateFormat));
                    }
                    else
                    {
                        excelWriter.WriteCell(i + 1, j, value?.ToString() ?? string.Empty);
                    }
                }
            }

            bool IsIntCastSafe(object value) => value.GetType() == typeof(int)
                || value.GetType() == typeof(uint)
                || value.GetType() == typeof(sbyte)
                || value.GetType() == typeof(byte);

            bool IsDoubleCastSafe(object value) => value.GetType() == typeof(double)
                || value.GetType() == typeof(decimal)
                || value.GetType() == typeof(float)
                || value.GetType() == typeof(long);

            excelWriter.EndWrite();
        }

        private void WriteDataToJSONFile(string path, CancellationToken cancellationToken)
        {
            using var fs = new FileStream(path, FileMode.OpenOrCreate);
            using var jsonWriter = new Utf8JsonWriter(fs);

            jsonWriter.WriteStartArray();
            foreach (DataRowView row in this.MainDataSource!.DefaultView)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                jsonWriter.WriteStartObject();
                for (var i = 0; i < row.Row.ItemArray.Length; i++)
                {
                    var columnName = this.MainDataSource.Columns[i].ColumnName;
                    jsonWriter.WritePropertyName(columnName);

                    object? value = row.Row.ItemArray[i];
                    StructValue.WriteValue(jsonWriter, value!, false);
                }
                jsonWriter.WriteEndObject();
            }
            jsonWriter.WriteEndArray();
        }

        private async Task WriteDataToParquetFile(string path, CancellationToken cancellationToken)
        {
            var fields = new List<Parquet.Schema.Field>(this.MainDataSource!.Columns.Count);
            foreach (DataColumn column in this.MainDataSource.Columns)
            {
                fields.Add(this._openParquetEngine!.Schema!.Fields
                    .Where(field => field.Name.Equals(column.ColumnName, StringComparison.InvariantCulture))
                    .First());
            }
            var parquetSchema = new Parquet.Schema.ParquetSchema(fields);

            using var fs = new FileStream(path, FileMode.OpenOrCreate);
            using var parquetWriter = await Parquet.ParquetWriter.CreateAsync(parquetSchema, fs, cancellationToken: cancellationToken);
            parquetWriter.CompressionLevel = System.IO.Compression.CompressionLevel.Optimal;
            parquetWriter.CustomMetadata = new Dictionary<string, string>
            {
                { "ParquetViewer", @"
{
    ""CreatedBy"": ""ParquetViewer"",
    ""Website"": ""https://github.com/mukunku/ParquetViewer"",
    ""CreationDate"": """ + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + @"""
}
" }
            };

            using var rowGroup = parquetWriter.CreateRowGroup();
            foreach (var dataField in parquetSchema.DataFields)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                var type = dataField.IsNullable ? dataField.ClrType.GetNullableVersion() : dataField.ClrType;
                var values = this.MainDataSource.GetColumnValues(type, dataField.Name);
                var dataColumn = new Parquet.Data.DataColumn(dataField, values);
                await rowGroup.WriteColumnAsync(dataColumn, cancellationToken);
            }
        }

        private static void HandleAllFilesSkippedException(AllFilesSkippedException ex)
        {
            var sb = new StringBuilder();
            sb.AppendLine("No valid Parquet files found in folder. Invalid Parquet files:");
            foreach (var skippedFile in ex.SkippedFiles)
            {
                sb.AppendLine($"-{skippedFile.FileName}");
            }
            ShowError(sb.ToString());
        }

        private static void HandleSomeFilesSkippedException(SomeFilesSkippedException ex)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Some files could not be loaded. Invalid Parquet files:");
            foreach (var skippedFile in ex.SkippedFiles)
            {
                sb.AppendLine($"-{skippedFile.FileName}");
            }
            ShowError(sb.ToString());
        }

        private static void HandleFileReadException(FileReadException ex)
        {
            ShowError($"Could not load parquet file.{Environment.NewLine}{Environment.NewLine}" +
                $"If the problem persists please consider opening a bug ticket in the project repo: Help → About{Environment.NewLine}{Environment.NewLine}" +
                $"{ex}");
        }

        private static void HandleMultipleSchemasFoundException(MultipleSchemasFoundException ex)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Multiple schemas detected. ");

            int schemaIndex = 1;
            const int topCount = 5;
            foreach (var schema in ex.Schemas)
            {
                sb.AppendLine($"SCHEMA-{schemaIndex++} FIELDS (TOP 5):");
                for (var i = 0; i < topCount; i++)
                {
                    if (i == schema.Fields.Count)
                        break;

                    sb.AppendLine($"  {schema.Fields.ElementAt(i).Name}");
                }
            }
            ShowError(sb.ToString());
        }

        private static void ShowError(string message, string title = "Something went wrong") => MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}
