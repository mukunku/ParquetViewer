using ParquetViewer.Analytics;
using ParquetViewer.Engine;
using ParquetViewer.Engine.Types;
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
        //Actual number is around 43k (https://stackoverflow.com/q/52792876/1458738)
        //But lets use something smaller to increase rendering performance.
        private const int MAX_CHARACTERS_THAT_CAN_BE_RENDERED_IN_A_CELL = 2000;

        private readonly Dictionary<(int, int), QuickPeekForm> openQuickPeekForms = new();
        private bool isCopyingToClipboard = false;

        private Theme _gridTheme = Constants.LightModeTheme;
        public Theme GridTheme
        {
            get => _gridTheme;
            set
            {
                if (value != _gridTheme)
                {
                    _gridTheme = value;
                    SetTheme();
                }
            }
        }

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
            base.OnDataSourceChanged(e); //Fun fact: This runs OnColumnAdded() for all columns before continuing.

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

            foreach (DataGridViewColumn column in this.Columns)
            {
                if (column.ValueType == typeof(DateTime))
                    column.DefaultCellStyle.Format = dateFormat;
            }

            //Need to tell the parquet engine how to render date values
            ParquetEngineSettings.DateDisplayFormat = dateFormat;
        }

        public void AutoSizeColumns()
        {
            const int DEFAULT_COL_WIDTH = 100;

            if (AppSettings.AutoSizeColumnsMode == Helpers.AutoSizeColumnsMode.AllCells)
                this.FastAutoSizeColumns();
            else if (AppSettings.AutoSizeColumnsMode.ToDGVMode() != DataGridViewAutoSizeColumnsMode.None)
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
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                //Draw NULLs
                if (e.Value == DBNull.Value || e.Value == null)
                {
                    e.Paint(e.CellBounds, DataGridViewPaintParts.All
                        & ~(DataGridViewPaintParts.ContentForeground));

                    var font = new Font(e.CellStyle!.Font, FontStyle.Italic);
                    var color = this.GridTheme.CellPlaceholderTextColor;
                    if (e.State.HasFlag(DataGridViewElementStates.Selected))
                        color = Color.White;

                    TextRenderer.DrawText(e.Graphics!, "NULL", font, e.CellBounds, color,
                        TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.PreserveGraphicsClipping);

                    e.Handled = true;
                }
                else if (e.Value is ListValue || e.Value is MapValue || e.Value is StructValue)
                {
                    e.CellStyle!.Font = new Font(e.CellStyle.Font, FontStyle.Underline);
                    e.CellStyle.ForeColor = this.GridTheme.HyperlinkColor;
                }
                else if (e.Value is ByteArrayValue byteArrayValue)
                {
                    var tag = this.Columns[e.ColumnIndex].Tag as string;
                    if (tag is null || (!tag.Equals("NOT-IMAGE") && !tag.Equals("IMAGE")))
                    {
                        tag = byteArrayValue.ToImage(out _) ? "IMAGE" : "NOT-IMAGE";
                        this.Columns[e.ColumnIndex].Tag = tag;
                    }

                    if (tag.Equals("IMAGE"))
                    {
                        e.CellStyle!.Font = new Font(e.CellStyle.Font, FontStyle.Underline);
                        e.CellStyle.ForeColor = this.GridTheme.HyperlinkColor;
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
                    copy.Click += (object? clickSender, EventArgs clickArgs) =>
                    {
                        this.isCopyingToClipboard = true;
                        Clipboard.SetDataObject(this.GetClipboardContent(), true, 2, 250); //Without these extra params, this call can cause a UI thread deadlock somehow...
                        this.isCopyingToClipboard = false;
                    };

                    var copyWithHeaders = new ToolStripMenuItem("Copy with headers");
                    copyWithHeaders.Click += (object? clickSender, EventArgs clickArgs) =>
                    {
                        this.isCopyingToClipboard = true;
                        this.RowHeadersVisible = false; //disable row headers temporarily so they don't end up in the clipboard content
                        this.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
                        Clipboard.SetDataObject(this.GetClipboardContent(), true, 2, 250); //Without these extra params, this call can cause a UI thread deadlock somehow...
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

        protected override void OnCellMouseLeave(DataGridViewCellEventArgs e)
        {
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
                && openQuickPeekForms.TryGetValue((e.RowIndex, e.ColumnIndex), out var existingQuickPeekForm)
                && existingQuickPeekForm.UniqueTag.Equals(cellUniqueTag))
            {
                //Idea: Move the form to the cursor location, maybe? Might help for multi monitor setups.
                existingQuickPeekForm.Focus();
                return;
            }

            var dataType = QuickPeekEvent.DataTypeId.Unknown;
            QuickPeekForm? quickPeekForm = null;
            var uniqueCellTag = Guid.NewGuid();
            if (clickedCell.Value is ListValue listValue)
            {
                dataType = QuickPeekEvent.DataTypeId.List;

                var dt = new DataTable();
                dt.Columns.Add(new DataColumn(this.Columns[e.ColumnIndex].Name, listValue.Type!));

                foreach (var item in listValue)
                {
                    var row = dt.NewRow();
                    row[0] = item;
                    dt.Rows.Add(row);
                }

                quickPeekForm = new QuickPeekForm(this.Columns[e.ColumnIndex].Name, dt, uniqueCellTag, e.RowIndex, e.ColumnIndex);
            }
            else if (clickedCell.Value is MapValue mapValue)
            {
                dataType = QuickPeekEvent.DataTypeId.Map;

                var dt = new DataTable();
                dt.Columns.Add(new DataColumn($"key", mapValue.KeyType));
                dt.Columns.Add(new DataColumn($"value", mapValue.ValueType));

                foreach ((object key, object value) in mapValue)
                {
                    var row = dt.NewRow();
                    row[0] = key;
                    row[1] = value;
                    dt.Rows.Add(row);
                }

                quickPeekForm = new QuickPeekForm(this.Columns[e.ColumnIndex].Name, dt, uniqueCellTag, e.RowIndex, e.ColumnIndex);
            }
            else if (clickedCell.Value is StructValue structValue)
            {
                dataType = QuickPeekEvent.DataTypeId.Struct;

                var dt = structValue.Data.Table.Clone();
                var row = dt.NewRow();
                row.ItemArray = structValue.Data.ItemArray;
                dt.Rows.Add(row);

                quickPeekForm = new QuickPeekForm(this.Columns[e.ColumnIndex].Name, dt, uniqueCellTag, e.RowIndex, e.ColumnIndex);
            }
            else if (clickedCell.Value is ByteArrayValue byteArray && byteArray.ToImage(out var image))
            {
                dataType = QuickPeekEvent.DataTypeId.Image;
                quickPeekForm = new QuickPeekForm(this.Columns[e.ColumnIndex].Name, image!, uniqueCellTag, e.RowIndex, e.ColumnIndex);
            }
            else
            {
                //Nothing to preview
                return;
            }

            clickedCell.Tag = uniqueCellTag;

            quickPeekForm.TakeMeBackEvent += (object? form, TakeMeBackEventArgs tag) =>
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
                            f.DisableTakeMeBackLink();
                    }
                }
                else
                {
                    //User has navigated the file. We can't find the same cell again
                    if (form is QuickPeekForm f)
                        f.DisableTakeMeBackLink();
                }
            };

            quickPeekForm.FormClosed += (object? sender, FormClosedEventArgs _) =>
            {
                if (openQuickPeekForms.TryGetValue((e.RowIndex, e.ColumnIndex), out var quickPeekForm)
                    && quickPeekForm.UniqueTag.Equals(uniqueCellTag))
                {
                    openQuickPeekForms.Remove((e.RowIndex, e.ColumnIndex));
                }
            };

            openQuickPeekForms.Remove((e.RowIndex, e.ColumnIndex)); //Remove any leftover value if the user navigated the file
            openQuickPeekForms.Add((e.RowIndex, e.ColumnIndex), quickPeekForm);
            quickPeekForm.Show(this.Parent ?? this);
            QuickPeekEvent.FireAndForget(dataType);
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

            var cellValueType = this[e.ColumnIndex, e.RowIndex].ValueType;
            if (cellValueType == typeof(float) && e.Value is float f)
            {
                e.Value = f.ToDecimalString();
                e.FormattingApplied = true;
            }
            else if (cellValueType == typeof(double) && e.Value is double d)
            {
                e.Value = d.ToDecimalString();
                e.FormattingApplied = true;
            }
            
            if (this.isCopyingToClipboard)
            {
                //Temporarily replace checkboxes with true/false for better copy/paste experience.
                //Otherwise you end up with: Indeterminate, Cleared, or Selected
                if (cellValueType == typeof(bool))
                {
                    if (e.Value == DBNull.Value)
                    {
                        e.Value = string.Empty;
                        e.FormattingApplied = true;
                    }
                    else if (e.Value is bool @bool)
                    {
                        e.Value = @bool.ToString();
                        e.FormattingApplied = true;
                    }
                }

                //In order to get full cell values into the clipboard during a copy to
                //clipboard operation we need to skip the truncation formatting below 
                return;
            }
            
            if (e.FormattingApplied)
            {
                //We already formatted the value above so exit early.
                return;
            }

            if (cellValueType == typeof(ByteArrayValue))
            {
                string value = e.Value!.ToString()!; //We never put `null` in cells. Only `DBNull.Value` so it can't be null.
                if (value.Length > MAX_CHARACTERS_THAT_CAN_BE_RENDERED_IN_A_CELL)
                {
                    e.Value = value[..(MAX_CHARACTERS_THAT_CAN_BE_RENDERED_IN_A_CELL / 2)]
                        + " [...] " + value[(value.Length - (MAX_CHARACTERS_THAT_CAN_BE_RENDERED_IN_A_CELL / 2))..];
                    e.FormattingApplied = true;
                }
            }
            else if (cellValueType == typeof(string))
            {
                string value = e.Value!.ToString()!;
                if (value.Length > MAX_CHARACTERS_THAT_CAN_BE_RENDERED_IN_A_CELL)
                {
                    e.Value = value[..MAX_CHARACTERS_THAT_CAN_BE_RENDERED_IN_A_CELL] + "[...]";
                    e.FormattingApplied = true;
                }
            }
            else if (cellValueType == typeof(StructValue))
            {
                string value = e.Value!.ToString()!;
                if (value.Length > MAX_CHARACTERS_THAT_CAN_BE_RENDERED_IN_A_CELL)
                {
                    e.Value = ((StructValue)e.Value).ToStringTruncated();
                    e.FormattingApplied = true;
                }
            }
        }

        protected override void OnSorted(EventArgs e)
        {
            if (!(this.SortedColumn.Tag is string tag && tag.Equals("WIDENED")))
            {
                using var gfx = this.CreateGraphics();
                var headerLength = MeasureStringWidth(gfx, this.SortedColumn.Name, true);
                var columnWidth = this.SortedColumn.Width;

                //Widen the column a bit so the sorting arrow can be shown.
                var whitespaceWidth = columnWidth - headerLength;
                if (whitespaceWidth >= 0 && whitespaceWidth < 21)
                {
                    this.SortedColumn.Width += 21 - whitespaceWidth;
                }

                //Don't widen the same column twice (this shouldn't be needed but I don't trust the logic above)
                this.SortedColumn.Tag = "WIDENED";
            }
            base.OnSorted(e);
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
            const int MAX_WIDTH = 360;
            const int DECIMAL_PREFERRED_WIDTH = 180;

            if (this.DataSource is not DataTable gridTable)
                return;

            var maxWidth = MAX_WIDTH;

            // Create a graphics object from the target grid. Used for measuring text size.
            using var gfx = this.CreateGraphics();

            for (int i = 0; i < gridTable.Columns.Count; i++)
            {
                //Don't autosize the same column twice
                if (this.Columns[i].Tag is string tag && tag.Equals("AUTOSIZED"))
                    continue;
                else
                    this.Columns[i].Tag = "AUTOSIZED";

                //Fit header by default. If header is short, make sure NULLs will fit at least
                string columnNameOrNull = gridTable.Columns[i].ColumnName.Length < 5 ? "NULL" : gridTable.Columns[i].ColumnName;
                var newColumnSize = MeasureStringWidth(gfx, columnNameOrNull, true);

                if (gridTable.Columns[i].DataType == typeof(DateTime))
                {
                    //All date time's will have the same string length so no need to go through actual values.
                    //We can just measure one and use that.
                    var dateTime = gridTable.AsEnumerable()
                        .FirstOrDefault(row => row[i] != DBNull.Value)?
                        .Field<DateTime>(i) ?? DateTime.UtcNow;
                    string formattedDateTimeValue = dateTime.ToString(AppSettings.DateTimeDisplayFormat.GetDateFormat());
                    newColumnSize = Math.Max(newColumnSize, MeasureStringWidth(gfx, formattedDateTimeValue, false));
                }
                else
                {
                    // Collect all the rows into a string enumerable, making sure to exclude null values.
                    IEnumerable<string> colStringCollection;
                    if (gridTable.Columns[i].DataType == typeof(StructValue))
                    {
                        colStringCollection = gridTable.AsEnumerable()
                            .Select(row => row.Field<StructValue>(i)?.ToStringTruncated())
                            .Where(value => value is not null)!;
                    }
                    else if (gridTable.Columns[i].DataType == typeof(float))
                    {
                        colStringCollection = gridTable.AsEnumerable()
                            .Where(row => row[i] != DBNull.Value)
                            .Select(row => row.Field<float>(i).ToDecimalString());

                        //Allow longer than preferred width if header is longer
                        maxWidth = Math.Max(newColumnSize, DECIMAL_PREFERRED_WIDTH);
                    }
                    else if (gridTable.Columns[i].DataType == typeof(double))
                    {
                        colStringCollection = gridTable.AsEnumerable()
                            .Where(row => row[i] != DBNull.Value)
                            .Select(row => row.Field<double>(i).ToDecimalString());

                        //Allow longer than preferred width if header is longer
                        maxWidth = Math.Max(newColumnSize, DECIMAL_PREFERRED_WIDTH);
                    }
                    else
                    {
                        colStringCollection = gridTable.AsEnumerable()
                            .Select(row => row.Field<object>(i)?.ToString())
                            .Where(value => value is not null)!;
                    }

                    // Get the longest string in the array.
                    string longestColString = colStringCollection
                        .OrderByDescending((x) => x.Length).FirstOrDefault() ?? string.Empty;
                    newColumnSize = Math.Max(newColumnSize, MeasureStringWidth(gfx, longestColString, true));
                }

                this.Columns[i].Width = Math.Min(newColumnSize, maxWidth);
            }
        }

        private int MeasureStringWidth(Graphics gfx, string input, bool appendWhitespaceBuffer)
        {
            const string WHITESPACE_BUFFER = "#";
            try
            {
                var width = (int)gfx.MeasureString(input + (appendWhitespaceBuffer ? WHITESPACE_BUFFER : string.Empty), this.Font).Width;

                if (width <= 0) //happens with really long strings sometimes
                    return int.MaxValue;
                else
                    return width;
            }
            catch (Exception)
            {
                return int.MaxValue; //Assume worst case
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

        private void SetTheme()
        {
            if (this._gridTheme == Constants.LightModeTheme)
            {
                this.DefaultCellStyle = null;
                this.AlternatingRowsDefaultCellStyle = null;
                this.ColumnHeadersDefaultCellStyle = null;
                this.RowHeadersDefaultCellStyle = null;
                this.RowHeadersBorderStyle = Constants.LightModeTheme.RowHeaderBorderStyle;
                this.BackgroundColor = Constants.LightModeTheme.GridBackgroundColor;
                this.GridColor = Constants.LightModeTheme.GridColor;
            }
            else
            {
                this.DefaultCellStyle.BackColor = this._gridTheme.CellBackgroundColor;
                this.DefaultCellStyle.ForeColor = this._gridTheme.TextColor;

                this.AlternatingRowsDefaultCellStyle.BackColor = this._gridTheme.AlternateRowsCellBackgroundColor;
                this.AlternatingRowsDefaultCellStyle.ForeColor = this._gridTheme.TextColor;

                this.ColumnHeadersDefaultCellStyle.BackColor = this._gridTheme.ColumnHeaderColor;
                this.ColumnHeadersDefaultCellStyle.ForeColor = this._gridTheme.TextColor;

                this.RowHeadersDefaultCellStyle.BackColor = this._gridTheme.RowHeaderColor;
                this.RowHeadersDefaultCellStyle.ForeColor = this._gridTheme.TextColor;
                this.RowHeadersBorderStyle = this._gridTheme.RowHeaderBorderStyle;

                this.BackgroundColor = this._gridTheme.GridBackgroundColor;
                this.GridColor = this._gridTheme.GridColor;
                
            }

            //This is the default font winforms uses. We need to set it explicitly here otherwise it doesn't work.
            this.ColumnHeadersDefaultCellStyle!.Font = new Font("Segoe UI Semibold", this.Font.SizeInPoints, FontStyle.Bold);
        }
    }
}
