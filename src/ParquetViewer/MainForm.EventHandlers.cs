using ParquetViewer.Analytics;
using ParquetViewer.Engine;
using ParquetViewer.Engine.Types;
using ParquetViewer.Exceptions;
using ParquetViewer.Helpers;
using System;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ParquetViewer;

public partial class MainForm
{
    [GeneratedRegex("^WHERE ")]
    private static partial Regex QueryUselessPartRegex();

    private int _failedFileIntegrityCheckCount;

    private void OffsetTextBox_KeyPress(object sender, KeyPressEventArgs e)
    {
        if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
        {
            e.Handled = true;
        }
    }

    private void RecordsToTextBox_KeyPress(object sender, KeyPressEventArgs e)
    {
        if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
        {
            e.Handled = true;
        }
    }

    private void OffsetTextBox_TextChanged(object sender, EventArgs e)
    {
        var textbox = (TextBox)sender;
        if (int.TryParse(textbox.Text, out var offset))
            CurrentOffset = offset;
        else
            textbox.Text = CurrentOffset.ToString();
    }

    private void RecordsToTextBox_TextChanged(object sender, EventArgs? e)
    {
        var textbox = (TextBox)sender;
        if (int.TryParse(textbox.Text, out var recordCount) && recordCount > 0)
            CurrentMaxRowCount = recordCount;
        else
            textbox.Text = CurrentMaxRowCount.ToString();
    }

    private void SearchFilterTextBox_KeyPress(object sender, KeyPressEventArgs e)
    {
        if (e.KeyChar == Convert.ToChar(Keys.Return))
        {
            RunQueryButton_Click(runQueryButton, null);
        }
        else if (e.KeyChar == Convert.ToChar(Keys.Escape))
        {
            ClearFilterButton_Click(clearFilterButton, null);
        }
    }

    private async void MainForm_DragDrop(object sender, DragEventArgs e)
    {
        try
        {
            var files = e.Data?.GetData(DataFormats.FileDrop) as string[];
            if (files?.Length > 0)
            {
                MenuBarClickEvent.FireAndForget(MenuBarClickEvent.ActionId.DragDrop);
                await OpenNewFileOrFolder(files[0]);
            }
        }
        catch
        {
            OpenFileOrFolderPath = null;
            throw;
        }
    }

