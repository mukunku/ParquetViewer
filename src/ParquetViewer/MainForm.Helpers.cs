using ParquetViewer.Analytics;
using ParquetViewer.Engine.Exceptions;
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
        private LoadingIcon ShowLoadingIcon(string message, long loadingBarMax = 0)
        {
            var loadingIcon = new LoadingIcon(this, message, loadingBarMax);
            loadingIcon.OnShow += (object sender, EventArgs e) =>
            {
                this.mainTableLayoutPanel.Enabled = false;
                this.mainMenuStrip.Enabled = false;
            };
            loadingIcon.OnHide += (object sender, EventArgs e) =>
            {
                this.mainTableLayoutPanel.Enabled = true;
                this.mainMenuStrip.Enabled = true;
            };

            loadingIcon.Show();
            return loadingIcon;
        }

        private async void ExportResults(FileType defaultFileType)
        {
            string filePath = null;
            LoadingIcon loadingIcon = null;
            try
            {
                if (this.mainGridView.RowCount > 0)
                {
                    this.exportFileDialog.Title = string.Format("{0} records will be exported", this.mainGridView.RowCount);
                    this.exportFileDialog.Filter = "CSV file (*.csv)|*.csv|Excel file (*.xls)|*.xls";
                    this.exportFileDialog.FilterIndex = (int)defaultFileType + 1;
                    if (this.exportFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        filePath = this.exportFileDialog.FileName;
                        var selectedFileType = Path.GetExtension(filePath).Equals(FileType.XLS.GetExtension()) ? FileType.XLS : FileType.CSV;

                        var stopWatch = Stopwatch.StartNew();
                        loadingIcon = this.ShowLoadingIcon("Exporting Data");
                        if (selectedFileType == FileType.CSV)
                        {
                            await Task.Run(() => this.WriteDataToCSVFile(filePath, loadingIcon.CancellationToken));
                        }
                        else if (selectedFileType == FileType.XLS)
                        {
                            const int MAX_XLS_COLUMN_COUNT = 256; //.xls format has a hard limit on 256 columns
                            if (this.MainDataSource.Columns.Count > MAX_XLS_COLUMN_COUNT)
                            {
                                MessageBox.Show($"the .xls file format supports a maximum of {MAX_XLS_COLUMN_COUNT} columns.\r\n\r\nPlease try another file format or reduce the amount of columns you are exporting. Your columns: {this.MainDataSource.Columns.Count}",
                                    "Too many columns", MessageBoxButtons.OK, MessageBoxIcon.Error);

                                return;
                            }

                            await Task.Run(() => this.WriteDataToExcelFile(filePath, loadingIcon.CancellationToken));
                        }
                        else
                        {
                            throw new Exception(string.Format("Unsupported export type: '{0}'", selectedFileType.ToString()));
                        }

                        if (loadingIcon.CancellationToken.IsCancellationRequested)
                        {
                            CleanupFile(filePath);
                            MessageBox.Show("Export has been cancelled", "Export Cancelled", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            FileExportEvent.FireAndForget(selectedFileType, new FileInfo(filePath).Length, this.mainGridView.RowCount, this.mainGridView.ColumnCount, stopWatch.ElapsedMilliseconds);
                            MessageBox.Show("Export successful!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }
            catch(IOException ex)
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

            static void CleanupFile(string filePath)
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
            foreach (DataColumn column in this.MainDataSource.Columns)
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
                foreach (object value in row.Row.ItemArray)
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
                        rowBuilder.Append(UtilityMethods.CleanCSVValue(value.ToString()));
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
            for (int i = 0; i < this.MainDataSource.Columns.Count; i++)
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
                    var value = this.mainDataSource.DefaultView[i][j];
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
                $"If the problem persists please consider opening a bug ticket in the project repo: Help -> About{Environment.NewLine}{Environment.NewLine}" +
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

        /// <summary>
        /// We don't have a way to handle byte arrays at the moment. So let's render them as strings for now as a hack.
        /// Ideally we should set 'AutoGenerateColumns' to 'false' on the gridview and generate the datagridview columns ourselves.
        /// That way we can avoid things like the automatic logic creating 'DataGridViewImageCell' types for byte[] values (#79).
        /// </summary>
        private static void ReplaceUnsupportedColumnTypes(DataTable dataTable)
        {
            const string tempRenameSuffix = "|temp";

            ReplaceByteArrays(dataTable);

            static void ReplaceByteArrays(DataTable data)
            {
                var arrayColumnOrdinals = new List<int>();
                foreach (DataColumn column in data.Columns)
                {
                    if (column.DataType == typeof(byte[]))
                    {
                        arrayColumnOrdinals.Add(column.Ordinal);
                    }
                }

                foreach (var arrayColumnOrdinal in arrayColumnOrdinals)
                {
                    var arrayColumn = data.Columns[arrayColumnOrdinal];

                    string columnName = arrayColumn.ColumnName;
                    string tempColumnName = columnName + tempRenameSuffix;
                    arrayColumn.ColumnName = columnName + tempRenameSuffix;

                    var stringColumnReplacement = new DataColumn(columnName, typeof(string));
                    data.Columns.Add(stringColumnReplacement);

                    //swap the columns
                    data.Columns[columnName].SetOrdinal(data.Columns[tempColumnName].Ordinal);
                    data.Columns[tempColumnName].SetOrdinal(arrayColumnOrdinal);

                    //Stringify and copy the data
                    foreach (DataRow row in data.Rows)
                    {
                        if (row[tempColumnName] is null || row[tempColumnName] == DBNull.Value)
                        {
                            row[columnName] = DBNull.Value;
                        }
                        else
                        {
                            var value = (byte[])row[tempColumnName];
                            row[columnName] = BitConverter.ToString(value);
                        }
                    }

                    //remove the array column
                    data.Columns.Remove(data.Columns[tempColumnName]);
                }
            }
        }
    }
}
