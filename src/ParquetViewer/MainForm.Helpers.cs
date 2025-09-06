using ParquetViewer.Analytics;
using ParquetViewer.Engine.Exceptions;
using ParquetViewer.Engine.Types;
using ParquetViewer.Exceptions;
using ParquetViewer.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ParquetViewer
{
    public partial class MainForm
    {
        public LoadingIcon ShowLoadingIcon(string message, long loadingBarMax = 0)
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

        //TODO: Should we export floats and binary data with custom formatting if activated?
        //E.g. float -> Decimal format, Binary -> Size format, etc.
        //We can't use the gridview formattedValue directly as we're changing the type sometimes.
        private async void ExportResults(FileType defaultFileType)
        {
            string? filePath = null;
            LoadingIcon? loadingIcon = null;
            try
            {
                if (this.MainDataSource?.DefaultView.Count > 0)
                {
                    this.exportFileDialog.Title = string.Format("{0} records will be exported", this.MainDataSource.DefaultView.Count);
                    this.exportFileDialog.Filter = "CSV file (*.csv)|*.csv|JSON file (*.json)|*.json|Excel file (*.xls)|*.xls";
                    this.exportFileDialog.FilterIndex = (int)defaultFileType + 1;

                    if (this._openParquetEngine?.ParquetSchemaTree?.Children.All(s => s.FieldType == Engine.ParquetSchemaElement.FieldTypeId.Primitive) == true)
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
                        loadingIcon = this.ShowLoadingIcon("Exporting Data", this.MainDataSource.DefaultView.Count * this.MainDataSource.Columns.Count);
                        if (selectedFileType == FileType.CSV)
                        {
                            await WriteDataToCSVFile(this.MainDataSource, filePath, loadingIcon.CancellationToken, loadingIcon);
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

                            await WriteDataToExcelFile(this.MainDataSource, filePath, loadingIcon.CancellationToken, loadingIcon);
                        }
                        else if (selectedFileType == FileType.JSON)
                        {
                            await WriteDataToJSONFile(this.MainDataSource, filePath, loadingIcon.CancellationToken, loadingIcon);
                        }
                        else if (selectedFileType == FileType.PARQUET)
                        {
                            await this.WriteDataToParquetFile(filePath, loadingIcon.CancellationToken, loadingIcon);
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

        private static Task WriteDataToCSVFile(DataTable dataTable, string path, CancellationToken cancellationToken, IProgress<int> progress)
            => Task.Run(() =>
                {
                    using var writer = new StreamWriter(path, false, Encoding.UTF8);

                    var rowBuilder = new StringBuilder();
                    bool isFirst = true;
                    foreach (DataColumn column in dataTable.Columns)
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

                    string dateFormat = AppSettings.DateTimeDisplayFormat.GetDateFormat();
                    foreach (DataRowView row in dataTable.DefaultView)
                    {
                        rowBuilder.Clear();

                        isFirst = true;
                        foreach (object? value in row.Row.ItemArray)
                        {
                            if (cancellationToken.IsCancellationRequested)
                            {
                                break;
                            }

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

                            progress.Report(1);
                        }

                        writer.WriteLine(rowBuilder.ToString());
                    }
                }, cancellationToken);

        private static Task WriteDataToExcelFile(DataTable dataTable, string path, CancellationToken cancellationToken, IProgress<int> progress)
            => Task.Run(() =>
                {
                    string dateFormat = AppSettings.DateTimeDisplayFormat.GetDateFormat();
                    using var fs = new FileStream(path, FileMode.OpenOrCreate);
                    var excelWriter = new ExcelWriter(fs);
                    excelWriter.BeginWrite();

                    //Write headers
                    for (int i = 0; i < dataTable.Columns.Count; i++)
                    {
                        excelWriter.WriteCell(0, i, dataTable.Columns[i].ColumnName);
                    }

                    //Write data
                    for (int i = 0; i < dataTable.DefaultView.Count; i++)
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            break;
                        }

                        for (int j = 0; j < dataTable.Columns.Count; j++)
                        {
                            var value = dataTable.DefaultView[i][j];
                            if (value == DBNull.Value)
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
                                var stringValue = value.ToString();

                                //BUG: for some reason strings longer than 255 characters appear empty.
                                //Don't know how to fix it so throwing for now...
                                const int maxSupportedCellLength = 255;
                                if (stringValue!.Length > maxSupportedCellLength)
                                {
                                    throw new XlsCellLengthException("Maximum 255 characters per cell are supported. Please try another file format.");
                                }

                                excelWriter.WriteCell(i + 1, j, stringValue);
                            }
                            progress.Report(1);
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
                }, cancellationToken);

        private static Task WriteDataToJSONFile(DataTable dataTable, string path, CancellationToken cancellationToken, IProgress<int> progress)
            => Task.Run(() =>
                {
                    using var fs = new FileStream(path, FileMode.OpenOrCreate);
                    using var jsonWriter = new Engine.Utf8JsonWriterWithRunningLength(fs);

                    jsonWriter.WriteStartArray();
                    foreach (DataRowView row in dataTable.DefaultView)
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            break;
                        }

                        jsonWriter.WriteStartObject();
                        for (var i = 0; i < row.Row.ItemArray.Length; i++)
                        {
                            var columnName = dataTable.Columns[i].ColumnName;
                            jsonWriter.WritePropertyName(columnName);

                            object? value = row.Row.ItemArray[i];
                            StructValue.WriteValue(jsonWriter, value!, false);
                            progress.Report(1);
                        }
                        jsonWriter.WriteEndObject();
                    }
                    jsonWriter.WriteEndArray();
                }, cancellationToken);

        private Task WriteDataToParquetFile(string path, CancellationToken cancellationToken, IProgress<int> progress)
            => Task.Run(async () =>
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
                                {
"ParquetViewer", @"
{
    ""CreatedWith"": ""ParquetViewer"",
    ""Version"": """ + Env.AssemblyVersion.ToString() + @""",
    ""Website"": ""https://github.com/mukunku/ParquetViewer"",
    ""CreationDate"": """ + DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ") + @"""
}"
                                }
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
                        progress.Report(values.Length); //No way to report progress for each row, so do it by column
                    }
                }, cancellationToken);

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

        private static void HandleFileNotFoundException(FileNotFoundException ex)
        {
            ShowError(ex.Message);
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

        private static void HandleMalformedFieldException(MalformedFieldException ex)
        {
            ShowError($"{ex.Message}{Environment.NewLine}{Environment.NewLine}" +
                $"If you think the file is valid please consider opening an issue in the GitHub repo. See: Help → About");
        }

        private static void HandleDecimalOverflowException(DecimalOverflowException ex)
            => ShowError($"Field `{ex.FieldName}` with type DECIMAL({ex.Precision}, {ex.Scale}) contains values outside ParquetViewer's supported range between " +
                $"DECIMAL({DecimalOverflowException.MAX_DECIMAL_PRECISION}, {DecimalOverflowException.MAX_DECIMAL_SCALE}) and DECIMAL({DecimalOverflowException.MAX_DECIMAL_PRECISION}, 0)",
                "Decimal value too large");

        private static void ShowError(string message, string title = "Something went wrong") => MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}
