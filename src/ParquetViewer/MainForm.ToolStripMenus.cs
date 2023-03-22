using ParquetViewer.Helpers;
using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace ParquetViewer
{
    public partial class MainForm
    {
        private const string DefaultTableName = "MY_TABLE";

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.OpenFileOrFolderPath = null;
        }

        private async void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.openParquetFileDialog.ShowDialog(this) == DialogResult.OK)
                {
                    await this.OpenNewFileOrFolder(this.openParquetFileDialog.FileName);
                }
            }
            catch (Exception ex)
            {
                this.OpenFileOrFolderPath = null;
                ShowError(ex);
            }
        }

        private async void openFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.openFolderDialog.ShowDialog(this) == DialogResult.OK)
                {
                    await this.OpenNewFileOrFolder(this.openFolderDialog.SelectedPath);
                }
            }
            catch (Exception ex)
            {
                this.OpenFileOrFolderPath = null;
                ShowError(ex);
            }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                this.ExportResults(default);
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private async void changeFieldsMenuStripButton_Click(object sender, EventArgs e)
        {
            try
            {
                await this.OpenFieldSelectionDialog(true);
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }

        private void GetSQLCreateTableScriptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                var openFileOrFolderPath = this.OpenFileOrFolderPath;
                if (openFileOrFolderPath?.EndsWith("/") == true)
                {
                    //trim trailing slash '/'
                    openFileOrFolderPath = openFileOrFolderPath[..^1];
                }

                string tableName = Path.GetFileNameWithoutExtension(openFileOrFolderPath) ?? DefaultTableName;
                if (this.mainDataSource?.Columns.Count > 0)
                {
                    var dataset = new DataSet();

                    this.mainDataSource.TableName = tableName;
                    dataset.Tables.Add(this.mainDataSource);

                    var scriptAdapter = new CustomScriptBasedSchemaAdapter();
                    string sql = scriptAdapter.GetSchemaScript(dataset, false);

                    Clipboard.SetText(sql);
                    MessageBox.Show(this, "Create table script copied to clipboard!", "Parquet Viewer", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                    MessageBox.Show(this, "Please select some fields first to get the SQL script", "Parquet Viewer", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }

        private void MetadataViewerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (IsAnyFileOpen)
                {
                    using var metadataViewer = new MetadataViewer(this._openParquetEngine);
                    metadataViewer.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }

        private void changeColumnSizingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                DataGridViewAutoSizeColumnsMode columnSizingMode;
                if (sender is ToolStripMenuItem tsi && tsi.Tag != null
                    && Enum.TryParse(tsi.Tag.ToString(), out columnSizingMode)
                    && AppSettings.AutoSizeColumnsMode != columnSizingMode)
                {
                    AppSettings.AutoSizeColumnsMode = columnSizingMode;
                    foreach (ToolStripMenuItem toolStripItem in tsi.GetCurrentParent().Items)
                    {
                        toolStripItem.Checked = toolStripItem.Tag?.Equals(tsi.Tag) == true;
                    }

                    var tempMainDataSource = this.MainDataSource;
                    if (columnSizingMode == DataGridViewAutoSizeColumnsMode.None)
                        this.MainDataSource = null; //Need to reload the entire grid to return to the default sizing

                    this.MainDataSource = tempMainDataSource; //Will cause a refresh of the column rendering
                }
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }

        private void rememberRecordCountToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                this.rememberRecordCountToolStripMenuItem.Checked = !this.rememberRecordCountToolStripMenuItem.Checked;
                AppSettings.RememberLastRowCount = this.rememberRecordCountToolStripMenuItem.Checked;
                AppSettings.LastRowCount = this.CurrentMaxRowCount;
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            (new AboutBox()).ShowDialog(this);
        }

        private void userGuideToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start(new ProcessStartInfo(Constants.WikiURL) { UseShellExecute = true });
        }

        private void DateFormatMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (sender is ToolStripMenuItem item)
                {
                    var selectedDateFormat = (DateFormat)(int.Parse((string)item.Tag));
                    AppSettings.DateTimeDisplayFormat = selectedDateFormat;
                    this.RefreshDateFormatMenuItemSelection();
                    this.MainDataSource = this.MainDataSource; //Will cause a refresh of the date formats
                }
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }
    }
}
