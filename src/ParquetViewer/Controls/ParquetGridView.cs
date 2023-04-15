using ParquetViewer.Engine;
using ParquetViewer.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace ParquetViewer.Controls
{
    internal class ParquetGridView : DataGridView
    {
        private ToolTip dateOnlyFormatWarningToolTip = new();
        private Dictionary<(int, int), QuickPeekForm> openQuickPeekForms = new();

        public ParquetGridView() : base()
        {
            DoubleBuffered = true; //Set DGV to be double buffered for smoother loading and scrolling
            AllowUserToAddRows = false;
            AllowUserToDeleteRows = false;
            AllowUserToOrderColumns = true;
            ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            ColumnHeadersDefaultCellStyle = new()
            {
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                BackColor = SystemColors.ControlLight,
                Font = new Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point),
                ForeColor = SystemColors.WindowText,
                SelectionBackColor = SystemColors.Highlight,
                SelectionForeColor = SystemColors.HighlightText,
                WrapMode = DataGridViewTriState.True
            };
            ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            EnableHeadersVisualStyles = false;
            ReadOnly = true;
            RowHeadersWidth = 24;
            SelectionMode = DataGridViewSelectionMode.CellSelect;
        }

        protected override void OnDataBindingComplete(DataGridViewBindingCompleteEventArgs e)
        {
            //Format date fields
            string dateFormat = AppSettings.DateTimeDisplayFormat.GetDateFormat();
            ListValue.DateDisplayFormat = dateFormat; //Need to tell the parquet engine how to render date values
            foreach (DataGridViewColumn column in this.Columns)
            {
                if (column.ValueType == typeof(DateTime))
                    column.DefaultCellStyle.Format = dateFormat;
            }

            //Handle NULLs for bool types
            foreach (DataGridViewColumn column in this.Columns)
            {
                if (column is DataGridViewCheckBoxColumn checkboxColumn)
                {
                    checkboxColumn.ThreeState = true;
                }
            }

            //Adjust column sizes if required
            if (AppSettings.AutoSizeColumnsMode == Helpers.AutoSizeColumnsMode.AllCells)
                this.FastAutoSizeColumns();
            else if (AppSettings.AutoSizeColumnsMode != Helpers.AutoSizeColumnsMode.None)
                this.AutoResizeColumns(AppSettings.AutoSizeColumnsMode.ToDGVMode());

            base.OnDataBindingComplete(e);
        }

        protected override void OnCellPainting(DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex == -1 && e.ColumnIndex >= 0)
            {
                //Add warnings to date field headers if the user is using a "Date Only" date format.
                //We want to be helpful so people don't accidentally leave a date only format on and think they are missing time information in their data.

                bool isDateTimeCell = this.Columns[e.ColumnIndex].ValueType == typeof(DateTime);
                bool isUserUsingDateOnlyFormat = AppSettings.DateTimeDisplayFormat.IsDateOnlyFormat();
                if (isDateTimeCell && isUserUsingDateOnlyFormat)
                {
                    var img = Properties.Resources.exclamation_icon_yellow;
                    Rectangle r1 = new(e.CellBounds.Left + e.CellBounds.Width - img.Width, 4, img.Width, img.Height);
                    Rectangle r2 = new(0, 0, img.Width, img.Height);
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
                    if (this.SelectedCells.Contains(this[e.ColumnIndex, e.RowIndex]))
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

            base.OnCellPainting(e); //Handle any additional event handlers
        }

        protected override void OnCellMouseMove(DataGridViewCellMouseEventArgs e)
        {
            base.OnCellMouseMove(e);

            if (e.ColumnIndex < 0 || e.RowIndex < 0)
                return;

            if (this.Columns[e.ColumnIndex].ValueType == typeof(ListValue))
            {
                //Lets be fancy and only change the cursor if the user is hovering over the actual text in the cell
                if (IsCursorOverCellText(e.ColumnIndex, e.RowIndex))
                    this.Cursor = Cursors.Hand;
                else
                    this.Cursor = Cursors.Default;
            }
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                int rowIndex = this.HitTest(e.X, e.Y).RowIndex;
                int columnIndex = this.HitTest(e.X, e.Y).ColumnIndex;

                if (rowIndex >= 0 && columnIndex >= 0)
                {
                    var toolStripMenuItem = new ToolStripMenuItem("Copy");
                    toolStripMenuItem.Click += (object clickSender, EventArgs clickArgs) =>
                    {
                        Clipboard.SetDataObject(this.GetClipboardContent());
                    };

                    var menu = new ContextMenuStrip();
                    menu.Items.Add(toolStripMenuItem);
                    menu.Show(this, new Point(e.X, e.Y));
                }
            }

            base.OnMouseClick(e);
        }

        protected override void OnColumnAdded(DataGridViewColumnEventArgs e)
        {
            if (e.Column is DataGridViewColumn column)
            {
                //This will help avoid overflowing the sum(fillweight) of the grid's columns when there are too many of them.
                //The value of this field is not important as we do not use the FILL mode for column sizing.
                column.FillWeight = 0.01f;
            }

            base.OnColumnAdded(e);
        }

        protected override void OnCellMouseEnter(DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1 && e.ColumnIndex >= 0)
            {
                bool isDateTimeCell = this.Columns[e.ColumnIndex].ValueType == typeof(DateTime);
                bool isUserUsingDateOnlyFormat = AppSettings.DateTimeDisplayFormat.IsDateOnlyFormat();
                if (isDateTimeCell && isUserUsingDateOnlyFormat)
                {
                    var relativeMousePosition = this.PointToClient(Cursor.Position);
                    this.dateOnlyFormatWarningToolTip.Show($"Date only format enabled. To see time values: Edit -> Date Format",
                        this, relativeMousePosition, 10000);
                }
            }

            base.OnCellMouseEnter(e);
        }

        protected override void OnCellMouseLeave(DataGridViewCellEventArgs e)
        {
            this.dateOnlyFormatWarningToolTip.Hide(this);
            this.Cursor = Cursors.Default;

            base.OnCellMouseLeave(e);
        }

        protected override void OnCellContentClick(DataGridViewCellEventArgs e)
        {
            base.OnCellContentClick(e);

            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            var clickedCell = this[e.ColumnIndex, e.RowIndex];

            //Check if there's already a quick peek open for this cell
            if (clickedCell.Tag is Guid cellUniqueTag
                && openQuickPeekForms.TryGetValue((e.RowIndex, e.ColumnIndex), out var quickPeekForm)
                && quickPeekForm.UniqueTag.Equals(cellUniqueTag))
            {
                //TODO: Move the form to the cursor location, maybe? Might help for multi monitor setups.
                quickPeekForm.Focus();
                return;
            }

            DataTable dt = null;
            if (clickedCell.Value is ListValue listValue)
            {
                dt = new DataTable();
                dt.Columns.Add(new DataColumn(this.Columns[e.ColumnIndex].Name, listValue.Type));

                foreach (var item in listValue.Data)
                {
                    var row = dt.NewRow();
                    row[0] = item;
                    dt.Rows.Add(row);
                }
            }

            if (dt == null)
                return;

            var uniqueCellTag = Guid.NewGuid();
            clickedCell.Tag = uniqueCellTag;

            var quickPeakForm = new QuickPeekForm(null, dt, uniqueCellTag, e.RowIndex, e.ColumnIndex);
            quickPeakForm.TakeMeBackEvent += (object form, TakeMeBackEventArgs tag) =>
            {
                if (this.Rows.Count > tag.SourceRowIndex && this.Columns.Count > tag.SourceColumnIndex) //Can't be too safe
                {
                    DataGridViewCell cellToReturnTo = this[tag.SourceColumnIndex, tag.SourceRowIndex];

                    //Check if the cell is still the same (user hasn't navigated the file since opening the popup)
                    if (cellToReturnTo.Tag is Guid t && t == tag.UniqueTag)
                    {
                        if (form is Form f)
                            f.Close();

                        this.ClearSelection();
                        this.FirstDisplayedScrollingRowIndex = cellToReturnTo.RowIndex;
                        this.FirstDisplayedScrollingColumnIndex = tag.SourceColumnIndex;
                        this[cellToReturnTo.ColumnIndex, cellToReturnTo.RowIndex].Selected = true;
                        this.Focus();
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
                    //User has navigated the file. We can't find the same cell again
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

        public void ClearQuickPeekForms()
        {
            foreach (var form in this.openQuickPeekForms)
            {
                try
                {
                    form.Value.Close();
                }
                catch { /*Swallow*/ }
            }
        }

        /// <summary>
        /// Provides very fast and basic column sizing for large data sets.
        /// </summary>
        private void FastAutoSizeColumns()
        {
            const string WHITESPACE_BUFFER = "#";

            // Cast out a DataTable from the target grid datasource.
            // We need to iterate through all the data in the grid and a DataTable supports enumeration.
            var gridTable = this.DataSource as DataTable;

            if (gridTable is null)
                return;

            // Create a graphics object from the target grid. Used for measuring text size.
            using (var gfx = this.CreateGraphics())
            {
                // Iterate through the columns.
                for (int i = 0; i < gridTable.Columns.Count; i++)
                {
                    var newColumnSize = MeasureStringWidth(gridTable.Columns[i].ColumnName + WHITESPACE_BUFFER); //Fit header by default
                    bool isComplexType = typeof(IComplexValue).IsAssignableFrom(gridTable.Columns[i].DataType);
                    if (!isComplexType)
                    {
                        // Leverage Linq enumerator to rapidly collect all the rows into a string array, making sure to exclude null values.
                        IEnumerable<string> colStringCollection = gridTable.AsEnumerable()
                            .Select(row => row.Field<object>(i)?.ToString())
                            .Where(value => value is not null);

                        // Sort the string array by string lengths.
                        colStringCollection = colStringCollection.OrderBy((x) => x.Length);

                        // Get the last and longest string in the array.
                        string longestColString = colStringCollection.LastOrDefault() ?? string.Empty;

                        if (gridTable.Columns[i].ColumnName.Length > longestColString.Length)
                            longestColString = gridTable.Columns[i].ColumnName + WHITESPACE_BUFFER;

                        var maxColWidth = MeasureStringWidth(longestColString + WHITESPACE_BUFFER);

                        // If the calculated width is larger than the column header width, use that instead
                        if (maxColWidth > newColumnSize)
                            newColumnSize = maxColWidth;
                    }

                    this.Columns[i].Width = newColumnSize;
                }

                int MeasureStringWidth(string input) => (int)gfx.MeasureString(input, this.Font).Width;
            }
        }

        private bool IsCursorOverCellText(int columnIndex, int rowIndex)
        {
            if (this[columnIndex, rowIndex] is DataGridViewCell cell)
            {
                var cursorPosition = this.PointToClient(Cursor.Position);
                var cellAreaWithTextInIt =
                    new Rectangle(this.GetCellDisplayRectangle(columnIndex, rowIndex, true).Location, cell.GetContentBounds(rowIndex).Size);

                return cellAreaWithTextInIt.Contains(cursorPosition);
            }

            return false;
        }
    }
}
