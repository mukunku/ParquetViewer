using MiniExcelLibs;
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
using System.Text.RegularExpressions;
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
        //We can't use the gridview formattedValue directly as we're changing the type sometimes plus enumerating the dgv is really slow due to row unsharing.
        private async void ExportResults(FileType defaultFileType, string? filePathWithExtension = null)
        {
            string? filePath = null;
            LoadingIcon? loadingIcon = null;
            FileType? rerunType = null;
            filePathWithExtension = string.IsNullOrWhiteSpace(filePathWithExtension) ? null : filePathWithExtension;
            try
            {
                if (this.MainDataSource?.DefaultView.Count > 0)
                {
                    this.exportFileDialog.Title = Resources.Strings.RecordsToBeExportedTitleFormat.Format(this.MainDataSource.DefaultView.Count);
                    this.exportFileDialog.Filter = "CSV file (*.csv)|*.csv|JSON file (*.json)|*.json|Excel '93 file (*.xls)|*.xls|Excel '07 file (*.xlsx)|*.xlsx";
                    this.exportFileDialog.FilterIndex = (int)defaultFileType + 1;

                    if (this._openParquetEngine?.ParquetSchemaTree?.Children.All(s => s.FieldType == Engine.ParquetSchemaElement.FieldTypeId.Primitive) == true)
                    {
                        this.exportFileDialog.Filter += "|Parquet file (*.parquet)|*.parquet";
                    }

                    if (filePathWithExtension is not null || this.exportFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        filePath = filePathWithExtension ?? this.exportFileDialog.FileName;
                        CleanupFile(filePath); //Delete any existing file (user already confirmed any overwrite)

                        var fileExtension = Path.GetExtension(filePath);
                        FileType? selectedFileType = UtilityMethods.ExtensionToFileType(fileExtension);

                        var stopWatch = Stopwatch.StartNew();
                        loadingIcon = this.ShowLoadingIcon(Resources.Strings.ExportingDataLabelText, this.MainDataSource.DefaultView.Count * this.MainDataSource.Columns.Count);
                        if (selectedFileType == FileType.CSV)
                        {
                            await WriteDataToCSVFile(this.MainDataSource, filePath, loadingIcon.CancellationToken, loadingIcon);
                        }
                        else if (selectedFileType == FileType.XLS)
                        {
                            const int MAX_XLS_COLUMN_COUNT = 256; //.xls format has a hard limit on 256 columns
                            if (this.MainDataSource!.Columns.Count > MAX_XLS_COLUMN_COUNT)
                            {
                                MessageBox.Show(this,
                                    Resources.Errors.TooManyColumnsXlsErrorMessageFormat.Format(MAX_XLS_COLUMN_COUNT, this.MainDataSource.Columns.Count),
                                    Resources.Errors.TooManyColumnsErrorTitle,
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);

                                return;
                            }

                            await WriteDataToExcel93File(this.MainDataSource, filePath, loadingIcon.CancellationToken, loadingIcon);
                        }
                        else if (selectedFileType == FileType.XLSX)
                        {
                            const int MAX_XLSX_COLUMN_COUNT = 16384; //.xlsx format has a hard limit on 16384 columns
                            if (this.MainDataSource!.Columns.Count > MAX_XLSX_COLUMN_COUNT)
                            {
                                MessageBox.Show(this,
                                    Resources.Errors.TooManyColumnsXlsxErrorMessageFormat.Format(MAX_XLSX_COLUMN_COUNT, this.MainDataSource.Columns.Count),
                                    Resources.Errors.TooManyColumnsErrorTitle, 
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);

                                return;
                            }

                            await WriteDataToExcel2007File(this.MainDataSource, filePath, loadingIcon.CancellationToken, loadingIcon);
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
                            throw new Exception(string.Format(Resources.Errors.UnsupportedExportTypeFormat, fileExtension));
                        }

                        if (loadingIcon.CancellationToken.IsCancellationRequested)
                        {
                            CleanupFile(filePath);
                            MessageBox.Show(Resources.Strings.ExportCancelledMessage, Resources.Strings.ExportCancelledTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            long fileSizeInBytes = new FileInfo(filePath).Length;

                            FileExportEvent.FireAndForget(
                                selectedFileType.Value, 
                                fileSizeInBytes,
                                this.mainGridView.RowCount, 
                                this.mainGridView.ColumnCount, 
                                stopWatch.ElapsedMilliseconds);

                            MessageBox.Show(this,
                                Resources.Strings.ExportSuccessfulMessageFormat.Format(Math.Round((fileSizeInBytes / 1024.0) / 1024.0, 2)),
                                Resources.Strings.ExportSuccessfulTitle, 
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }
            catch (IOException ex)
            {
                CleanupFile(filePath);
                ShowError(ex.Message, Resources.Errors.ExportFailedErrorTitle);
            }
            catch (XlsCellLengthException ex)
            {
                CleanupFile(filePath);
                
                if (MessageBox.Show(this,
                    Resources.Strings.SwitchFromXlsToXlsxMessageFormat.Format(ex.MaxLength, ex.FileType.GetExtension(), FileType.XLSX.GetExtension()),
                    Resources.Strings.SwitchFromXlsToXlsxMessageTitle, 
                    MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation) == DialogResult.OK)
                {
                    rerunType = FileType.XLSX;
                }
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

            if (rerunType is not null)
            {
                ExportResults(default, filePath is not null ? Path.ChangeExtension(filePath, rerunType.Value.GetExtension()) : filePath);
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

        private async Task WriteDataToExcel2007File(DataTable mainDataSource, string path, CancellationToken cancellationToken, IProgress<int> progress)
        {
            const int MAX_XLSX_SHEET_NAME_LENGTH = 31;
            var sheetName = Path.GetFileNameWithoutExtension(this.OpenFileOrFolderPath) ?? "Sheet1";

            //sanitize sheet name
            sheetName = Regex.Replace(sheetName, "[^a-zA-Z0-9 _\\-()]", string.Empty).Left(MAX_XLSX_SHEET_NAME_LENGTH);

            using var fs = new FileStream(path, FileMode.OpenOrCreate);
            await fs.SaveAsAsync(mainDataSource, printHeader: true, sheetName, ExcelType.XLSX, configuration: null, progress, cancellationToken);
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

        private static Task WriteDataToExcel93File(DataTable dataTable, string path, CancellationToken cancellationToken, IProgress<int> progress)
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
                                const int maxSupportedCellLength = 255;
                                if (stringValue!.Length > maxSupportedCellLength)
                                {
                                    throw new XlsCellLengthException(maxSupportedCellLength);
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
                        jsonWriter.WriteStartObject();
                        for (var i = 0; i < row.Row.ItemArray.Length; i++)
                        {
                            if (cancellationToken.IsCancellationRequested)
                            {
                                break;
                            }

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

                    const int MAX_ROWS_PER_ROWGROUP = 100_000; //Without batching we sometimes get OverflowException: Array dimensions exceeded supported range from Parquet.NET
                    var batchIndex = 0;
                    var isLastBatch = false;
                    while (!isLastBatch)
                    {
                        using var rowGroup = parquetWriter.CreateRowGroup();
                        foreach (var dataField in parquetSchema.DataFields)
                        {
                            if (cancellationToken.IsCancellationRequested)
                            {
                                break;
                            }

                            var type = dataField.IsNullable ? dataField.ClrType.GetNullableVersion() : dataField.ClrType;
                            var values = this.MainDataSource.GetColumnValues(type, dataField.Name, batchIndex * MAX_ROWS_PER_ROWGROUP, MAX_ROWS_PER_ROWGROUP);
                            var dataColumn = new Parquet.Data.DataColumn(dataField, values);
                            await rowGroup.WriteColumnAsync(dataColumn, cancellationToken);
                            progress.Report(values.Length); //No way to report progress for each row, so do it by column
                            isLastBatch = values.Length < MAX_ROWS_PER_ROWGROUP;
                        }
                        batchIndex++;
                    }
                }, cancellationToken);

        private static void HandleAllFilesSkippedException(AllFilesSkippedException ex)
        {
            var sb = new StringBuilder();
            sb.AppendLine(Resources.Errors.NoValidParquetFilesFoundErrorMessage);
            foreach (var skippedFile in ex.SkippedFiles)
            {
                sb.AppendLine($"-{skippedFile.FileName}");
            }
            ShowError(sb.ToString());
        }

        private static void HandleSomeFilesSkippedException(SomeFilesSkippedException ex)
        {
            var sb = new StringBuilder();
            sb.AppendLine(Resources.Errors.SomeInvalidParquetFilesFoundErrorMessage);
            foreach (var skippedFile in ex.SkippedFiles)
            {
                sb.AppendLine($"-{skippedFile.FileName}");
            }
            ShowError(sb.ToString());
        }

        private static void HandleFileReadException(FileReadException ex)
        {
            ShowError(Resources.Errors.UnexpectedFileReadErrorMessageFormat.Format(ex));
        }

        private static void HandleFileNotFoundException(FileNotFoundException ex)
        {
            ShowError(ex.Message);
        }

        private static void HandleMultipleSchemasFoundException(MultipleSchemasFoundException ex)
        {
            var sb = new StringBuilder();
            sb.Append(Resources.Errors.MultipleSchemasDetectedErrorMessage);
            sb.AppendLine(" ");

            var schemaIndex = 1;
            const int topCount = 5;
            const int maxSchemasLimit = 10; //prevent a giant textbox from appearing
            foreach (var schema in ex.Schemas)
            {
                sb.AppendLine(Resources.Errors.MultipleSchemasDetectedEntriesErrorMessageFormat.Format(schemaIndex++));
                for (var i = 0; i < topCount; i++)
                {
                    if (i == schema.Fields.Count)
                        break;

                    sb.AppendLine($"  {schema.Fields.ElementAt(i).Name}");
                }

                if (schemaIndex > maxSchemasLimit)
                {
                    sb.AppendLine("...");
                    break;
                }
            }
            ShowError(sb.ToString());
        }

        private static void HandleMalformedFieldException(MalformedFieldException ex)
        {
            ShowError(Resources.Errors.MalformedFieldErrorMessageFormat.Format(ex.Message));
        }

        private static void HandleDecimalOverflowException(DecimalOverflowException ex)            
            => ShowError(
                Resources.Errors.DecimalValueTooLargeErrorMessageFormat.Format(
                    ex.FieldName,
                    ex.Precision,
                    ex.Scale,
                    DecimalOverflowException.MAX_DECIMAL_PRECISION,
                    DecimalOverflowException.MAX_DECIMAL_SCALE),
                Resources.Errors.DecimalValueTooLargeErrorTitle);

        private static void ShowError(string message, string? title = null) 
            => MessageBox.Show(message, title ?? Resources.Errors.GenericErrorMessage, MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}
