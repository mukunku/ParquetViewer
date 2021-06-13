using ParquetFileViewer.CustomGridTypes;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace ParquetFileViewer.Controls
{
    public class ParquetGridView : DataGridView
    {
        private static readonly Random random = new Random();

        public ParquetGridView() : base()
        {
            this.CellPainting += ParquetGridView_CellPainting;
            this.ColumnAdded += ParquetGridView_ColumnAdded;
            this.CellMouseMove += ParquetGridView_CellMouseMove;
            this.CellMouseLeave += ParquetGridView_CellMouseLeave;
            this.MouseClick += ParquetGridView_MouseClick;
            this.CellContentClick += ParquetGridView_CellContentClick;
        }

        private void ParquetGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            if (sender is DataGridView dgv)
            {
                DataTable dt = null;
                if (dgv[e.ColumnIndex, e.RowIndex].Value is ListType lt)
                {
                    dt = new DataTable();
                    dt.Columns.Add(new DataColumn(dgv.Columns[e.ColumnIndex].Name, lt.Type));

                    foreach (var item in lt.Data)
                    {
                        var row = dt.NewRow();
                        row[0] = item;
                        dt.Rows.Add(row);
                    }
                }

                if (dt != null)
                {
                    int uniqueTag = random.Next();
                    dgv.Rows[e.RowIndex].Tag = uniqueTag;

                    var quickPeakForm = new QuickPeekForm(null, dt, uniqueTag, e.RowIndex, e.ColumnIndex);
                    quickPeakForm.TakeMeBackEvent += (object form, TakeMeBackEventArgs tag) =>
                    {
                        if (this.Rows.Count > tag.SourceRowIndex)
                        {
                            DataGridViewRow rowToReturnTo = null;

                            //Check if the row is still the same (user hasn't navigated the file since opening the popup)
                            if (this.Rows[tag.SourceRowIndex].Tag is int t && t == tag.UniqueTag)
                            {
                                rowToReturnTo = this.Rows[tag.SourceRowIndex];
                            }
                            else
                            {
                                //It seems the row is not there anymore. Try to linear search for it :/
                                foreach(DataGridViewRow row in this.Rows)
                                {
                                    if (row.Tag is int t2 && t2 == tag.UniqueTag)
                                    {
                                        rowToReturnTo = row;
                                        break;
                                    }
                                }
                            }

                            if (rowToReturnTo != null)
                            {
                                if (form is Form f)
                                {
                                    f.Close();
                                    f.Dispose();
                                }

                                this.ClearSelection();
                                this.FirstDisplayedScrollingRowIndex = rowToReturnTo.Index;

                                if (this.Columns.Count > tag.SourceColumnIndex) //Can't be too safe
                                {
                                    this.FirstDisplayedScrollingColumnIndex = tag.SourceColumnIndex;
                                    this[tag.SourceColumnIndex, rowToReturnTo.Index].Selected = true;
                                }
                                else
                                    rowToReturnTo.Selected = true;

                                this.Focus();
                            }
                            else
                            {
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

                    quickPeakForm.Show(this.Parent ?? this);
                }
            }
        }

        private void ParquetGridView_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            if ((e.Value == null || e.Value == DBNull.Value) && sender is DataGridView dgv)
            {
                e.Paint(e.CellBounds, DataGridViewPaintParts.All
                    & ~(DataGridViewPaintParts.ContentForeground));

                var font = new Font(e.CellStyle.Font, FontStyle.Italic);
                var color = SystemColors.ActiveCaptionText;

                if (dgv.SelectedCells.Contains(((DataGridView)sender)[e.ColumnIndex, e.RowIndex]))
                    color = Color.White;

                TextRenderer.DrawText(e.Graphics, "NULL", font, e.CellBounds, color, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);

                e.Handled = true;
            }
            else if (e.Value is ListType)
            {
                e.CellStyle.Font = new Font(e.CellStyle.Font, FontStyle.Underline);
                e.CellStyle.ForeColor = Color.Blue;
            }
        }

        private void ParquetGridView_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            if (sender is DataGridView dgv)
            {
                dgv.Cursor = Cursors.Default;
            }
        }

        private void ParquetGridView_ColumnAdded(object sender, DataGridViewColumnEventArgs e)
        {
            if (e.Column is DataGridViewColumn column)
            {
                //This will help avoid overflowing the sum(fillweight) of the grid's columns when there are too many of them.
                //The value of this field is not important as we do not use the FILL mode for column sizing.
                column.FillWeight = 0.01f;
            }
        }

        private void ParquetGridView_CellMouseMove(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.ColumnIndex < 0 || e.RowIndex < 0)
                return;

            if (sender is DataGridView dgv)
            {
                if (dgv.Columns[e.ColumnIndex].ValueType == typeof(ListType))
                {
                    //Lets be fancy and only change the cursor if the user is hovering over the actual text in the cell
                    if (IsCursorOverCellText(dgv, e.ColumnIndex, e.RowIndex))
                        dgv.Cursor = Cursors.Hand;
                    else
                        dgv.Cursor = Cursors.Default;
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

        private void ParquetGridView_MouseClick(object sender, MouseEventArgs e)
        {
            try
            {
                //We only handle single cell copy below :(
                var dgv = (DataGridView)sender;
                if (e.Button == MouseButtons.Right)
                {
                    int rowIndex = dgv.HitTest(e.X, e.Y).RowIndex;
                    int columnIndex = dgv.HitTest(e.X, e.Y).ColumnIndex;

                    if (rowIndex >= 0 && columnIndex >= 0)
                    {
                        ContextMenu menu = new ContextMenu();
                        var copyMenuItem = new MenuItem("Copy");

                        copyMenuItem.Click += (object clickSender, EventArgs clickArgs) =>
                        {
                            string value = dgv[columnIndex, rowIndex].Value?.ToString();
                            if (!string.IsNullOrEmpty(value))
                                Clipboard.SetText(value);
                            else
                                Clipboard.Clear();
                        };

                        menu.MenuItems.Add(copyMenuItem);
                        menu.Show(dgv, new Point(e.X, e.Y));
                    }
                }
            }
            catch (Exception ex)
            {
                Clipboard.SetText($"Something went wrong while copying...{Environment.NewLine}{ex.ToString()}");
            }
        }
    }
}
