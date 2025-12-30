using ParquetViewer.Analytics;
using ParquetViewer.Engine.Types;
using ParquetViewer.Exceptions;
using ParquetViewer.Helpers;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace ParquetViewer
{
    public partial class MainForm
    {
        [GeneratedRegex("^WHERE ")]
        private static partial Regex QueryUselessPartRegex();

        private void offsetTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void recordsToTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void offsetTextBox_TextChanged(object sender, EventArgs e)
        {
            var textbox = (TextBox)sender;
            if (int.TryParse(textbox.Text, out var offset))
                this.CurrentOffset = offset;
            else
                textbox.Text = this.CurrentOffset.ToString();
        }

        private void recordsToTextBox_TextChanged(object sender, EventArgs? e)
        {
            var textbox = (TextBox)sender;
            if (int.TryParse(textbox.Text, out var recordCount))
                this.CurrentMaxRowCount = recordCount;
            else
                textbox.Text = this.CurrentMaxRowCount.ToString();
        }

        private void searchFilterTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == Convert.ToChar(Keys.Return))
            {
                this.runQueryButton_Click(this.runQueryButton, null);
            }
            else if (e.KeyChar == Convert.ToChar(Keys.Escape))
            {
                this.clearFilterButton_Click(this.clearFilterButton, null);
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
                    await this.OpenNewFileOrFolder(files[0]);
                }
            }
            catch
            {
                this.OpenFileOrFolderPath = null;
                throw;
            }
        }

        private void MainForm_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true)
                e.Effect = DragDropEffects.Copy;
        }

        private void searchFilterLabel_Click(object sender, EventArgs e)
        {
            MessageBox.Show(Resources.Strings.QuerySyntaxHelpText, Resources.Strings.QuerySyntaxHelpTitle);
        }

        private void mainGridView_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            this.actualShownRecordCountLabel.Text = this.mainGridView.RowCount.ToString();
        }

        private void showingStatusBarLabel_Click(object sender, EventArgs e)
        {
            //This is just here in case I want to add debug info
        }

        private void searchFilterTextBox_Enter(object sender, EventArgs e)
        {
            if (sender is TextBox searchBox)
            {
                if (string.IsNullOrWhiteSpace(searchBox.Text))
                {
                    searchBox.Text = "WHERE ";
                }
            }
        }

        private void searchFilterTextBox_Leave(object sender, EventArgs e)
        {
            if (sender is TextBox searchBox)
            {
                if (searchBox.Text.Trim().Equals("WHERE", StringComparison.OrdinalIgnoreCase))
                {
                    searchBox.Text = string.Empty; //show the placeholder
                }
            }
        }

        private void loadAllRowsButton_EnabledChanged(object sender, EventArgs e)
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

        private void loadAllRowsButton_Click(object? sender, EventArgs? e)
        {
            if (this._openParquetEngine is not null)
            {
                //Force file reload to happen instantly by triggering the event handler ourselves
                this.recordCountTextBox.SetTextQuiet(this._openParquetEngine.RecordCount.ToString());
                this.recordsToTextBox_TextChanged(this.recordCountTextBox, null);
                MenuBarClickEvent.FireAndForget(MenuBarClickEvent.ActionId.LoadAllRows);
            }
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.E && this.loadAllRowsButton.Enabled)
            {
                this.loadAllRowsButton_Click(null, null);
            }
        }

        private void runQueryButton_Click(object sender, EventArgs? e)
        {
            try
            {
                if (!this.IsAnyFileOpen || this.MainDataSource is null)
                    return;

                string queryText = this.searchFilterTextBox.Text ?? string.Empty;
                queryText = QueryUselessPartRegex().Replace(queryText, string.Empty).Trim();

                //Treat list, map, and struct types as strings by casting them automatically
                foreach (var complexField in this.mainGridView.Columns.OfType<DataGridViewColumn>()
                    .Where(c => c.ValueType == typeof(ListValue) || c.ValueType == typeof(MapValue)
                        || c.ValueType == typeof(StructValue) || c.ValueType == typeof(ByteArrayValue))
                    .Select(c => c.Name))
                {
                    //This isn't perfect but it should handle most cases
                    queryText = queryText.Replace(complexField, $"CONVERT({complexField}, System.String)", StringComparison.InvariantCultureIgnoreCase);
                }

                if (string.IsNullOrWhiteSpace(queryText)
                    || this.MainDataSource.DefaultView.RowFilter == queryText) //No need to execute the same query again
                {
                    return;
                }

                var stopwatch = Stopwatch.StartNew();
                var queryEvent = new ExecuteQueryEvent
                {
                    RecordCountTotal = this.MainDataSource.Rows.Count,
                    ColumnCount = this.MainDataSource.Columns.Count
                };

                try
                {
                    this.Cursor = Cursors.WaitCursor;
                    this.MainDataSource.DefaultView.RowFilter = queryText;
                    queryEvent.IsValid = true;
                    queryEvent.RecordCountFiltered = this.MainDataSource.DefaultView.Count;
                }
                catch (Exception ex)
                {
                    this.MainDataSource.DefaultView.RowFilter = null;
                    throw new InvalidQueryException(ex);
                }
                finally
                {
                    this.Cursor = Cursors.Default;
                    queryEvent.RunTimeMS = stopwatch.ElapsedMilliseconds;
                    var _ = queryEvent.Record(); //Fire and forget
                    this.actualShownRecordCountLabel.Text = this.MainDataSource.DefaultView.Count.ToString();
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

        private void clearFilterButton_Click(object sender, EventArgs? e)
        {
            if (!string.IsNullOrEmpty(this.MainDataSource?.DefaultView.RowFilter))
            {
                try
                {
                    this.Cursor = Cursors.WaitCursor;
                    this.MainDataSource.DefaultView.RowFilter = null;
                }
                finally
                {
                    this.Cursor = Cursors.Default;
                    this.actualShownRecordCountLabel.Text = this.MainDataSource.DefaultView.Count.ToString();

                }
            }
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                //Hide context menu on minimize to avoid a glitch where
                //the context menu won't go away until you click on it.
                this.mainGridView.CloseContextMenu();
            }
        }

        private void languageToolStripMenuItem_Click(object sender, EventArgs e)
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
    }
}
