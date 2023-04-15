using ParquetViewer.Engine;
using ParquetViewer.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace ParquetViewer
{
    public partial class MainForm
    {
        private const string QueryUselessPartRegex = "^WHERE ";

        private ToolTip dateOnlyFormatWarningToolTip = new();
        private Dictionary<(int, int), QuickPeekForm> openQuickPeekForms = new();

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
            var textbox = sender as TextBox;
            if (int.TryParse(textbox.Text, out var offset))
                this.CurrentOffset = offset;
            else
                textbox.Text = this.CurrentOffset.ToString();
        }

        private void recordsToTextBox_TextChanged(object sender, EventArgs e)
        {
            var textbox = sender as TextBox;
            if (int.TryParse(textbox.Text, out var recordCount))
                this.CurrentMaxRowCount = recordCount;
            else
                textbox.Text = this.CurrentMaxRowCount.ToString();
        }

        private void MainGridView_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            //Add warnings to date field headers if the user is using a "Date Only" date format.
            //We want to be helpful so people don't accidentally leave a date only format on and think they are missing time information in their data.
            if (e.RowIndex == -1 && e.ColumnIndex >= 0)
            {
                bool isDateTimeCell = ((DataGridView)sender).Columns[e.ColumnIndex].ValueType == typeof(DateTime);
                bool isUserUsingDateOnlyFormat = AppSettings.DateTimeDisplayFormat.IsDateOnlyFormat();
                if (isDateTimeCell && isUserUsingDateOnlyFormat)
                {
                    var img = Properties.Resources.exclamation_icon_yellow;
                    Rectangle r1 = new Rectangle(e.CellBounds.Left + e.CellBounds.Width - img.Width, 4, img.Width, img.Height);
                    Rectangle r2 = new Rectangle(0, 0, img.Width, img.Height);
                    string header = ((DataGridView)sender).Columns[e.ColumnIndex].HeaderText;
                    e.PaintBackground(e.CellBounds, true);
                    e.PaintContent(e.CellBounds);
                    e.Graphics.DrawImage(img, r1, r2, GraphicsUnit.Pixel);

                    e.Handled = true;
                }
            }
            else if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                //Draw NULLs
                if (e.Value == null || e.Value == DBNull.Value)
                {
                    e.Paint(e.CellBounds, DataGridViewPaintParts.All
                        & ~(DataGridViewPaintParts.ContentForeground));

                    var font = new Font(e.CellStyle.Font, FontStyle.Italic);
                    var color = SystemColors.ActiveCaptionText;
                    if (this.mainGridView.SelectedCells.Contains(((DataGridView)sender)[e.ColumnIndex, e.RowIndex]))
                        color = Color.White;

                    TextRenderer.DrawText(e.Graphics, "NULL", font, e.CellBounds, color,
                        TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.PreserveGraphicsClipping);

                    e.Handled = true;
                }
                else if (e.Value is ListValue)
                {
                    e.CellStyle.Font = new Font(e.CellStyle.Font, FontStyle.Underline);
                    e.CellStyle.ForeColor = Color.Blue;
                }
            }
        }

        private static bool IsCursorOverCellText(DataGridView dgv, int columnIndex, int rowIndex)
        {
            if (dgv[columnIndex, rowIndex] is DataGridViewCell cell)
            {
                var cursorPosition = dgv.PointToClient(Cursor.Position);
                var cellAreaWithTextInIt =
                    new Rectangle(dgv.GetCellDisplayRectangle(columnIndex, rowIndex, true).Location, cell.GetContentBounds(rowIndex).Size);

                return cellAreaWithTextInIt.Contains(cursorPosition);
            }

            return false;
        }

        private void searchFilterTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == Convert.ToChar(Keys.Return))
            {
                this.runQueryButton_Click(this.runQueryButton, null);
            }
        }

        private async void MainForm_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files != null && files.Length > 0)
                {
                    this.Cursor = Cursors.WaitCursor;
                    await this.OpenNewFileOrFolder(files[0]);
                }
            }
            catch (Exception ex)
            {
                this.OpenFileOrFolderPath = null;
                ShowError(ex);
            }
        }

        private void MainForm_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
        }

        private void searchFilterLabel_Click(object sender, EventArgs e)
        {
            MessageBox.Show(@"NULL CHECK: 
    WHERE field_name IS NULL
    WHERE field_name IS NOT NULL
DATETIME:   
    WHERE field_name >= #01/01/2000#
NUMERIC:
    WHERE field_name <= 123.4
STRING:
    WHERE field_name LIKE '%value%' 
    WHERE field_name = 'equals value'
    WHERE field_name <> 'not equals'
MULTIPLE CONDITIONS: 
    WHERE (field_1 > #01/01/2000# AND field_1 < #01/01/2001#) OR field_2 <> 100 OR field_3 = 'string value'", "Filtering Query Syntax Examples");
        }

        private void mainGridView_CellMouseMove(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.ColumnIndex < 0 || e.RowIndex < 0)
                return;

            if (sender is DataGridView dgv)
            {
                if (dgv.Columns[e.ColumnIndex].ValueType == typeof(ListValue))
                {
                    //Lets be fancy and only change the cursor if the user is hovering over the actual text in the cell
                    if (IsCursorOverCellText(dgv, e.ColumnIndex, e.RowIndex))
                        dgv.Cursor = Cursors.Hand;
                    else
                        dgv.Cursor = Cursors.Default;
                }
            }
        }

        private void mainGridView_MouseClick(object sender, MouseEventArgs e)
        {
            try
            {
                var dgv = (DataGridView)sender;
                if (e.Button == MouseButtons.Right)
                {
                    int rowIndex = dgv.HitTest(e.X, e.Y).RowIndex;
                    int columnIndex = dgv.HitTest(e.X, e.Y).ColumnIndex;

                    if (rowIndex >= 0 && columnIndex >= 0)
                    {
                        var toolStripMenuItem = new ToolStripMenuItem("Copy");
                        toolStripMenuItem.Click += (object clickSender, EventArgs clickArgs) =>
                        {
                            Clipboard.SetDataObject(dgv.GetClipboardContent());
                        };

                        var menu = new ContextMenuStrip();
                        menu.Items.Add(toolStripMenuItem);
                        menu.Show(dgv, new Point(e.X, e.Y));
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError(ex, null, false);
            }
        }

        private void mainGridView_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            this.actualShownRecordCountLabel.Text = this.mainGridView.RowCount.ToString();

            foreach (DataGridViewColumn column in ((DataGridView)sender).Columns)
            {
                if (column is DataGridViewCheckBoxColumn checkboxColumn)
                {
                    checkboxColumn.ThreeState = true; //handle NULLs for bools
                }
            }
        }

        private void MainGridView_ColumnAdded(object sender, DataGridViewColumnEventArgs e)
        {
            if (e.Column is DataGridViewColumn column)
            {
                //This will help avoid overflowing the sum(fillweight) of the grid's columns when there are too many of them.
                //The value of this field is not important as we do not use the FILL mode for column sizing.
                column.FillWeight = 0.01f;
            }
        }

        private void mainGridView_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1 && e.ColumnIndex >= 0)
            {
                bool isDateTimeCell = ((DataGridView)sender).Columns[e.ColumnIndex].ValueType == typeof(DateTime);
                bool isUserUsingDateOnlyFormat = AppSettings.DateTimeDisplayFormat.IsDateOnlyFormat();
                if (isDateTimeCell && isUserUsingDateOnlyFormat)
                {
                    var relativeMousePosition = this.PointToClient(Cursor.Position);
                    this.dateOnlyFormatWarningToolTip.Show($"Date only format enabled. To see time values: Edit -> Date Format",
                        this, relativeMousePosition, 10000);
                }
            }
        }

        private void mainGridView_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            this.dateOnlyFormatWarningToolTip.Hide(this);

            if (sender is DataGridView dgv)
            {
                dgv.Cursor = Cursors.Default;
            }
        }

        private void showingStatusBarLabel_Click(object sender, EventArgs e)
        {
            //This is just here in case I want to add debug info
        }

        private void mainGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            if (sender is DataGridView dgv)
            {
                //Check if there's already a quick peek open for this cell
                if (dgv[e.ColumnIndex, e.RowIndex].Tag is Guid cellUniqueTag
                    && openQuickPeekForms.TryGetValue((e.RowIndex, e.ColumnIndex), out var quickPeekForm)
                    && quickPeekForm.UniqueTag.Equals(cellUniqueTag))
                {
                    quickPeekForm.Focus();
                    return;
                }

                DataTable dt = null;
                if (dgv[e.ColumnIndex, e.RowIndex].Value is ListValue listValue)
                {
                    dt = new DataTable();
                    dt.Columns.Add(new DataColumn(dgv.Columns[e.ColumnIndex].Name, listValue.Type));

                    foreach (var item in listValue.Data)
                    {
                        var row = dt.NewRow();
                        row[0] = item;
                        dt.Rows.Add(row);
                    }
                }

                if (dt == null)
                    return;

                dgv[e.ColumnIndex, e.RowIndex].Tag = Guid.NewGuid();
                var uniqueCellTag = (Guid)dgv[e.ColumnIndex, e.RowIndex].Tag;

                var quickPeakForm = new QuickPeekForm(null, dt, uniqueCellTag, e.RowIndex, e.ColumnIndex);
                quickPeakForm.TakeMeBackEvent += (object form, TakeMeBackEventArgs tag) =>
                {
                    if (dgv.Rows.Count > tag.SourceRowIndex) //Can't be too safe
                    {
                        DataGridViewCell cellToReturnTo = dgv[tag.SourceColumnIndex, tag.SourceRowIndex];

                        //Check if the cell is still the same (user hasn't navigated the file since opening the popup)
                        if (cellToReturnTo.Tag is Guid t && t == tag.UniqueTag)
                        {
                            if (form is Form f)
                                f.Close();

                            dgv.ClearSelection();
                            dgv.FirstDisplayedScrollingRowIndex = cellToReturnTo.RowIndex;

                            if (dgv.Columns.Count > tag.SourceColumnIndex) //Can't be too safe
                            {
                                dgv.FirstDisplayedScrollingColumnIndex = tag.SourceColumnIndex;
                                dgv[cellToReturnTo.ColumnIndex, cellToReturnTo.RowIndex].Selected = true;
                            }
                            else
                            {
                                //Can't find column, select whole row
                                cellToReturnTo.Selected = true;
                            }

                            dgv.Focus();
                        }
                        else
                        {
                            //Can't find return row
                            if (form is QuickPeekForm f)
                                f.TakeMeBackLinkDisable();
                        }
                    }
                    else
                    {
                        //User has navigated the file. No chance we can find the same row again :(
                        if (form is QuickPeekForm f)
                            f.TakeMeBackLinkDisable();
                    }
                };

                quickPeakForm.FormClosed += (object sender, FormClosedEventArgs _) =>
                {
                    if (openQuickPeekForms.TryGetValue((e.RowIndex, e.ColumnIndex), out var quickPeekForm)
                        && quickPeekForm.UniqueTag.Equals(uniqueCellTag))
                    {
                        openQuickPeekForms.Remove((e.RowIndex, e.ColumnIndex));
                    }
                };

                openQuickPeekForms.Remove((e.RowIndex, e.ColumnIndex)); //Remove any leftover value if the user navigated the file
                openQuickPeekForms.Add((e.RowIndex, e.ColumnIndex), quickPeakForm);
                quickPeakForm.Show(this.Parent ?? this);
            }
        }
    }
}