    private void MainForm_DragEnter(object sender, DragEventArgs e)
    {
        if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true)
            e.Effect = DragDropEffects.Copy;
    }

    private void SearchFilterLabel_Click(object sender, EventArgs e)
    {
        MessageBox.Show(Resources.Strings.QuerySyntaxHelpText, Resources.Strings.QuerySyntaxHelpTitle);
    }

    private void MainGridView_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
    {
        actualShownRecordCountLabel.Text = mainGridView.RowCount.ToString();
    }

    private void ShowingStatusBarLabel_Click(object sender, EventArgs e)
    {
        //This is just here in case I want to add debug info
    }

    private void SearchFilterTextBox_Enter(object sender, EventArgs e)
    {
        if (sender is TextBox searchBox)
        {
            if (string.IsNullOrWhiteSpace(searchBox.Text))
            {
                searchBox.Text = "WHERE ";
            }
        }
    }

    private void SearchFilterTextBox_Leave(object sender, EventArgs e)
    {
        if (sender is TextBox searchBox)
        {
            if (searchBox.Text.Trim().Equals("WHERE", StringComparison.OrdinalIgnoreCase))
            {
                searchBox.Text = string.Empty; //show the placeholder
            }
        }
    }

    private void LoadAllRowsButton_EnabledChanged(object sender, EventArgs e)
    {
        if (sender is Button loadAllRecordsButton)
        {
            loadAllRecordsButton.FlatAppearance.MouseOverBackColor = Color.Transparent;
            loadAllRecordsButton.FlatAppearance.MouseDownBackColor = Color.Transparent;

            if (loadAllRecordsButton.Enabled)
            {
                loadAllRecordsButton.Image = Resources.Icons.next_blue;
            }
            else
            {
                loadAllRecordsButton.Image = Resources.Icons.next_disabled;
            }
        }
    }

    private void LoadAllRowsButton_Click(object? sender, EventArgs? e)
    {
        if (_openParquetEngine is not null)
        {
            //Force file reload to happen instantly by triggering the event handler ourselves
            recordCountTextBox.SetTextQuiet(_openParquetEngine.RecordCount.ToString());
            RecordsToTextBox_TextChanged(recordCountTextBox, null);
            MenuBarClickEvent.FireAndForget(MenuBarClickEvent.ActionId.LoadAllRows);
        }
    }

    private void MainForm_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Control && e.KeyCode == Keys.E && loadAllRowsButton.Enabled)
        {
            LoadAllRowsButton_Click(null, null);
        }
        else if (e.Control && e.KeyCode == Keys.R && _openParquetEngine is not null) //Reload shortcut
        {
            LoadFileToGridview();
        }
    }

    private void RunQueryButton_Click(object sender, EventArgs? e)
    {
        try
        {
            if (!IsAnyFileOpen || MainDataSource is null)
                return;

            string queryText = searchFilterTextBox.Text ?? string.Empty;
            queryText = QueryUselessPartRegex().Replace(queryText, string.Empty).Trim();

            //Treat list, map, and struct types as strings by casting them automatically
            foreach (var complexField in mainGridView.Columns.OfType<DataGridViewColumn>()
                .Where(c => c.ValueType.ImplementsInterface<IListValue>() || c.ValueType.ImplementsInterface<IMapValue>()
                    || c.ValueType.ImplementsInterface<IStructValue>() || c.ValueType.ImplementsInterface<IByteArrayValue>())
                .Select(c => c.Name))
            {
                //This isn't perfect but it should handle most cases
                queryText = queryText.Replace(complexField, $"CONVERT({complexField}, System.String)", StringComparison.InvariantCultureIgnoreCase);
            }

            if (string.IsNullOrWhiteSpace(queryText)
                || MainDataSource.DefaultView.RowFilter == queryText) //No need to execute the same query again
            {
                return;
            }

            var stopwatch = Stopwatch.StartNew();
            var queryEvent = new ExecuteQueryEvent
            {
                RecordCountTotal = MainDataSource.Rows.Count,
                ColumnCount = MainDataSource.Columns.Count
            };

            try
            {
                Cursor = Cursors.WaitCursor;
                MainDataSource.DefaultView.RowFilter = queryText;
                queryEvent.IsValid = true;
                queryEvent.RecordCountFiltered = MainDataSource.DefaultView.Count;
            }
            catch (Exception ex)
            {
                MainDataSource.DefaultView.RowFilter = null;
                throw new InvalidQueryException(ex);
            }
            finally
            {
                Cursor = Cursors.Default;
                queryEvent.RunTimeMS = stopwatch.ElapsedMilliseconds;
                var _ = queryEvent.Record(); //Fire and forget
                actualShownRecordCountLabel.Text = MainDataSource.DefaultView.Count.ToString();
            }
        }
        catch (InvalidQueryException ex)
        {
            MessageBox.Show(ex.Message + Environment.NewLine + Environment.NewLine + ex.InnerException?.Message,
                Resources.Errors.InvalidQueryErrorTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        catch (Exception)
        {
            throw;
        }
    }

    private void ClearFilterButton_Click(object sender, EventArgs? e)
    {
        if (!string.IsNullOrEmpty(MainDataSource?.DefaultView.RowFilter))
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                MainDataSource.DefaultView.RowFilter = null;
            }
            finally
            {
                Cursor = Cursors.Default;
                actualShownRecordCountLabel.Text = MainDataSource.DefaultView.Count.ToString();

            }
        }
    }

    private void MainForm_Resize(object sender, EventArgs e)
    {
        if (WindowState == FormWindowState.Minimized)
        {
            //Hide context menu on minimize to avoid a glitch where
            //the context menu won't go away until you click on it.
            mainGridView.CloseContextMenu();
        }
    }

    private void LanguageToolStripMenuItem_Click(object sender, EventArgs e)
    {
        if (sender is not ToolStripItem toolStripItem)
        {
            return;
        }

        var targetCulture = toolStripItem.Tag?.ToString();
        if (string.IsNullOrWhiteSpace(targetCulture))
        {
            targetCulture = "en-US"; //our default culture
        }

        if (!UtilityMethods.TryParseCultureInfo(targetCulture, out CultureInfo? newCultureInfo))
        {
            return; //invalid culture
        }

        if (newCultureInfo.Equals(CultureInfo.CurrentUICulture))
        {
            return; //no change
        }

        if (MessageBox.Show(this,
            Resources.Strings.LanguageChangeConfirmationMessage,
            Resources.Strings.LanguageChangeConfirmationTitle,
            MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
        {
            return; //user cancelled
        }

        AppSettings.UserSelectedCulture = newCultureInfo;
        UtilityMethods.RestartApplication();
    }

    /// <remarks>Originally I implemented a FileSystemWatcher but it seems network drives are not reliable with that.
    /// Not sure how common that is but this implementation without it is simpler and I'm hoping not too IO intensive</remarks>
    private async void FileIntegrityCheckingTimer_Tick(object sender, EventArgs e)
    {
        if (OpenFileOrFolderPath is null || _openParquetEngine is null)
            return; //no file open

        fileIntegrityCheckingTimer.Stop();
        try
        {
            var fileDeletedSuffix = $" ({Resources.Strings.OpenFileNoLongerExistsTitleSuffix})";
            var fileModifiedSuffix = $" ({Resources.Strings.OpenFileWasModifiedTitleSuffix})";

            if (_originalModifiedInfo is null)
            {
                ResetTitle();
            }

            var alreadyHasDeletedSuffix = Text.EndsWith(fileDeletedSuffix);

            //Perform file system checks in a background thread avoid blocking the UI thread.
            //Only really relevant when opening a folder with many files on a network drive.
            var engineSnapshot = _openParquetEngine;
            var lastModifiedInfo = await Task.Run(() => TryGetLastModifiedInfo(engineSnapshot, OpenFileOrFolderPath));
            if (!ReferenceEquals(engineSnapshot, _openParquetEngine))
                return; //the user has opened a different file/folder while we were checking the file system, so ignore this result

            if (lastModifiedInfo is null && !alreadyHasDeletedSuffix)
            {
                ResetTitle();
                //File or folder no longer exists. In this case let's not mark this timer as handled
                //and let it keep running in case the file/folder is restored later.
                Text += fileDeletedSuffix;
                return;
            }
            else if (lastModifiedInfo is not null && alreadyHasDeletedSuffix)
            {
                ResetTitle();
            }

            if (lastModifiedInfo is not null)
            {
                if (_originalModifiedInfo is null)
                {
                    _originalModifiedInfo = lastModifiedInfo;
                }
                else if (_originalModifiedInfo != lastModifiedInfo && !Text.EndsWith(fileModifiedSuffix))
                {
                    ResetTitle();
                    Text += fileModifiedSuffix;
                }
            }

            _failedFileIntegrityCheckCount = 0;

            void ResetTitle()
            {
                if (Text.EndsWith(fileModifiedSuffix))
                    Text = Text.Replace(fileModifiedSuffix, string.Empty);
                else if (Text.EndsWith(fileDeletedSuffix))
                    Text = Text.Replace(fileDeletedSuffix, string.Empty);
            }
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
        {
            //Swallow expected exceptions to not overload the user with error message dialogs
        }
        catch (Exception ex)
        {
            //Swallow to not overload the user with error message dialogs but log it as this is unexpected.
            //Also make sure we don't spam exception events for repeated failures.
            if (++_failedFileIntegrityCheckCount == 1)
            {
                ExceptionEvent.FireAndForget(ex);
            }
        }
        finally
        {
            fileIntegrityCheckingTimer.Start();
        }

        //Returns the last modified date and size of the open file, or the most recent last modified
        //date and total combined size of all open files in the folder.
        static (DateTime LastModifiedUtc, long Length)? TryGetLastModifiedInfo(IParquetEngine engine, string openFileOrFolderPath)
        {
            if (engine is null)
            {
                return null; //no open file;
            }

            DateTime latest = Directory.Exists(openFileOrFolderPath) ? Directory.GetCreationTimeUtc(openFileOrFolderPath) : DateTime.MinValue;
            long totalLength = 0;
            bool foundAny = false;
            var counter = 0;

            foreach (var filePath in engine.GetOpenParquetFilePaths())
            {
                if (counter >= 250)
                {
                    //We don't want to check too many files in case the user has a folder with a lot of files open.
                    //This is a safeguard against performance issues.
                    break;
                }

                var info = new FileInfo(filePath);
                if (!info.Exists)
                {
                    return null; //file was deleted
                }

                //There's a chance the file could be deleted between the time we check for existence above and when we access .Length and .LastWriteTimeUtc below.
                //This is fine as the caller has a try-catch block that catches IOException types.
                totalLength += info.Length;

                if (!foundAny || info.LastWriteTimeUtc > latest)
                {
                    latest = info.LastWriteTimeUtc;
                    foundAny = true;
                }

                counter++;
            }

            return (latest, totalLength);
        }
    }
}