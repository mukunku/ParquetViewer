using ParquetViewer.Engine;
using ParquetViewer.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace ParquetViewer.Controls
{
    internal class ParquetGridView : DataGridView
    {
        //Actual number is around 43k (https://stackoverflow.com/q/52792876/1458738)
        //But lets use something smaller to increase rendering performance.
        private const int MAX_CHARACTERS_THAT_CAN_BE_RENDERED_IN_A_CELL = 2000;

        private readonly ToolTip dateOnlyFormatWarningToolTip = new();
        private readonly Dictionary<(int, int), QuickPeekForm> openQuickPeekForms = new();
        private bool isCopyingToClipboard = false;

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
                Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point),
                ForeColor = SystemColors.WindowText,
                SelectionBackColor = SystemColors.Highlight,
                SelectionForeColor = SystemColors.HighlightText,
                WrapMode = DataGridViewTriState.True
            };
            ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            EnableHeadersVisualStyles = false;
            ReadOnly = true;
            RowHeadersWidth = 24;
            SelectionMode = DataGridViewSelectionMode.RowHeaderSelect;
            ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            ShowCellToolTips = false; //tooltips for columns with very long strings cause performance issues. This was the easiest solution
        }

        protected override void OnDataSourceChanged(EventArgs e)
        {
            base.OnDataSourceChanged(e);

            UpdateDateFormats();

            foreach (DataGridViewColumn column in this.Columns)
            {
                //Handle NULLs for bool types
                if (column is DataGridViewCheckBoxColumn checkboxColumn)
                    checkboxColumn.ThreeState = true;
            }

            AutoSizeColumns();
        }

        public void UpdateDateFormats()
        {
            string dateFormat = AppSettings.DateTimeDisplayFormat.GetDateFormat();
            ListValue.DateDisplayFormat = dateFormat; //Need to tell the parquet engine how to render date values
            MapValue.DateDisplayFormat = dateFormat;

            foreach (DataGridViewColumn column in this.Columns)
            {
                if (column.ValueType == typeof(DateTime))
                    column.DefaultCellStyle.Format = dateFormat;
            }
        }

        public void AutoSizeColumns()
        {
            const int DEFAULT_COL_WIDTH = 100;

            if (AppSettings.AutoSizeColumnsMode == Helpers.AutoSizeColumnsMode.AllCells)
                this.FastAutoSizeColumns();
            else if (AppSettings.AutoSizeColumnsMode != Helpers.AutoSizeColumnsMode.None)
                this.AutoResizeColumns(AppSettings.AutoSizeColumnsMode.ToDGVMode());
            else
            {
                foreach (DataGridViewColumn column in this.Columns)
                {
                    column.Width = DEFAULT_COL_WIDTH;
                }
            }
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
                if (e.Value == DBNull.Value || e.Value == null)
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
                else if (e.Value is ListValue || e.Value is MapValue || e.Value is StructValue)
                {
                    e.CellStyle.Font = new Font(e.CellStyle.Font, FontStyle.Underline);
                    e.CellStyle.ForeColor = Color.Blue;
                }
                else if (e.Value is ByteArrayValue byteArrayValue)
                {
                    var tag = this.Columns[e.ColumnIndex].Tag as string;
                    if (tag is null || (!tag.Equals("NOT-IMAGE") && !tag.Equals("IMAGE")))
                    {
                        tag = byteArrayValue.IsImage() ? "IMAGE" : "NOT-IMAGE";
                        this.Columns[e.ColumnIndex].Tag = tag;
                    }

                    if (tag.Equals("IMAGE"))
                    {
                        e.CellStyle.Font = new Font(e.CellStyle.Font, FontStyle.Underline);
                        e.CellStyle.ForeColor = Color.Blue;
                    }
                }
            }

            base.OnCellPainting(e); //Handle any additional event handlers
        }

        protected override void OnCellMouseMove(DataGridViewCellMouseEventArgs e)
        {
            base.OnCellMouseMove(e);

            if (e.ColumnIndex < 0 || e.RowIndex < 0)
                return;

            var valueType = this.Columns[e.ColumnIndex].ValueType;
            var isClickableByteArrayType = valueType == typeof(ByteArrayValue) && this.Columns[e.ColumnIndex].Tag is string tag && tag.Equals("IMAGE");
            if (valueType == typeof(ListValue) || valueType == typeof(MapValue) || valueType == typeof(StructValue) || isClickableByteArrayType)
            {
                //Lets be fancy and only change the cursor if the user is hovering over the actual text in the cell
                if (IsCursorOverCellText(e.ColumnIndex, e.RowIndex))
                    this.Cursor = Cursors.Hand;
                else
                    this.Cursor = Cursors.Default;
            }
            else
            {
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
                    var copy = new ToolStripMenuItem("Copy");
                    copy.Click += (object clickSender, EventArgs clickArgs) =>
                    {
                        this.isCopyingToClipboard = true;
                        Clipboard.SetDataObject(this.GetClipboardContent());
                        this.isCopyingToClipboard = false;
                    };

                    var copyWithHeaders = new ToolStripMenuItem("Copy with headers");
                    copyWithHeaders.Click += (object clickSender, EventArgs clickArgs) =>
                    {
                        this.isCopyingToClipboard = true;
                        this.RowHeadersVisible = false; //disable row headers temporarily so they don't end up in the clipboard content
                        this.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
                        Clipboard.SetDataObject(this.GetClipboardContent());
                        this.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
                        this.RowHeadersVisible = true;
                        this.isCopyingToClipboard = false;
                    };

                    var menu = new ContextMenuStrip();
                    menu.Items.Add(copy);
                    menu.Items.Add(copyWithHeaders);
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
                        Parent ?? this, relativeMousePosition, 10000);
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
            else if (clickedCell.Value is MapValue mapValue)
            {
                dt = new DataTable();
                dt.Columns.Add(new DataColumn($"{this.Columns[e.ColumnIndex].Name}-key", mapValue.KeyType));
                dt.Columns.Add(new DataColumn($"{this.Columns[e.ColumnIndex].Name}-value", mapValue.ValueType));

                var row = dt.NewRow();
                row[0] = mapValue.Key;
                row[1] = mapValue.Value;
                dt.Rows.Add(row);
            }
            else if (clickedCell.Value is StructValue structValue)
            {
                dt = structValue.Data.Table.Clone();
                var row = dt.NewRow();
                row.ItemArray = structValue.Data.ItemArray;
                dt.Rows.Add(row);
            }
            else if (clickedCell.Value is ByteArrayValue byteArray)
            {
                Image image = null;
                try
                {
                    image = byteArray.ToImage();
                }
                catch { /* not an image */ }

                if (image is not null)
                {
                    new ImagePreviewForm()
                    {
                        PreviewImage = image,
                        Width = image.Width + 22,
                        Height = image.Height + 77
                    }.Show(this.Parent ?? this);
                }
            }

            if (dt is null)
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
                        this.CurrentCell = cellToReturnTo;
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

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Modifiers.HasFlag(Keys.Control) && e.KeyCode.HasFlag(Keys.C))
            {
                this.isCopyingToClipboard = true;
            }
            base.OnKeyDown(e);
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            this.isCopyingToClipboard = false;
            base.OnKeyUp(e);
        }

        protected override void OnCellFormatting(DataGridViewCellFormattingEventArgs e)
        {
            base.OnCellFormatting(e);

            if (this.isCopyingToClipboard)
            {
                //In order to get the full values into the clipboard we need to
                //disable formatting during a copy to clipboard operation
                return;
            }

            var cellValueType = this[e.ColumnIndex, e.RowIndex].ValueType;
            if (cellValueType == typeof(ByteArrayValue))
            {
                string value = e.Value.ToString();
                if (value.Length > MAX_CHARACTERS_THAT_CAN_BE_RENDERED_IN_A_CELL)
                {
                    e.Value = value[..(MAX_CHARACTERS_THAT_CAN_BE_RENDERED_IN_A_CELL / 2)]
                        + "[...]" + value[(value.Length - (MAX_CHARACTERS_THAT_CAN_BE_RENDERED_IN_A_CELL / 2))..];
                    e.FormattingApplied = true;
                }
            }
            else if (cellValueType == typeof(string))
            {
                string value = e.Value.ToString();
                if (value.Length > MAX_CHARACTERS_THAT_CAN_BE_RENDERED_IN_A_CELL)
                {
                    e.Value = value[..MAX_CHARACTERS_THAT_CAN_BE_RENDERED_IN_A_CELL] + "[...]";
                    e.FormattingApplied = true;
                }
            }
            else if (cellValueType == typeof(StructValue))
            {
                string value = e.Value.ToString();
                if (value.Length > MAX_CHARACTERS_THAT_CAN_BE_RENDERED_IN_A_CELL)
                {
                    e.Value = ((StructValue)e.Value).ToStringTruncated();
                    e.FormattingApplied = true;
                }
            }
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
            const string WHITESPACE_BUFFER = "#|";
            const int MAX_WIDTH = 400;

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
                    //Don't autosize the same column twice
                    if (this.Columns[i].Tag is string tag && tag.Equals("AUTOSIZED"))
                        continue;
                    else
                        this.Columns[i].Tag = "AUTOSIZED";

                    //Fit header by default. If header is short, make sure NULLs will fit at least
                    string columnNameOrNull = gridTable.Columns[i].ColumnName.Length < 5 ? "NULL" : gridTable.Columns[i].ColumnName;
                    var newColumnSize = MeasureStringWidth(columnNameOrNull + WHITESPACE_BUFFER);

                    if (gridTable.Columns[i].DataType == typeof(DateTime))
                    {
                        //All date time's will have the same string length so no need to go through actual values.
                        //We can just measure one and use that.
                        string formattedDateTimeValue = DateTime.Now.ToString(AppSettings.DateTimeDisplayFormat.GetDateFormat());
                        var maxDateTimeWidth = MeasureStringWidth(formattedDateTimeValue + WHITESPACE_BUFFER);

                        // If the calculated width is larger than the column header width, use that instead
                        if (maxDateTimeWidth > newColumnSize)
                            newColumnSize = maxDateTimeWidth;
                    }
                    else
                    {
                        // Collect all the rows into a string array, making sure to exclude null values.
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

                    this.Columns[i].Width = Math.Min(newColumnSize, MAX_WIDTH);
                }

                int MeasureStringWidth(string input)
                {
                    try
                    {
                        return (int)gfx.MeasureString(input, this.Font).Width;
                    }
                    catch (Exception)
                    {
                        return int.MaxValue; //Assume worst case
                    }
                }
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
