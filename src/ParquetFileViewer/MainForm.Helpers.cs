using ParquetFileViewer.Exceptions;
using ParquetFileViewer.Helpers;
using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ParquetFileViewer
{
    public partial class MainForm
    {
        private Panel loadingPanel = null;
        private const int LoadingPanelWidth = 200;
        private const int LoadingPanelHeight = 200;

        private static void ShowError(Exception ex, string customMessage = null, bool showStackTrace = true)
        {
            MessageBox.Show(string.Concat(customMessage ?? "Something went wrong:", Environment.NewLine, showStackTrace ? ex.ToString() : ex.Message), ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private CancellationToken ShowLoadingIcon(string message)
        {
            var cancellationToken = new CancellationTokenSource();
            this.loadingPanel = new Panel();
            this.loadingPanel.Size = new Size(LoadingPanelWidth, LoadingPanelHeight);
            this.loadingPanel.Location = this.GetFormCenter(LoadingPanelWidth / 2, LoadingPanelHeight / 2);

            this.loadingPanel.Controls.Add(new Label()
            {
                Name = "loadingmessagelabel",
                Text = message,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top
            });

            var pictureBox = new PictureBox()
            {
                Name = "loadingpicturebox",
                Image = Properties.Resources.hourglass,
                Size = new Size(200, 200)
            };
            this.loadingPanel.Controls.Add(pictureBox);

            Button cancelButton = new Button()
            {
                Name = "cancelloadingbutton",
                Text = "Cancel",
                Dock = DockStyle.Bottom,
                Enabled = cancellationToken.Token.CanBeCanceled
            };
            cancelButton.Click += (object buttonSender, EventArgs buttonClickEventArgs) =>
            {
                cancellationToken.Cancel();

                ((Button)buttonSender).Enabled = false;
                ((Button)buttonSender).Text = "Cancelling...";
            };
            this.loadingPanel.Controls.Add(cancelButton);
            cancelButton.BringToFront();

            Cursor.Current = Cursors.WaitCursor;

            this.Controls.Add(this.loadingPanel);

            this.loadingPanel.BringToFront();
            this.loadingPanel.Show();

            this.mainTableLayoutPanel.Enabled = false;
            this.mainMenuStrip.Enabled = false;

            return cancellationToken.Token;
        }

        private void HideLoadingIcon()
        {
            this.mainTableLayoutPanel.Enabled = true;
            this.mainMenuStrip.Enabled = true;
            Cursor.Current = Cursors.Default;

            if (this.loadingPanel != null)
                this.loadingPanel.Dispose();
        }

        private Point GetFormCenter(int offsetX, int offsetY)
        {
            return new Point((this.Size.Width / 2) - offsetX, (this.Size.Height / 2) - offsetY);
        }

        private async void ExportResults(FileType defaultFileType)
        {
            string filePath = null;
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

                        var cancellationToken = this.ShowLoadingIcon("Exporting Data");
                        if (selectedFileType == FileType.CSV)
                        {
                            await Task.Run(() => this.WriteDataToCSVFile(filePath, cancellationToken));
                        }
                        else if (selectedFileType == FileType.XLS)
                        {
                            await Task.Run(() => this.WriteDataToExcelFile(filePath, cancellationToken));
                        }
                        else
                        {
                            throw new Exception(string.Format("Unsupported export type: '{0}'", selectedFileType.ToString()));
                        }

                        if (cancellationToken.IsCancellationRequested)
                        {
                            CleanupFile(filePath);
                            MessageBox.Show("Export has been cancelled", "Export Cancelled", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("Export successful!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }
            catch (Exception)
            {
                CleanupFile(filePath);
                throw;
            }
            finally
            {
                this.HideLoadingIcon();
            }

            void CleanupFile(string filePath)
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

                    if (value is DateTime dt)
                    {
                        excelWriter.WriteCell(i + 1, j, dt.ToString(dateFormat));
                    }
                    else
                    {
                        excelWriter.WriteCell(i + 1, j, value?.ToString() ?? string.Empty);
                    }
                }
            }

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
            ShowError(new Exception(sb.ToString(), ex.SkippedFiles.FirstOrDefault()?.Exception));
        }

        private static void HandleSomeFilesSkippedException(SomeFilesSkippedException ex)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Some files could not be loaded. Invalid Parquet files:");
            foreach (var skippedFile in ex.SkippedFiles)
            {
                sb.AppendLine($"-{skippedFile.FileName}");
            }
            ShowError(new Exception(sb.ToString(), ex.SkippedFiles.FirstOrDefault()?.Exception));
        }

        private static void HandleFileReadException(FileReadException ex)
        {
            ShowError(new Exception($"Could not load parquet file.{Environment.NewLine}{Environment.NewLine}" +
                $"If the problem persists please consider opening a bug ticket in the project repo: Help -> About{Environment.NewLine}", ex));
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
            ShowError(new Exception(sb.ToString(), ex));
        }
    }
}
