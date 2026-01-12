using DuckDB.NET.Data;
using ParquetViewer.Analytics;
using ParquetViewer.Controls;
using ParquetViewer.Engine;
using ParquetViewer.Engine.Exceptions;
using ParquetViewer.Engine.Types;
using ParquetViewer.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ParquetViewer
{
    public partial class QueryEditor : FormBase
    {
        private const string QUERY_FORMAT =
@"SELECT {0}
FROM {1}
LIMIT {2}
OFFSET {3} ";

        private Brush _splitterColor = Brushes.Silver;
        private bool _wasByteArrayConversionErrorShown = false;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string QueryText
        {
            get => this.queryRichTextBox.Text;
            set => this.queryRichTextBox.Text = value;
        }

        public QueryEditor()
        {
            InitializeComponent();
            this.mainSplitContainer.Paint += MainSplitContainer_Paint;
            this.resultsGridView.ShowCopyAsWhereContextMenuItem = true;
            this.resultsGridView.CopyAsWhereIcon = Resources.Icons.sql_server_icon.ToBitmap();
            this.resultsGridView.ColumnNameEscapeFormat = "\"{0}\"";
            this.resultsGridView.DateValueEscapeFormat = "'{0}'";
        }

        public QueryEditor(IEnumerable<string>? fields = null, string? filePath = null, int offset = 0, int limit = 1000) : this()
        {
            var filePathSpecified = filePath is not null;
            filePath ??= Path.Combine(Path.GetDirectoryName(Application.ExecutablePath) ?? string.Empty, "your-file.parquet");

            var queryFields = fields is not null ? string.Join(',', fields.Select(f => $"\"{f}\"")) : "*";
            var fromClause = filePath?.EndsWith(".parquet") == true ? $"'{filePath}'" : $"read_parquet('{filePath}')";
            this.queryRichTextBox.Text = QUERY_FORMAT.Format(queryFields, fromClause, limit, offset);

            if (!filePathSpecified)
                this.executeQueryButton.Enabled = false; //start off disabled, so the user has to adjust the query
        }

        private void QueryEditor_Load(object sender, EventArgs e)
        {
            this.resultsGridView.AutoGenerateColumns = true;
            this.queryExecutionStatusLabel.Visible = false;
            this.timeElapsedLabel.Visible = false;

            //Set caret to the end of the text
            this.queryRichTextBox.SelectionStart = this.queryRichTextBox.Text.Length;
            this.queryRichTextBox.SelectionLength = 0;

            SetZoom(AppSettings.QueryEditorZoomLevel);
        }

        private async void executeQueryButton_Click(object sender, EventArgs e)
        {
            this.executeQueryButton.Enabled = false;
            this.Cursor = Cursors.WaitCursor;
            this.queryExecutionStatusLabel.Visible = true;
            this.timeElapsedLabel.Visible = true;
            this.queryExecutionStatusLabel.Text = Resources.Strings.QueryRunningStatusText;
            this.timeElapsedLabel.Text = "00:00";
            this.resultsGridView.Enabled = false;

            var result = new DataTable();
            var queryEvent = new ExecuteQueryEvent() { IsDuckDB = true };
            try
            {
                var stopwatch = Stopwatch.StartNew();
                var query = this.queryRichTextBox.Text;
                var queryTask = Task.Run(() =>
                {
                    using var connection = new DuckDBConnection("Data Source=:memory:");
                    connection.Open();

                    using var command = connection.CreateCommand();
                    command.CommandText = query;

                    using var reader = command.ExecuteReader();
                    result.Load(reader);
                });

                while (!queryTask.IsCompleted)
                {
                    await Task.Delay(100);
                    this.timeElapsedLabel.Text = stopwatch.Elapsed.ToString("mm\\:ss");
                }

                stopwatch.Stop();
                await queryTask;

                this.queryExecutionStatusLabel.Text = Resources.Strings.QueryFinishedStatusText;
                this.timeElapsedLabel.Text = stopwatch.Elapsed.ToString("mm\\:ss");
                queryEvent.IsValid = true;
                queryEvent.RunTimeMS = stopwatch.ElapsedMilliseconds;
                queryEvent.RecordCountFiltered = result.Rows.Count;
                queryEvent.ColumnCount = result.Columns.Count;
            }
            catch (Exception ex)
            {
                result.Dispose();
                if (ex is InvalidCastException && ex.Message.Contains("The list contains null value"))
                {
                    MessageBox.Show(Resources.Errors.ListsWithNullsErrorMessage, Resources.Errors.ListsWithNullsErrorTitle, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else if (ex is DuckDBException && ex.Message.StartsWith("Parser Error:"))
                {
                    MessageBox.Show($"{Resources.Errors.InvalidQueryErrorMessage}{Environment.NewLine}{Environment.NewLine}{ex.Message}",
                        Resources.Errors.InvalidQueryErrorTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else if (ex is OverflowException && ex.Message.Contains("Value was either too large or too small for a Decimal"))
                {
                    MessageBox.Show(Resources.Errors.DecimalValueUnknownSizeTooLargeErrorMessageFormat
                   .Format(null, null, null,
                       DecimalOverflowException.MAX_DECIMAL_PRECISION,
                       DecimalOverflowException.MAX_DECIMAL_SCALE),
                   Resources.Errors.DecimalValueTooLargeErrorTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    ExceptionEvent.FireAndForget(ex);
                    MessageBox.Show(ex.Message, Resources.Errors.QueryExecutionErrorTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                this.executeQueryButton.Enabled = true;
                this.resultsGridView.Enabled = true;
                this.Cursor = Cursors.Default;
                return;
            }
            finally
            {
                //Fire and forget
                var _ = queryEvent.Record();
            }

            //Cleanup previous results
            if (this.resultsGridView.DataSource is DataTable dt)
            {
                dt.Dispose();
            }

            try
            {
                this.resultsGridView.DataSource = ConvertValues(result);
            }
            catch (Exception ex)
            {
                ExceptionEvent.FireAndForget(ex);
                MessageBox.Show($"{ex.Message}{Environment.NewLine}{Environment.NewLine}{ex.StackTrace}",
                    Resources.Errors.RenderResultsErrorTitle, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.executeQueryButton.Enabled = true;
                this.resultsGridView.Enabled = true;
                this.Cursor = Cursors.Default;
            }
        }

        private void queryRichTextBox_TextChanged(object sender, FastColoredTextBoxNS.TextChangedEventArgs e)
        {
            this.executeQueryButton.Enabled = true;
        }

        private void zoomPercentage_Click(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem menuItem && int.TryParse(menuItem.Tag?.ToString(), out int zoomPercentage))
            {
                SetZoom(zoomPercentage);
            }
        }

        private void SetZoom(int? zoomPercentage)
        {
            zoomPercentage = Math.Clamp(zoomPercentage ?? 100, 100, 150);
            this.queryRichTextBox.Zoom = zoomPercentage.Value;
            this.zoomPercentageDropDown.Text = Resources.Strings.QueryZoomStatusTextFormat.Format(zoomPercentage);
            AppSettings.QueryEditorZoomLevel = zoomPercentage.Value;

            foreach (ToolStripMenuItem menuItem in this.zoomPercentageDropDown.DropDownItems)
            {
                menuItem.Checked = int.TryParse(menuItem.Tag?.ToString(), out int percentage) && percentage == zoomPercentage.Value;
            }
        }

        private void querySyntaxDocsButton_Click(object sender, EventArgs e)
        {
            Process.Start(new ProcessStartInfo(Constants.DuckDBSqlSyntaxURL) { UseShellExecute = true });
        }

        private void queryRichTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5 || (e.Shift && e.KeyCode == Keys.Enter))
            {
                executeQueryButton.PerformClick();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void QueryEditor_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyValue == (int)Keys.Escape)
            {
                this.Close();
                e.Handled = true;
            }
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void MainSplitContainer_Paint(object? sender, PaintEventArgs e)
        {
            //Draw a line where the splitter is so users can tell they can resize the sections
            var splitContainer = sender as SplitContainer;

            if (splitContainer != null)
            {
                var rectangle = splitContainer.SplitterRectangle;
                rectangle.Offset(0, 1);
                rectangle.Height -= 2;
                e.Graphics.FillRectangle(_splitterColor, rectangle);
            }
        }

        private DataTable ConvertValues(DataTable intermediateResult)
        {
            var hasComplexType = false;
            foreach (DataColumn column in intermediateResult.Columns)
            {
                if (column.DataType.ImplementsInterface<IDictionary>()
                    || column.DataType.ImplementsInterface<IList>()
                    || column.DataType == typeof(Stream))
                {
                    hasComplexType = true;
                    break;
                }
            }

            if (!hasComplexType)
            {
                //Nothing to convert
                return intermediateResult;
            }

            var result = new DataTable();
            foreach (DataColumn column in intermediateResult.Columns)
            {
                if (column.DataType == typeof(Dictionary<string, object?>))
                {
                    result.Columns.Add(new DataColumn(column.ColumnName, typeof(StructValue)));
                }
                else if (column.DataType.ImplementsInterface<IList>())
                {
                    result.Columns.Add(new DataColumn(column.ColumnName, typeof(ListValue)));
                }
                else if (column.DataType.ImplementsInterface<IDictionary>())
                {
                    result.Columns.Add(new DataColumn(column.ColumnName, typeof(MapValue)));
                }
                else if (column.DataType == typeof(Stream))
                {
                    result.Columns.Add(new DataColumn(column.ColumnName, typeof(ByteArrayValue)));
                }
                else
                {
                    result.Columns.Add(new DataColumn(column.ColumnName, column.DataType));
                }
            }

            result.BeginLoadData();
            foreach (DataRow row in intermediateResult.Rows)
            {
                var newRow = result.NewRow();
                for (var i = 0; i < row.ItemArray.Length; i++)
                {
                    var value = row.ItemArray[i];
                    newRow[i] = ConvertValue(value!);
                }
                result.Rows.Add(newRow);
            }
            result.EndLoadData();

            return result;
        }

        private object ConvertValue(object? value)
        {
            if (value == DBNull.Value || value is null)
            {
                return DBNull.Value;
            }
            else if (value is Dictionary<string, object?> structDictionary)
            {
                var dataRow = new QueryResultDataRow(structDictionary.Keys.ToList(),
                    structDictionary.Values.Select(ConvertValue).ToArray());

                var structValue = new StructValue(dataRow);
                return structValue;
            }
            else if (value is IList list)
            {
                var arrayList = new ArrayList(list.Count);
                var listType = typeof(object);
                for (var i = 0; i < list.Count; i++)
                {
                    var convertedListValue = ConvertValue(list[i]);
                    arrayList.Add(convertedListValue);

                    if (convertedListValue != DBNull.Value)
                    {
                        listType = convertedListValue.GetType();
                    }
                }

                var listValue = new ListValue(arrayList, listType);
                return listValue;
            }
            else if (value is IDictionary dictionary)
            {
                var keysList = new ArrayList(dictionary.Keys.Count);
                var valuesList = new ArrayList(dictionary.Values.Count);
                var keysType = typeof(object);
                var valuesType = typeof(object);
                foreach (var keyValuePair in Engine.Helpers.PairEnumerables(
                    dictionary.Keys.OfType<object?>(),
                    dictionary.Values.OfType<object?>(),
                    DBNull.Value))
                {
                    var convertedKey = ConvertValue(keyValuePair.Item1);
                    var convertedValue = ConvertValue(keyValuePair.Item2);
                    keysList.Add(convertedKey);
                    valuesList.Add(convertedValue);
                    if (convertedKey != DBNull.Value)
                    {
                        keysType = convertedKey.GetType();
                    }
                    if (convertedValue != DBNull.Value)
                    {
                        valuesType = convertedValue.GetType();
                    }
                }
                var mapValue = new MapValue(
                    keysList, keysType,
                    valuesList, valuesType);
                return mapValue;
            }
            else if (value is Stream byteArray)
            {
                //DuckDB doesn't seem to like byte array values. It fails to read after the first row with a memory access violation error.
                if (!this._wasByteArrayConversionErrorShown)
                {
                    this._wasByteArrayConversionErrorShown = true;
                    MessageBox.Show(
                        Resources.Strings.ByteArraysNotSupportedErrorMessage,
                        Resources.Strings.ByteArraysNotSupportedErrorTitle,
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                return DBNull.Value;
                /*
                    using var ms = new MemoryStream();
                    byteArray.CopyTo(ms); //MemoryAccessViolation thrown here on second row and beyond
                    var byteArrayValue = new ByteArrayValue(ms.ToArray());
                    return byteArrayValue;
                */
            }
            else
            {
                return value;
            }
        }

        private class QueryResultDataRow : IDataRowLite
        {
            public IReadOnlyCollection<string> ColumnNames { get; }

            public object[] Row { get; }

            public QueryResultDataRow(List<string> columnNames, object[] data)
            {
                this.ColumnNames = columnNames.AsReadOnly();
                this.Row = data;
            }

            public object GetValue(string columnName)
            {
                var result = this.ColumnNames.Index().Where(pair => pair.Item == columnName).FirstOrDefault();
                if (result == default)
                    throw new ArgumentOutOfRangeException($"Column `{columnName}` doesn't exist");
                return this.Row[result.Index];
            }
        }

        public override void SetTheme(Theme theme)
        {
            if (DesignMode)
            {
                return;
            }

            base.SetTheme(theme);
            this.resultsGridView.GridTheme = theme;
            this.querySyntaxDocsButton.ForeColor = Color.Black;
            this.executeQueryButton.ForeColor = Color.Black;
            this.statusStrip.BackColor = theme.FormBackgroundColor;
            this.statusStrip.ForeColor = theme.TextColor;
            this.mainTableLayoutPanel.BackColor = theme.FormBackgroundColor;
            this._splitterColor = new SolidBrush(theme.DisabledTextColor);
            this.queryRichTextBox.IndentBackColor = theme.RowHeaderColor;
            this.queryRichTextBox.ForeColor = theme.TextColor;
            this.queryRichTextBox.SelectionColor = theme.SelectionBackColor;
            this.queryRichTextBox.CaretColor = theme.TextColor;

            if (theme == Theme.LightModeTheme)
            {
                this.queryRichTextBox.BackColor = Color.White;
            }
            else
            {
                this.queryRichTextBox.BackColor = theme.CellBackgroundColor;
            }

            this.statusStrip.Renderer = theme.ToolStripRenderer;
        }

        private void copyTextMenuItem_Click(object sender, EventArgs e)
        {
            this.queryRichTextBox.Copy();
        }

        private void pasteTextMenuItem_Click(object sender, EventArgs e)
        {
            this.queryRichTextBox.Paste();
        }
    }
}