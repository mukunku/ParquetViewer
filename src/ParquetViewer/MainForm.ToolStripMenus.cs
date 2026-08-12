using ParquetViewer.Analytics;
using ParquetViewer.Helpers;
using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ParquetViewer;

public partial class MainForm
{
    private const string DEFAULT_TABLE_NAME = "MY_TABLE";

    private string? _getSqlCreateTableScriptToolStripMenuItem_ToolTipOriginalText;

    private void newToolStripMenuItem_Click(object sender, EventArgs e)
    {
        MenuBarClickEvent.FireAndForget(MenuBarClickEvent.ActionId.FileNew);
        OpenFileOrFolderPath = null;
    }

    private async void openToolStripMenuItem_Click(object sender, EventArgs e)
    {
        try
        {
            if (openParquetFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                MenuBarClickEvent.FireAndForget(MenuBarClickEvent.ActionId.FileOpen);
                await OpenNewFileOrFolder(openParquetFileDialog.FileName);
            }
        }
        catch
        {
            OpenFileOrFolderPath = null;
            throw;
        }
    }

    private async void openFolderToolStripMenuItem_Click(object sender, EventArgs e)
    {
        try
        {
            if (openFolderDialog.ShowDialog(this) == DialogResult.OK)
            {
                MenuBarClickEvent.FireAndForget(MenuBarClickEvent.ActionId.FolderOpen);
                await OpenNewFileOrFolder(openFolderDialog.SelectedPath);
            }
        }
        catch
        {
            OpenFileOrFolderPath = null;
            throw;
        }
    }

    private void saveAsToolStripMenuItem_Click(object sender, EventArgs e) => ExportResults(default);

    private async void exitToolStripMenuItem_Click(object sender, EventArgs e)
    {
        var exitEventTask = new MenuBarClickEvent { Action = MenuBarClickEvent.ActionId.Exit }.Record();
        await Task.WhenAny(exitEventTask, Task.Delay(3000)); //don't prevent the app from closing for too long
        Close();
    }

    private async void changeFieldsMenuStripButton_Click(object sender, EventArgs e)
    {
        MenuBarClickEvent.FireAndForget(MenuBarClickEvent.ActionId.ChangeFields);
        var fieldList = await OpenFieldSelectionDialog(true);
        if (fieldList is not null)
            SelectedFields = fieldList; //triggers a file load
    }

