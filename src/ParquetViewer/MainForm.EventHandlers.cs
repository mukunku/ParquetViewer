using ParquetViewer.Analytics;
using ParquetViewer.Engine.Types;
using ParquetViewer.Exceptions;
using ParquetViewer.Properties;
using System;
using System.Diagnostics;
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
            MessageBox.Show(@"NULL CHECK: 
    WHERE field_name IS NULL
    WHERE field_name IS NOT NULL
DATETIME:   
    WHERE field_name >= #2000/12/31#
    WHERE field_name = #12/31/2000#
NUMERIC:
    WHERE field_name <= 123.4
    WHERE (field1 * field2) / 100 > 0.1
STRING:
    WHERE field_name LIKE '%value%' 
    WHERE field_name = 'equals value'
    WHERE field_name <> 'not equals'
MULTIPLE CONDITIONS: 
    WHERE (field_1 > #2000/12/31# AND field_1 < #2001/12/31#) OR field_2 <> 100

Checkout 'Help → User Guide' for more information.", "Filtering Query Syntax Examples");
        }

        private void mainGridView_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            this.actualShownRecordCountLabel.Text = this.mainGridView.RowCount.ToString();
        }

        private void showingStatusBarLabel_Click(object sender, EventArgs e)
        {
            //This is just here in case I want to add debug info
        }

        private void MainGridView_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            //Ignore errors and hope for the best.
            e.Cancel = true;
        }

        private void searchFilterTextBox_Enter(object sender, EventArgs e)
        {
            if (sender is TextBox searchBox)
            {
                if (!searchBox.Text.StartsWith("WHERE", StringComparison.InvariantCultureIgnoreCase))
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
                if (loadAllRecordsButton.Enabled)
                {
                    loadAllRecordsButton.Image = Resources.next_blue;
                }
                else
                {
                    loadAllRecordsButton.Image = Resources.next_disabled;
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
                if (!this.IsAnyFileOpen)
                    return;

                if (this.MainDataSource is null)
                    throw new ApplicationException("This should never happen");

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
                }
            }
            catch (InvalidQueryException ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + Environment.NewLine + ex.InnerException?.Message,
                    "Invalid Query", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void clearFilterButton_Click(object sender, EventArgs? e)
        {
            if (this.MainDataSource is not null)
            {
                this.MainDataSource.DefaultView.RowFilter = null;
            }
        }
    }
}
