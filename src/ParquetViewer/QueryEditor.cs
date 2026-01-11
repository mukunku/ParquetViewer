using DuckDB.NET.Data;
using ParquetViewer.Controls;
using ParquetViewer.Engine;
using ParquetViewer.Engine.Types;
using ParquetViewer.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.ComponentModel;

namespace ParquetViewer
{
    public partial class QueryEditor : FormBase
    {
        private const string QUERY_FORMAT =
@"SELECT {0}
FROM {1}
LIMIT {2}
OFFSET {3};";


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

            SetZoom(AppSettings.QueryEditorZoomLevel);
        }

        private async void executeQueryButton_Click(object sender, EventArgs e)
        {
            this.executeQueryButton.Enabled = false;
            this.Cursor = Cursors.WaitCursor;
            this.queryExecutionStatusLabel.Visible = true;
            this.timeElapsedLabel.Visible = true;
            this.queryExecutionStatusLabel.Text = "Running:";
            this.timeElapsedLabel.Text = "00:00";

            var result = new DataTable();

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

                this.queryExecutionStatusLabel.Text = "Finished in:";
                this.timeElapsedLabel.Text = stopwatch.Elapsed.ToString("mm\\:ss");
            }
            catch (Exception ex)
            {
                result.Dispose();
                MessageBox.Show($"{ex.Message}", "Error executing query", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.executeQueryButton.Enabled = true;
                this.Cursor = Cursors.Default;
                return;
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
                MessageBox.Show($"{ex.Message}{Environment.NewLine}{Environment.NewLine}{ex.StackTrace}", 
                    "Error rendering results", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.executeQueryButton.Enabled = true;
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
            this.zoomPercentageDropDown.Text = $"Query Zoom: {zoomPercentage}%";
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
                e.Graphics.FillRectangle(Brushes.Silver, rectangle);
            }
        }

        private DataTable ConvertValues(DataTable intermediateResult)
        {
            var hasComplexType = false;
            foreach (DataColumn column in intermediateResult.Columns)
            {
                if (column.DataType.ImplementsInterface<System.Collections.IDictionary>()
                    || column.DataType.ImplementsInterface<System.Collections.IList>())
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
                    result.Columns.Add(new DataColumn(column.ColumnName, typeof(QueryResultStructValue)));
                }
                else if (column.DataType.ImplementsInterface<IList>())
                {
                    result.Columns.Add(new DataColumn(column.ColumnName, typeof(QueryResultListValue)));
                }
                else if (column.DataType.ImplementsInterface<IDictionary>())
                {
                    result.Columns.Add(new DataColumn(column.ColumnName, typeof(QueryResultMapValue)));
                }
                else if (column.DataType == typeof(byte[]))
                {
                    result.Columns.Add(new DataColumn(column.ColumnName, typeof(QueryResultByteArrayValue)));
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
                    structDictionary.Values.Select(v => v is null ? DBNull.Value : ConvertValue(v)).ToArray());

                var structValue = new QueryResultStructValue(dataRow);
                return structValue;
            }
            else if (value is IList list)
            {
                if (!list.GetType().IsGenericType)
                    throw new InvalidDataException("List was not generic as expected.");

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

                var listValue = new QueryResultListValue(arrayList, listType);
                return listValue;
            }
            else if (value is IDictionary dictionary)
            {
                var types = dictionary.GetType().GetGenericArguments();
                var keyType = types[0];
                var valueType = types[1];
                var mapValue = new QueryResultMapValue(
                    new ArrayList(dictionary.Keys), keyType,
                    new ArrayList(dictionary.Values), valueType);
                return mapValue;
            }
            else if (value is byte[] byteArray)
            {
                var byteArrayValue = new QueryResultByteArrayValue(byteArray);
                return byteArrayValue;
            }
            else
            {
                return value;
            }
        }

        internal class QueryResultStructValue : StructValueBase
        {
            public QueryResultStructValue(IDataRowLite data) : base(data)
            {
            }
        }

        internal class QueryResultListValue : ListValueBase
        {
            public QueryResultListValue(ArrayList data, Type type) : base(data, type)
            {
            }
        }

        internal class QueryResultMapValue : MapValueBase
        {
            public QueryResultMapValue(ArrayList keys, Type keyType, ArrayList values, Type valueType)
                : base(keys, keyType, values, valueType)
            {
            }
        }

        internal class QueryResultByteArrayValue : ByteArrayContent
        {
            public QueryResultByteArrayValue(byte[] content) : base(content)
            {
            }
        }

        internal class QueryResultDataRow : IDataRowLite
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
    }
}