    private void GetSQLCreateTableScriptToolStripMenuItem_Click(object sender, EventArgs e)
    {
        var openFileOrFolderPath = OpenFileOrFolderPath;
        if (openFileOrFolderPath?.EndsWith('/') == true)
        {
            //trim trailing slash '/'
            openFileOrFolderPath = openFileOrFolderPath[..^1];
        }

        string tableName = Path.GetFileNameWithoutExtension(openFileOrFolderPath) ?? DEFAULT_TABLE_NAME;
        if (_mainDataSource?.Columns.Count > 0)
        {
            var dataset = new DataSet();

            _mainDataSource.TableName = tableName;
            dataset.Tables.Add(_mainDataSource);

            var scriptAdapter = new CustomScriptBasedSchemaAdapter();
            string sql = scriptAdapter.GetSchemaScript(dataset, false);

            dataset.Tables.Remove(_mainDataSource); //If we don't remove it, we can get errors in rare cases

            MenuBarClickEvent.FireAndForget(MenuBarClickEvent.ActionId.SQLCreateTable);
            Clipboard.SetText(sql);
            MessageBox.Show(this, Resources.Strings.CreateTableScriptCopiedToClipboardMessage, "ParquetViewer", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        else
            MessageBox.Show(this, Resources.Strings.CreateTableScriptFailedWithNoFieldsMessage, "ParquetViewer", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    private void MetadataViewerToolStripMenuItem_Click(object sender, EventArgs e)
    {
        if (IsAnyFileOpen)
        {
            MenuBarClickEvent.FireAndForget(MenuBarClickEvent.ActionId.MetadataViewer);
            using var metadataViewer = new MetadataViewer(_openParquetEngine!);
            metadataViewer.ShowDialog(this);
        }
    }

    private void alwaysLoadAllRecordsToolStripMenuItem_Click(object sender, EventArgs e)
    {
        alwaysLoadAllRecordsToolStripMenuItem.Checked = !alwaysLoadAllRecordsToolStripMenuItem.Checked;
        AppSettings.AlwaysLoadAllRecords = alwaysLoadAllRecordsToolStripMenuItem.Checked;
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

            RefreshDateFormatMenuItemSelection();
            mainGridView.UpdateDateFormats();
            mainGridView.Refresh();
        }
    }

    private void shareAnonymousUsageDataToolStripMenuItem_Click(object sender, EventArgs e)
    {
        shareAnonymousUsageDataToolStripMenuItem.Checked = !shareAnonymousUsageDataToolStripMenuItem.Checked;
        AppSettings.AnalyticsDataGatheringConsent = shareAnonymousUsageDataToolStripMenuItem.Checked;
        AppSettings.ConsentLastAskedOnVersion = Env.AssemblyVersion;
    }

    private void shareAnonymousUsageDataToolStripMenuItem_CheckedChanged(object sender, System.EventArgs e)
    {
        RefreshExperimentalFeatureToolStrips();
    }

    private void RefreshExperimentalFeatureToolStrips()
    {
        foreach (ToolStripDropDownItem dropdownItem in shareAnonymousUsageDataToolStripMenuItem.DropDownItems)
        {
            if (dropdownItem is ToolStripMenuItem toolstrip && toolstrip.Checked)
            {
                //If someone has an experimental feature enabled, don't hide the checkbox so they can disable it if they want.
                dropdownItem.Visible = true;
                continue;
            }

            //Expose experimental features to folks willing to share usage stats.
            //Also better this way if we end up killing the beta feature.
            dropdownItem.Visible = shareAnonymousUsageDataToolStripMenuItem.Checked;
        }
    }

    private void GetSQLCreateTableScriptToolStripMenuItem_MouseEnter(object sender, System.EventArgs e)
    {
        _getSqlCreateTableScriptToolStripMenuItem_ToolTipOriginalText ??= getSQLCreateTableScriptToolStripMenuItem.ToolTipText;
        var firstColumn = MainDataSource?.Columns.AsEnumerable().FirstOrDefault();
        if (firstColumn is null || OpenFileOrFolderPath is null)
        {
            ResetGetSQLCreateTableScriptToolStripMenuItemToolTipText();
            return;
        }

        //Adjust the tooltip dynamically to be fancy
        try
        {
            string tableName = Path.GetFileNameWithoutExtension(OpenFileOrFolderPath) ?? DEFAULT_TABLE_NAME;
            string sqlTypeDefinition = CustomScriptBasedSchemaAdapter.GetTypeFor(firstColumn);

            var truncateSuffix = MainDataSource?.Columns.Count > 1 ? ",..." : ")";
            getSQLCreateTableScriptToolStripMenuItem.ToolTipText = $"CREATE TABLE [{tableName.Left(40, "...")}] ([{firstColumn}] {sqlTypeDefinition}{truncateSuffix}";
        }
        catch
        {
            ResetGetSQLCreateTableScriptToolStripMenuItemToolTipText();
        }
    }

    private void ResetGetSQLCreateTableScriptToolStripMenuItemToolTipText()
    {
        if (_getSqlCreateTableScriptToolStripMenuItem_ToolTipOriginalText is not null)
            getSQLCreateTableScriptToolStripMenuItem.ToolTipText = _getSqlCreateTableScriptToolStripMenuItem_ToolTipOriginalText;
    }

    private void darkModeToolStripMenuItem_Click(object sender, EventArgs e)
    {
        darkModeToolStripMenuItem.Checked = !darkModeToolStripMenuItem.Checked;
        AppSettings.DarkMode = darkModeToolStripMenuItem.Checked; // Will trigger SetTheme()
        RefreshExperimentalFeatureToolStrips();
    }
}