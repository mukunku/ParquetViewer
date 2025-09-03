using ParquetViewer.Analytics;
using ParquetViewer.Helpers;
using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace ParquetViewer
{
    public partial class MainForm
    {
        private const string DEFAULT_TABLE_NAME = "MY_TABLE";

        private string? _getSqlCreateTableScriptToolStripMenuItem_ToolTipOriginalText;

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
            if (openFileOrFolderPath?.EndsWith('/') == true)
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
                MessageBox.Show(this, "Create table script copied to clipboard!", "ParquetViewer", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
                MessageBox.Show(this, "Please select some fields first to get the SQL script", "ParquetViewer", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private void alwaysLoadAllRecordsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.alwaysLoadAllRecordsToolStripMenuItem.Checked = !this.alwaysLoadAllRecordsToolStripMenuItem.Checked;
            AppSettings.AlwaysLoadAllRecords = this.alwaysLoadAllRecordsToolStripMenuItem.Checked;
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MenuBarClickEvent.FireAndForget(MenuBarClickEvent.ActionId.AboutBox);
            using var aboutForm = new AboutBox();
            aboutForm.ShowDialog(this);
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
                }
                else
                {
                    string? customDateFormat = null;
                    if (AppSettings.DateTimeDisplayFormat == DateFormat.Custom)
                    {
                        customDateFormat = AppSettings.CustomDateFormat;
                    }

                    using var customDateFormatInputForm = new CustomDateFormatInputForm(customDateFormat);
                    if (customDateFormatInputForm.ShowDialog(this) == DialogResult.OK)
                    {
                        AppSettings.DateTimeDisplayFormat = DateFormat.Custom;
                        AppSettings.CustomDateFormat = customDateFormatInputForm.UserEnteredDateFormat;
                    }
                }

                this.RefreshDateFormatMenuItemSelection();
                this.mainGridView.UpdateDateFormats();
                this.mainGridView.Refresh();
            }
        }

        private void shareAnonymousUsageDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.shareAnonymousUsageDataToolStripMenuItem.Checked = !this.shareAnonymousUsageDataToolStripMenuItem.Checked;
            AppSettings.AnalyticsDataGatheringConsent = this.shareAnonymousUsageDataToolStripMenuItem.Checked;
            AppSettings.ConsentLastAskedOnVersion = Env.AssemblyVersion;
        }

        private void shareAnonymousUsageDataToolStripMenuItem_CheckedChanged(object sender, System.EventArgs e)
        {
            RefreshExperimentalFeatureToolStrips();
        }

        private void RefreshExperimentalFeatureToolStrips()
        {
            foreach (ToolStripDropDownItem dropdownItem in this.shareAnonymousUsageDataToolStripMenuItem.DropDownItems)
            {
                if (dropdownItem is ToolStripMenuItem toolstrip && toolstrip.Checked)
                {
                    //If someone has an experimental feature enabled, don't hide the checkbox so they can disable it if they want.
                    dropdownItem.Visible = true; 
                    continue;
                }

                //Expose experimental features to folks willing to share usage stats.
                //Also better this way if we end up killing the beta feature.
                dropdownItem.Visible = this.shareAnonymousUsageDataToolStripMenuItem.Checked;
            }
        }

        private void GetSQLCreateTableScriptToolStripMenuItem_MouseEnter(object sender, System.EventArgs e)
        {
            _getSqlCreateTableScriptToolStripMenuItem_ToolTipOriginalText ??= this.getSQLCreateTableScriptToolStripMenuItem.ToolTipText;
            var firstColumn = this.MainDataSource?.Columns.AsEnumerable().FirstOrDefault();
            if (firstColumn is null || this.OpenFileOrFolderPath is null)
            {
                ResetGetSQLCreateTableScriptToolStripMenuItemToolTipText();
                return;
            }

            //Adjust the tooltip dynamically to be fancy
            try
            {
                string tableName = Path.GetFileNameWithoutExtension(this.OpenFileOrFolderPath) ?? DEFAULT_TABLE_NAME;
                string sqlTypeDefinition = CustomScriptBasedSchemaAdapter.GetTypeFor(firstColumn);

                var truncateSuffix = this.MainDataSource?.Columns.Count > 1 ? ",..." : ")";
                this.getSQLCreateTableScriptToolStripMenuItem.ToolTipText = $"CREATE TABLE [{tableName.Left(40, "...")}] ([{firstColumn}] {sqlTypeDefinition}{truncateSuffix}";
            }
            catch
            {
                ResetGetSQLCreateTableScriptToolStripMenuItemToolTipText();
            }
        }

        private void ResetGetSQLCreateTableScriptToolStripMenuItemToolTipText()
        {
            if (this._getSqlCreateTableScriptToolStripMenuItem_ToolTipOriginalText is not null)
                this.getSQLCreateTableScriptToolStripMenuItem.ToolTipText = this._getSqlCreateTableScriptToolStripMenuItem_ToolTipOriginalText;
        }

        private void darkModeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.darkModeToolStripMenuItem.Checked = !this.darkModeToolStripMenuItem.Checked;
            AppSettings.DarkMode = this.darkModeToolStripMenuItem.Checked; // Will trigger SetTheme()
            RefreshExperimentalFeatureToolStrips();
        }
    }
}
