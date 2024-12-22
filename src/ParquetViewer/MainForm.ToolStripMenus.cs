using ParquetViewer.Analytics;
using ParquetViewer.Helpers;
using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ParquetViewer
{
    public partial class MainForm
    {
        private const string DEFAULT_TABLE_NAME = "MY_TABLE";

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MenuBarClickEvent.FireAndForget(MenuBarClickEvent.ActionId.FileNew);
            this.OpenFileOrFolderPath = null;
        }

        private async void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.openParquetFileDialog.ShowDialog(this) == DialogResult.OK)
                {
                    MenuBarClickEvent.FireAndForget(MenuBarClickEvent.ActionId.FileOpen);
                    await this.OpenNewFileOrFolder(this.openParquetFileDialog.FileName);
                }
            }
            catch
            {
                this.OpenFileOrFolderPath = null;
                throw;
            }
        }

        private async void openFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.openFolderDialog.ShowDialog(this) == DialogResult.OK)
                {
                    MenuBarClickEvent.FireAndForget(MenuBarClickEvent.ActionId.FolderOpen);
                    await this.OpenNewFileOrFolder(this.openFolderDialog.SelectedPath);
                }
            }
            catch
            {
                this.OpenFileOrFolderPath = null;
                throw;
            }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e) => this.ExportResults(default);

        private async void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            await new MenuBarClickEvent { Action = MenuBarClickEvent.ActionId.Exit }.Record();
            this.Close();
        }

        private async void changeFieldsMenuStripButton_Click(object sender, EventArgs e)
        {
            MenuBarClickEvent.FireAndForget(MenuBarClickEvent.ActionId.ChangeFields);
            var fieldList = await this.OpenFieldSelectionDialog(true);
            if (fieldList is not null)
                this.SelectedFields = fieldList; //triggers a file load
        }

        private void GetSQLCreateTableScriptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var openFileOrFolderPath = this.OpenFileOrFolderPath;
            if (openFileOrFolderPath?.EndsWith("/") == true)
            {
                //trim trailing slash '/'
                openFileOrFolderPath = openFileOrFolderPath[..^1];
            }

            string tableName = Path.GetFileNameWithoutExtension(openFileOrFolderPath) ?? DEFAULT_TABLE_NAME;
            if (this.mainDataSource?.Columns.Count > 0)
            {
                var dataset = new DataSet();

                this.mainDataSource.TableName = tableName;
                dataset.Tables.Add(this.mainDataSource);

                var scriptAdapter = new CustomScriptBasedSchemaAdapter();
                string sql = scriptAdapter.GetSchemaScript(dataset, false);

                dataset.Tables.Remove(this.mainDataSource); //If we don't remove it, we can get errors in rare cases

                Clipboard.SetText(sql);
                MenuBarClickEvent.FireAndForget(MenuBarClickEvent.ActionId.SQLCreateTable);
                MessageBox.Show(this, "Create table script copied to clipboard!", "Parquet Viewer", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
                MessageBox.Show(this, "Please select some fields first to get the SQL script", "Parquet Viewer", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void MetadataViewerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (IsAnyFileOpen)
            {
                MenuBarClickEvent.FireAndForget(MenuBarClickEvent.ActionId.MetadataViewer);
                using var metadataViewer = new MetadataViewer(this._openParquetEngine!);
                metadataViewer.ShowDialog(this);
            }
        }

        private void changeColumnSizingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem tsi && tsi.Tag != null
                && Enum.TryParse(tsi.Tag.ToString(), out AutoSizeColumnsMode columnSizingMode)
                && AppSettings.AutoSizeColumnsMode != columnSizingMode)
            {
                AppSettings.AutoSizeColumnsMode = columnSizingMode;
                foreach (ToolStripMenuItem toolStripItem in tsi.GetCurrentParent()!.Items)
                {
                    toolStripItem.Checked = toolStripItem.Tag?.Equals(tsi.Tag) == true;
                }
                this.mainGridView.AutoSizeColumns();

                //Also clear out each column's Tag so auto sizing can pick it up again (see: FastAutoSizeColumns())
                foreach (DataGridViewColumn column in this.mainGridView.Columns)
                {
                    column.Tag = null; //TODO: This logic is terrible. Need to find a cleaner solution
                }
            }
        }

        private void alwaysLoadAllRecordsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.alwaysLoadAllRecordsToolStripMenuItem.Checked = !this.alwaysLoadAllRecordsToolStripMenuItem.Checked;
            AppSettings.AlwaysLoadAllRecords = this.alwaysLoadAllRecordsToolStripMenuItem.Checked;
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MenuBarClickEvent.FireAndForget(MenuBarClickEvent.ActionId.AboutBox);
            (new AboutBox()).ShowDialog(this);
        }

        private void userGuideToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MenuBarClickEvent.FireAndForget(MenuBarClickEvent.ActionId.UserGuide);
            Process.Start(new ProcessStartInfo(Constants.WikiURL) { UseShellExecute = true });
        }

        private void DateFormatMenuItem_Click(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem item && item.Tag is string tag)
            {
                var selectedDateFormat = (DateFormat)int.Parse(tag);
                if (selectedDateFormat != DateFormat.Custom)
                {
                    AppSettings.DateTimeDisplayFormat = selectedDateFormat;
                    this.RefreshDateFormatMenuItemSelection();
                    this.mainGridView.UpdateDateFormats();
                    this.mainGridView.Refresh();
                }
                else
                {
                    new CustomDateFormatInputForm().ShowDialog(this);
                }
            }
        }

        private void shareAnonymousUsageDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.shareAnonymousUsageDataToolStripMenuItem.Checked = !this.shareAnonymousUsageDataToolStripMenuItem.Checked;
            AppSettings.AnalyticsDataGatheringConsent = this.shareAnonymousUsageDataToolStripMenuItem.Checked;
            AppSettings.ConsentLastAskedOnVersion = AboutBox.AssemblyVersion;
        }
    }
}
