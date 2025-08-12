using ParquetViewer.Analytics;
using ParquetViewer.Engine;
using ParquetViewer.Engine.Types;
using ParquetViewer.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace ParquetViewer.Controls
{
    internal class ParquetGridView : DataGridView
    {
        //Actual number is around 43k (https://stackoverflow.com/q/52792876/1458738)
        //But let's use something smaller to increase rendering performance.
        public const int MAX_CHARACTERS_THAT_CAN_BE_RENDERED_IN_A_CELL = 2000;

        private Theme _gridTheme = Theme.LightModeTheme;
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

        public Image? CopyToClipboardIcon { get; set; } = null;
        public Image? CopyAsWhereIcon { get; set; } = null;
        public bool ShowCopyAsWhereContextMenuItem { get; set; } = false;

        private readonly HashSet<int> clickableColumnIndexes = new();
        private readonly Dictionary<(int, int), QuickPeekForm> openQuickPeekForms = new();
        private bool isCopyingToClipboard = false;
        private DataGridViewCellStyle? hyperlinkCellStyleCache;
        private bool isLeftClickButtonDown = false;
        private ContextMenuStrip? _contextMenu = null;
        private static readonly Regex _validColumnNameRegex = new Regex("^[a-zA-Z0-9_]+$");

        //We track format overrides by name+type so if the user adds/removes fields we can still continue formatting them the same.
        //There's a chance the user will load a different file containing columns with the same name+type but it wouldn't be so bad if we formatted those as well.
        private readonly Dictionary<(string ColumnName, Type ValueType), ByteArrayValue.DisplayFormat> byteArrayColumnsWithFormatOverrides = new();
        private readonly Dictionary<(string ColumnName, Type ValueType), FloatDisplayFormat> floatColumnsWithFormatOverrides = new();

        public ParquetGridView() : base()
        {
            DoubleBuffered = true; //Set DGV to be double buffered for smoother loading and scrolling
            AllowUserToAddRows = false;
            AllowUserToDeleteRows = false;
            AllowUserToOrderColumns = true;
            ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
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
            this.clickableColumnIndexes.Clear();
            base.OnDataSourceChanged(e); //This runs OnColumnAdded() for all columns before continuing.

            UpdateDateFormats();
            SetColumnCellStyles();
            AutoSizeColumns();
        }

        private void SetColumnCellStyles()
        {
            this.hyperlinkCellStyleCache = null;
            foreach (DataGridViewColumn column in this.Columns)
            {
                //Handle NULLs for bool types
                if (column is DataGridViewCheckBoxColumn checkboxColumn)
                {
                    checkboxColumn.ThreeState = true;
                }
                else if (column.ValueType == typeof(ListValue)
                    || column.ValueType == typeof(MapValue)
                    || column.ValueType == typeof(StructValue))
                {
                    column.DefaultCellStyle = GetHyperlinkCellStyle(column);
                }
                else if (column.ValueType == typeof(ByteArrayValue))
                {
                    //Check if this column contains images
                    for (var i = 0; i < this.Rows.Count; i++)
                    {
                        var cellValue = this[column.Index, i].Value;
                        if (cellValue != DBNull.Value)
                        {
                            var isImage = ((ByteArrayValue)cellValue).ToImage(out _);
                            if (isImage)
                            {
                                column.DefaultCellStyle = GetHyperlinkCellStyle(column);
                            }
                            break;
                        }
                    }
                }
            }
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
            if (e.RowIndex == -1 && e.ColumnIndex >= 0)
            {
                //Draw a star '*' next to column headers that are using a non-default display format
                var key = (this.Columns[e.ColumnIndex].Name, this.Columns[e.ColumnIndex].ValueType);
                if ((this.byteArrayColumnsWithFormatOverrides.TryGetValue(key, out var dateFormat) && dateFormat != default)
                    || (this.floatColumnsWithFormatOverrides.TryGetValue(key, out var floatFormat) && floatFormat != default))
                {
                    e.PaintBackground(e.CellBounds, true);
                    e.PaintContent(e.CellBounds);

                    WidenColumnForIndicator(this.Columns[e.ColumnIndex], e.Graphics!, e.CellStyle!.Font, false);
                    var length = MeasureStringWidth(e.Graphics!, e.CellStyle.Font, e.FormattedValue?.ToString() ?? string.Empty, false);
                    var drawPoint = new Point(e.CellBounds.Left + length - 2, e.CellBounds.Y + 4);
                    TextRenderer.DrawText(e.Graphics!, "*", e.CellStyle!.Font, drawPoint, e.CellStyle.ForeColor, TextFormatFlags.PreserveGraphicsClipping);

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

                    var font = new Font(e.CellStyle!.Font, FontStyle.Italic);
                    var color = this.GridTheme.CellPlaceholderTextColor;
                    if (e.State.HasFlag(DataGridViewElementStates.Selected))
                        color = Color.White;

                    TextRenderer.DrawText(e.Graphics!, "NULL", font, e.CellBounds, color,
                        TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.PreserveGraphicsClipping);

                    e.Handled = true;
                }
            }

            base.OnCellPainting(e); //Handle any additional event handlers
        }

        protected override void OnCellMouseMove(DataGridViewCellMouseEventArgs e)
        {
            base.OnCellMouseMove(e);
            if (e.RowIndex == -1 && e.ColumnIndex > -1 //cursor is hovering over column headers.
                && this.Cursor == Cursors.Default /*don't show hand if user is resizing columns for example*/)
            {
                //Since columns are sortable, show hand cursor on column headers
                this.Cursor = Cursors.Hand;
                return;
            }
            else if (e.ColumnIndex < 0 || e.RowIndex < 0)
            {
                return;
            }

            var isUserSelectingCells = this.isLeftClickButtonDown; //Don't show the hand cursor if the user is selecting cells
            if (!isUserSelectingCells && this.clickableColumnIndexes.Contains(e.ColumnIndex))
            {
                //Lets be fancy and only change the cursor if the user is hovering over the actual text in the cell
                if (IsCursorOverCellText(e.ColumnIndex, e.RowIndex))
                {
                    this.Cursor = Cursors.Hand;
                    return;
                }
            }
            this.Cursor = Cursors.Default;
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                int rowIndex = this.HitTest(e.X, e.Y).RowIndex;
                int columnIndex = this.HitTest(e.X, e.Y).ColumnIndex;

                if (rowIndex >= 0 && columnIndex >= 0)
                {
                    if (_contextMenu is null)
                    {
                        var copy = new ToolStripMenuItem("Copy", this.CopyToClipboardIcon);
                        copy.Click += (object? clickSender, EventArgs clickArgs) =>
                        {
                            this.CopySelectionToClipboard(false);
                        };

                        var copyWithHeaders = new ToolStripMenuItem("Copy with headers");
                        copyWithHeaders.Click += (object? clickSender, EventArgs clickArgs) =>
                        {
                            this.CopySelectionToClipboard(true);
                        };

                        var copyAsWhere = new ToolStripMenuItem("Copy as WHERE...", this.CopyAsWhereIcon);
                        copyAsWhere.Click += (object? clickSender, EventArgs clickArgs) =>
                        {
                            this.CopySelectionToClipboardAsWhereCondition();
                        };

                        _contextMenu = new ContextMenuStrip();
                        _contextMenu.Items.Add(copy);
                        _contextMenu.Items.Add(copyWithHeaders);

                        if (this.ShowCopyAsWhereContextMenuItem)
                        {
                            _contextMenu.Items.Add(copyAsWhere);
                        }
                    }

                    _contextMenu.Show(this, new Point(e.X, e.Y));
                }
            }

            base.OnMouseClick(e);
        }

        public void CloseContextMenu()
        {
            //HACK: For some reason calling Close() isn't working so we're forcing it via .Dispose()
            this._contextMenu?.Dispose();
            this._contextMenu = null;
        }

        protected override void OnColumnAdded(DataGridViewColumnEventArgs e)
        {
            //This will help avoid overflowing the sum(fillweight) of the grid's columns when there are too many of them.
            //The value of this field is not important as we do not use the FILL mode for column sizing.
            e.Column.FillWeight = 0.01f;

            //Checkbox columns aren't sortable by default for some reason.
            //https://stackoverflow.com/q/14979848/1458738
            e.Column.SortMode = DataGridViewColumnSortMode.Automatic;

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
                this.CopySelectionToClipboard(false);
                e.Handled = true;
            }
            base.OnKeyDown(e);
        }

        protected override void OnCellFormatting(DataGridViewCellFormattingEventArgs e)
        {
            base.OnCellFormatting(e);

            var cellValueType = this[e.ColumnIndex, e.RowIndex].ValueType;
            if (cellValueType == typeof(float) && e.Value is float f)
            {
                var key = (this.Columns[e.ColumnIndex].Name, this.Columns[e.ColumnIndex].ValueType);
                if (!this.floatColumnsWithFormatOverrides.TryGetValue(key, out var userSelectedDisplayFormat))
                    userSelectedDisplayFormat = default;

                if (userSelectedDisplayFormat == FloatDisplayFormat.Decimal)
                {
                    e.Value = f.ToDecimalString();
                    e.FormattingApplied = true;
                }
            }
            else if (cellValueType == typeof(double) && e.Value is double d)
            {
                var key = (this.Columns[e.ColumnIndex].Name, this.Columns[e.ColumnIndex].ValueType);
                if (!this.floatColumnsWithFormatOverrides.TryGetValue(key, out var userSelectedDisplayFormat))
                    userSelectedDisplayFormat = default;

                if (userSelectedDisplayFormat == FloatDisplayFormat.Decimal)
                {
                    e.Value = d.ToDecimalString();
                    e.FormattingApplied = true;
                }
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
            }

            if (cellValueType == typeof(ByteArrayValue) && e.Value is ByteArrayValue byteArrayValue)
            {
                //Don't truncate the binary data if this is a copy to clipboard operation
                int charLimit = this.isCopyingToClipboard ? int.MaxValue : MAX_CHARACTERS_THAT_CAN_BE_RENDERED_IN_A_CELL;

                //Figure out which format to show the binary data in
                var key = (this.Columns[e.ColumnIndex].Name, cellValueType);
                if (!this.byteArrayColumnsWithFormatOverrides.TryGetValue(key, out var userSelectedDisplayFormat))
                    userSelectedDisplayFormat = default;

                e.Value = FormatByteArrayString(byteArrayValue, userSelectedDisplayFormat, charLimit);
                e.FormattingApplied = true;
            }

            //In order to get full cell values into the clipboard during a copy to
            //clipboard operation we need to skip the truncation formatting below 
            var skipTruncation = this.isCopyingToClipboard
                || e.FormattingApplied; //Also exit early if we already formatted the value above

            if (skipTruncation)
            {
                return;
            }

            if (cellValueType == typeof(string))
            {
                string value = e.Value!.ToString()!;
                if (value.Length > MAX_CHARACTERS_THAT_CAN_BE_RENDERED_IN_A_CELL)
                {
                    e.Value = value[..MAX_CHARACTERS_THAT_CAN_BE_RENDERED_IN_A_CELL] + "[...]";
                    e.FormattingApplied = true;
                }
            }
            else if (cellValueType == typeof(StructValue) && e.Value is StructValue structValue)
            {
                e.Value = structValue.ToStringTruncated(MAX_CHARACTERS_THAT_CAN_BE_RENDERED_IN_A_CELL);
                e.FormattingApplied = true;
            }
        }

        protected override void OnSorted(EventArgs e)
        {
            using var graphics = this.CreateGraphics();
            WidenColumnForIndicator(this.SortedColumn, graphics, this.Font, true);
            base.OnSorted(e);
        }

        private static void WidenColumnForIndicator(DataGridViewColumn column, Graphics graphics, Font font, bool includeWhitespaceBuffer)
        {
            if (!(column.Tag is string tag && tag.Equals("WIDENED")))
            {
                var headerLength = MeasureStringWidth(graphics, font, column.Name, includeWhitespaceBuffer);
                var columnWidth = column.Width;

                //Widen the column a bit so the sorting arrow can be shown.
                var whitespaceWidth = columnWidth - headerLength;
                if (whitespaceWidth >= 0 && whitespaceWidth < 21)
                {
                    column.Width += 21 - whitespaceWidth;
                }

                //Don't widen the same column twice (this shouldn't be needed but I don't trust the logic above)
                column.Tag = "WIDENED";
            }
        }

        protected override void OnColumnHeaderMouseClick(DataGridViewCellMouseEventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                base.OnColumnHeaderMouseClick(e); //This will trigger the sort operation and the OnSorted event if it's a left-click

                if (e.Button == MouseButtons.Right)
                {
                    ShowDisplayFormatOptions(e.ColumnIndex);
                }
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                this.isLeftClickButtonDown = true;

            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                this.isLeftClickButtonDown = false;

            base.OnMouseUp(e);
        }

        public void ClearQuickPeekForms()
        {
            foreach (var form in this.openQuickPeekForms)
            {
                try
                {
                    form.Value.Close();
                    form.Value.Dispose();
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
                var newColumnSize = MeasureStringWidth(gfx, this.Font, columnNameOrNull, true);

                if (gridTable.Columns[i].DataType == typeof(DateTime))
                {
                    //All date time's will have the same string length so no need to go through all values.
                    //We can just measure one and use that.
                    var dateTime = gridTable.AsEnumerable()
                        .FirstOrDefault(row => row[i] != DBNull.Value)?
                        .Field<DateTime>(i);
                    if (dateTime is not null)
                    {
                        string formattedDateTimeValue = dateTime.Value.ToString(AppSettings.DateTimeDisplayFormat.GetDateFormat());
                        newColumnSize = Math.Max(newColumnSize, MeasureStringWidth(gfx, this.Font, formattedDateTimeValue, false));
                    }
                }
                else
                {
                    // Collect all the rows into a string enumerable, making sure to exclude null values.
                    IEnumerable<string> colStringCollection;
                    if (gridTable.Columns[i].DataType == typeof(StructValue))
                    {
                        colStringCollection = gridTable.AsEnumerable()
                            .Select(row => row.Field<StructValue>(i)?.ToStringTruncated(MAX_CHARACTERS_THAT_CAN_BE_RENDERED_IN_A_CELL))
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
                    newColumnSize = Math.Max(newColumnSize, MeasureStringWidth(gfx, this.Font, longestColString, true));
                }

                this.Columns[i].Width = Math.Min(newColumnSize, maxWidth);
            }
        }

        private static int MeasureStringWidth(Graphics gfx, Font font, string input, bool appendWhitespaceBuffer)
        {
            const string WHITESPACE_BUFFER = "#";
            try
            {
                var width = (int)gfx.MeasureString(input + (appendWhitespaceBuffer ? WHITESPACE_BUFFER : string.Empty), font).Width;

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

        private void CopySelectionToClipboard(bool withHeaders)
        {
            this.isCopyingToClipboard = true;
            if (withHeaders)
            {
                this.RowHeadersVisible = false; //disable row headers temporarily so they don't end up in the clipboard content
                this.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
            }
            Clipboard.SetDataObject(this.GetClipboardContent(), true, 2, 250); //Without setting `copy` to true, this call can cause a UI thread deadlock somehow...
            if (withHeaders)
            {
                this.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
                this.RowHeadersVisible = true;
            }
            this.isCopyingToClipboard = false;
        }

        /// <remarks>
        /// Microsoft recommends using a shared cell style for best performance:
        /// https://learn.microsoft.com/en-us/dotnet/desktop/winforms/controls/best-practices-for-scaling-the-windows-forms-datagridview-control?view=netframeworkdesktop-4.8#using-cell-styles-efficiently
        /// </remarks>
        private DataGridViewCellStyle GetHyperlinkCellStyle(DataGridViewColumn column)
        {
            this.clickableColumnIndexes.Add(column.Index);
            this.hyperlinkCellStyleCache ??= new DataGridViewCellStyle(column.DefaultCellStyle)
            {
                Font = new(column.DefaultCellStyle.Font ?? column.InheritedStyle.Font, FontStyle.Underline),
                ForeColor = this.GridTheme.HyperlinkColor
            };
            return this.hyperlinkCellStyleCache;
        }

        private void SetTheme()
        {
            this.DefaultCellStyle.BackColor = this.GridTheme.CellBackgroundColor;
            this.DefaultCellStyle.ForeColor = this.GridTheme.TextColor;
            this.DefaultCellStyle.SelectionBackColor = this.GridTheme.SelectionBackColor;

            this.ColumnHeadersDefaultCellStyle.BackColor = this.GridTheme.ColumnHeaderColor;
            this.ColumnHeadersDefaultCellStyle.ForeColor = this.GridTheme.TextColor;

            this.RowHeadersDefaultCellStyle.BackColor = this.GridTheme.RowHeaderColor;
            this.RowHeadersDefaultCellStyle.ForeColor = this.GridTheme.TextColor;
            this.RowHeadersDefaultCellStyle.SelectionBackColor = this.GridTheme.SelectionBackColor;
            this.RowHeadersBorderStyle = this.GridTheme.RowHeaderBorderStyle;

            this.BackgroundColor = this.GridTheme.GridBackgroundColor;
            this.GridColor = this.GridTheme.GridColor;

            this.ColumnHeadersDefaultCellStyle = new()
            {
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                BackColor = this.GridTheme.ColumnHeaderColor,
                Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point),
                ForeColor = this.GridTheme.TextColor,
                SelectionBackColor = SystemColors.Highlight,
                SelectionForeColor = SystemColors.HighlightText,
                WrapMode = DataGridViewTriState.True
            };

            SetColumnCellStyles();
        }

        protected override void OnDataError(bool displayErrorDialogIfNoHandler, DataGridViewDataErrorEventArgs e)
        {
            if (this.ReadOnly)
            {
                //Since we don't allow editing just ignore errors and hope for the best.
                return;
            }

            base.OnDataError(displayErrorDialogIfNoHandler, e);
        }

        private void CopySelectionToClipboardAsWhereCondition()
        {
            var columnsAndValuesToFilterBy = new List<(string ColumnName, Type ValueType, object[] Values)>();
            foreach (var selectedCellsByColumn in
                this.SelectedCells.AsEnumerable()
                .GroupBy(cell => cell.ColumnIndex)
                .OrderBy(column => column.Key))
            {
                DataTable? dataTable = this.DataSource as DataTable;
                var cellValues = selectedCellsByColumn.Select(cell => this[cell.ColumnIndex, cell.RowIndex].Value);
                var columnIndex = selectedCellsByColumn.Key;
                var column = this.Columns[columnIndex];
                columnsAndValuesToFilterBy.Add((column.Name, column.ValueType, cellValues.ToArray()));
            }

            var filterQuery = GenerateFilterQuery(columnsAndValuesToFilterBy);
            if (filterQuery.Length < new TextBox().MaxLength) //This length check doesn't make the most sense but I wanted to put some kind of cap on this.
            {
                Clipboard.SetText(filterQuery, TextDataFormat.Text);
            }
            else
            {
                MessageBox.Show("The selected data is too large. Please select less cells.",
                    "Copy to clipboard failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static string GenerateFilterQuery(string columnName, Type valueType, object value)
            => GenerateFilterQuery(new() { (columnName, valueType, [value]) });

        public static string GenerateFilterQuery(List<(string ColumnName, Type ValueType, object[] Values)> columnsAndValuesToFilterBy)
        {
            var queryBuilder = new StringBuilder();
            if (columnsAndValuesToFilterBy is null || columnsAndValuesToFilterBy.Count == 0)
            {
                return queryBuilder.ToString();
            }

            for (var columnIndex = 0; columnIndex < columnsAndValuesToFilterBy.Count; columnIndex++)
            {
                var (columnName, valueType, values) = columnsAndValuesToFilterBy[columnIndex];

                ArgumentException.ThrowIfNullOrWhiteSpace(columnName);
                ArgumentNullException.ThrowIfNull(values);

                //Wrap column name in brackets if it contains spaces or punctuation (if it isn't wrapped already)
                var isAlreadyWrapped = columnName.StartsWith("[") && columnName.EndsWith("]");
                if (!isAlreadyWrapped && !_validColumnNameRegex.IsMatch(columnName))
                {
                    columnName = $"[{columnName}]";
                }

                var hasNulls = values.Any(value => value is null || value == DBNull.Value);
                values = values
                    .Where(value => value != DBNull.Value)
                    .Distinct() //Distinct() doesn't work if there are any DBNull's in the collection
                    .AppendIf(hasNulls, DBNull.Value) //Add one DBNull back if required
                    .Order()
                    .ToArray();

                for (var valueIndex = 0; valueIndex < values.Length; valueIndex++)
                {
                    var value = values[valueIndex];
                    if (valueIndex == 0)
                    {
                        if (columnIndex > 0)
                        {
                            queryBuilder.Append(" AND ");
                        }

                        queryBuilder.Append(columnName);
                        if (values.Length == 1)
                        {
                            if (value == DBNull.Value)
                            {
                                queryBuilder.Append(" IS NULL");
                                break;
                            }

                            queryBuilder.Append(" = ");
                        }
                        else
                        {
                            queryBuilder.Append(" IN (");
                        }
                    }
                    else
                    {
                        queryBuilder.Append(",");
                    }

                    if (valueType == typeof(DateTime))
                    {
                        //Use a standard date format so the query is always syntactically correct
                        queryBuilder.Append($"#{((DateTime)value).ToString("yyyy-MM-dd HH:mm:ss.FFFFFFF")}#");
                    }
                    else if (valueType.IsNumber())
                    {
                        var stringValue = value.ToString();
                        if ((valueType == typeof(float) || valueType == typeof(double))
                            && stringValue?.Contains('E', StringComparison.OrdinalIgnoreCase) == true)
                            stringValue = $"'{stringValue}'"; //scientific notation values need to be wrapped in single quotes

                        queryBuilder.Append(stringValue);
                    }
                    else
                    {
                        queryBuilder.Append($"'{value}'");
                    }

                    //Close the `IN (` parenthesis if required
                    if (valueIndex == values.Length - 1 && values.Length > 1)
                    {
                        queryBuilder.Append(")");
                    }
                }
            }

            return queryBuilder.ToString();
        }

        private void ShowDisplayFormatOptions(int columnIndex)
        {
            //If this is a byte array column, show available formatting options
            if (this.Columns[columnIndex].ValueType == typeof(ByteArrayValue))
            {
                const int RECORDS_TO_INTERSECT_COUNT = 8;

                //Find a few different non-null values and find the common display formats that all of them support.
                //This will reduce the chance the user sees #ERR in the cells from bad formatting conversions.
                int intersectCounter = RECORDS_TO_INTERSECT_COUNT;
                IEnumerable<ByteArrayValue.DisplayFormat> possibleDisplayFormats = Enum.GetValues<ByteArrayValue.DisplayFormat>();
                for (var i = 0; i < this.RowCount; i++)
                {
                    if (this[columnIndex, i].Value is not ByteArrayValue byteArrayValue)
                        continue;

                    possibleDisplayFormats = possibleDisplayFormats.Intersect(byteArrayValue.PossibleDisplayFormats);
                    intersectCounter--;

                    if (intersectCounter <= 0)
                        break;
                }

                if (intersectCounter == RECORDS_TO_INTERSECT_COUNT)
                {
                    //Most likely that all values are null. Just show the default option
                    possibleDisplayFormats = [default];
                }

                var columnHeaderContextMenu = new ContextMenuStrip();
                foreach (var supportedFormat in possibleDisplayFormats)
                {
                    var key = (this.Columns[columnIndex].Name, this.Columns[columnIndex].ValueType);

                    var toolstripMenuItem = new ToolStripMenuItem(supportedFormat.ToString());
                    toolstripMenuItem.Click += (object? _, EventArgs _) =>
                    {
                        if (byteArrayColumnsWithFormatOverrides.ContainsKey(key))
                            byteArrayColumnsWithFormatOverrides[key] = supportedFormat;
                        else
                            byteArrayColumnsWithFormatOverrides.Add(key, supportedFormat);

                        this.Refresh(); //Force a re-draw to render updated format
                    };
                    columnHeaderContextMenu.Items.Add(toolstripMenuItem);

                    if (!byteArrayColumnsWithFormatOverrides.TryGetValue(key, out var displayFormat))
                        displayFormat = default;

                    toolstripMenuItem.Checked = displayFormat == supportedFormat;
                }
                columnHeaderContextMenu.Show(Cursor.Position);
            }
            else if (this.Columns[columnIndex].ValueType == typeof(float) || this.Columns[columnIndex].ValueType == typeof(double))
            {
                var columnHeaderContextMenu = new ContextMenuStrip();

                var key = (this.Columns[columnIndex].Name, this.Columns[columnIndex].ValueType);
                if (!floatColumnsWithFormatOverrides.TryGetValue(key, out var displayFormat))
                    displayFormat = default;

                var scientificNotationMenuItem = new ToolStripMenuItem("Scientific")
                { Checked = displayFormat == FloatDisplayFormat.Scientific };
                scientificNotationMenuItem.Click += (object? _, EventArgs _) =>
                {
                    if (floatColumnsWithFormatOverrides.ContainsKey(key))
                        floatColumnsWithFormatOverrides[key] = FloatDisplayFormat.Scientific;
                    else
                        floatColumnsWithFormatOverrides.Add(key, FloatDisplayFormat.Scientific);

                    this.Refresh(); //Force a re-draw to render updated format
                };
                columnHeaderContextMenu.Items.Add(scientificNotationMenuItem);

                var decimalNotationMenuItem = new ToolStripMenuItem("Decimal")
                { Checked = displayFormat == FloatDisplayFormat.Decimal };
                decimalNotationMenuItem.Click += (object? _, EventArgs _) =>
                {
                    if (floatColumnsWithFormatOverrides.ContainsKey(key))
                        floatColumnsWithFormatOverrides[key] = FloatDisplayFormat.Decimal;
                    else
                        floatColumnsWithFormatOverrides.Add(key, FloatDisplayFormat.Decimal);

                    this.Refresh(); //Force a re-draw to render updated format
                };
                columnHeaderContextMenu.Items.Add(decimalNotationMenuItem);

                columnHeaderContextMenu.Show(Cursor.Position);
            }
        }

        /// <summary>
        /// Gets the string representation of the binary data in the desired format
        /// </summary>
        /// <param name="desiredFormat">How to interpret the binary data</param>
        /// <param name="desiredLength">An optional maximum string length target to try and achieve (NOT GUARANTEED)</param>
        /// <returns>String representation of the binary data in the desired format if possible.
        /// If conversion fails, <see cref="FORMATTING_ERROR_TEXT"/> is returned instead</returns>
        /// <remarks>Utilize <see cref="ByteArrayValue.PossibleDisplayFormats"/> to avoid calling incompatible conversions</remarks>
        private static string FormatByteArrayString(ByteArrayValue byteArrayValue, ByteArrayValue.DisplayFormat desiredFormat, int desiredLength = int.MaxValue)
        {
            const string FORMATTING_ERROR_TEXT = "#ERR";
            ArgumentNullException.ThrowIfNull(byteArrayValue);
            ArgumentOutOfRangeException.ThrowIfLessThan(desiredLength, 1);

            if (desiredFormat == ByteArrayValue.DisplayFormat.IPv4)
            {
                if (byteArrayValue.ToIPv4(out var ipAddress))
                {
                    return ipAddress.ToString();
                }

                return FORMATTING_ERROR_TEXT;
            }
            else if (desiredFormat == ByteArrayValue.DisplayFormat.IPv6)
            {
                if (byteArrayValue.ToIPv6(out var ipAddress))
                {
                    return ipAddress.ToString();
                }

                return FORMATTING_ERROR_TEXT;
            }
            else if (desiredFormat == ByteArrayValue.DisplayFormat.Guid)
            {
                if (byteArrayValue.ToGuid(out var @guid))
                {
                    return @guid.Value.ToString();
                }

                return FORMATTING_ERROR_TEXT;
            }
            else if (desiredFormat == ByteArrayValue.DisplayFormat.Short)
            {
                if (byteArrayValue.ToShort(out var @short))
                {
                    return @short.Value.ToString();
                }

                return FORMATTING_ERROR_TEXT;
            }
            else if (desiredFormat == ByteArrayValue.DisplayFormat.Integer)
            {
                if (byteArrayValue.ToInteger(out var @int))
                {
                    return @int.Value.ToString();
                }

                return FORMATTING_ERROR_TEXT;
            }
            else if (desiredFormat == ByteArrayValue.DisplayFormat.Long)
            {
                if (byteArrayValue.ToLong(out var @long))
                {
                    return @long.Value.ToString();
                }

                return FORMATTING_ERROR_TEXT;
            }
            else if (desiredFormat == ByteArrayValue.DisplayFormat.Float)
            {
                if (byteArrayValue.ToFloat(out var @float))
                {
                    return @float.Value.ToString();
                }

                return FORMATTING_ERROR_TEXT;
            }
            else if (desiredFormat == ByteArrayValue.DisplayFormat.Double)
            {
                if (byteArrayValue.ToDouble(out var @double))
                {
                    return @double.Value.ToString();
                }

                return FORMATTING_ERROR_TEXT;
            }
            else if (desiredFormat == ByteArrayValue.DisplayFormat.ASCII)
            {
                if (byteArrayValue.ToASCII(out var ascii))
                {
                    if (ascii.Length <= desiredLength)
                        return ascii;

                    return ascii[..desiredLength] + "[...]";
                }

                return FORMATTING_ERROR_TEXT;
            }
            else if (desiredFormat == ByteArrayValue.DisplayFormat.Base64)
            {
                byteArrayValue.ToBase64(out var base64);
                if (base64.Length <= desiredLength)
                    return base64;

                return base64[..desiredLength] + "[...]";
            }
            else if (desiredFormat == ByteArrayValue.DisplayFormat.Size)
            {
                return byteArrayValue.Data.Length.ToString() + (byteArrayValue.Data.Length == 1 ? " byte" : " bytes");
            }
            else
            {
                return byteArrayValue.ToStringTruncated(desiredLength);
            }
        }

        private enum FloatDisplayFormat
        {
            Scientific = 0,
            Decimal
        }
    }
}
