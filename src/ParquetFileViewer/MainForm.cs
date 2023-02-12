using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Parquet;
using ParquetFileViewer.Helpers;

namespace ParquetFileViewer
{
    public partial class MainForm : Form
    {
        private const string WikiURL = "https://github.com/mukunku/ParquetViewer/wiki";
        private const int DefaultOffset = 0;
        private const int DefaultRowCountValue = 1000;
        private const int loadingPanelWidth = 200;
        private const int loadingPanelHeight = 200;
        private const string QueryUselessPartRegex = "^WHERE ";
        private const string DefaultTableName = "MY_TABLE";
        private readonly string DefaultFormTitle;

        #region Members
        private string fileToLoadOnLaunch = null;
        private string openFilePath;
        private string OpenFilePath
        {
            get => this.openFilePath;
            set
            {
                this.openFileSchema = null;
                this.SelectedFields = null;
                this.openFilePath = value;
                this.changeFieldsMenuStripButton.Enabled = false;
                this.getSQLCreateTableScriptToolStripMenuItem.Enabled = false;
                this.saveAsToolStripMenuItem.Enabled = false;
                this.metadataViewerToolStripMenuItem.Enabled = false;
                this.recordCountStatusBarLabel.Text = "0";
                this.totalRowCountStatusBarLabel.Text = "0";
                this.MainDataSource.Clear();
                this.MainDataSource.Columns.Clear();

                if (string.IsNullOrWhiteSpace(value))
                {
                    this.Text = this.DefaultFormTitle;
                }
                else
                {
                    this.Text = string.Concat($"File: ", value);
                    this.changeFieldsMenuStripButton.Enabled = true;
                    this.saveAsToolStripMenuItem.Enabled = true;
                    this.getSQLCreateTableScriptToolStripMenuItem.Enabled = true;
                    this.metadataViewerToolStripMenuItem.Enabled = true;
                }
            }
        }

        private List<string> selectedFields = null;
        private List<string> SelectedFields
        {
            get => this.selectedFields;
            set
            {
                this.selectedFields = value;

                //Check for duplicate fields (We don't support case sensitive field names unfortunately)
                var duplicateFields = this.selectedFields?.GroupBy(f => f.ToUpperInvariant()).Where(g => g.Count() > 1).SelectMany(g => g).ToList();
                if (duplicateFields?.Count() > 0)
                {
                    this.selectedFields = this.selectedFields.Where(f => !duplicateFields.Any(df => df.Equals(f, StringComparison.InvariantCultureIgnoreCase))).ToList();

                    MessageBox.Show($"The following duplicate fields could not be loaded: {string.Join(',', duplicateFields)}. " +
                            $"\r\n\r\nCase sensitive field names are not currently supported.", "Duplicate fields detected",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                if (value?.Count > 0)
                {
                    LoadFileToGridview();
                }
            }
        }

        private int currentOffset = DefaultOffset;
        private int CurrentOffset
        {
            get => this.currentOffset;
            set
            {
                this.currentOffset = value;
                if (this.IsAnyFileOpen)
                    LoadFileToGridview();
            }
        }

        private int currentMaxRowCount = DefaultRowCount;
        private int CurrentMaxRowCount
        {
            get => this.currentMaxRowCount;
            set
            {
                this.currentMaxRowCount = value;
                DefaultRowCount = value;
                if (this.IsAnyFileOpen)
                    LoadFileToGridview();
            }
        }

        private static int DefaultRowCount
        {
            get => AppSettings.LastRowCount ?? DefaultRowCountValue;
            set => AppSettings.LastRowCount = value;
        }

        private bool IsAnyFileOpen
        {
            get
            {
                return !string.IsNullOrWhiteSpace(this.OpenFilePath) && this.SelectedFields != null;
            }
        }

        private DataTable mainDataSource;
        private DataTable MainDataSource
        {
            get => this.mainDataSource;
            set
            {
                this.mainDataSource = value;
                this.mainGridView.DataSource = this.mainDataSource;

                try
                {
                    //Format date fields
                    string dateFormat = AppSettings.DateTimeDisplayFormat.GetDateFormat();
                    foreach (DataGridViewColumn column in this.mainGridView.Columns)
                    {
                        if (column.ValueType == typeof(DateTime))
                            column.DefaultCellStyle.Format = dateFormat;
                    }

                    //Adjust column sizes if required
                    if (AppSettings.AutoSizeColumnsMode != DataGridViewAutoSizeColumnsMode.None)
                        this.mainGridView.AutoResizeColumns(AppSettings.AutoSizeColumnsMode);
                }
                catch { }
            }
        }
        private Panel loadingPanel = null;
        private Parquet.Schema.ParquetSchema openFileSchema;

        private ToolTip dateOnlyFormatWarningToolTip = new();
        #endregion

        public MainForm()
        {
            InitializeComponent();
            this.DefaultFormTitle = this.Text;
            this.offsetTextBox.SetTextQuiet(DefaultOffset.ToString());
            this.recordCountTextBox.SetTextQuiet(DefaultRowCount.ToString());
            this.MainDataSource = new DataTable();
            this.OpenFilePath = null;

            //Set DGV to be double buffered for smoother loading and scrolling
            if (!SystemInformation.TerminalServerSession)
            {
                Type dgvType = this.mainGridView.GetType();
                System.Reflection.PropertyInfo pi = dgvType.GetProperty("DoubleBuffered", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                pi.SetValue(this.mainGridView, true, null);
            }
        }

        public MainForm(string fileToOpenPath) : this()
        {
            //The code below will be executed after the default constructor => this()
            this.fileToLoadOnLaunch = fileToOpenPath;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            try
            {
                //Open existing file on first load (Usually this means user "double clicked" a parquet file with this utility as the default program).
                if (!string.IsNullOrWhiteSpace(this.fileToLoadOnLaunch))
                {
                    this.OpenNewFile(this.fileToLoadOnLaunch);
                }

                //Setup date format checkboxes
                this.RefreshDateFormatMenuItemSelection();

                if (AppSettings.ReadingEngine == ParquetEngine.Default)
                    this.defaultParquetEngineToolStripMenuItem.Checked = true;
                else if (AppSettings.ReadingEngine == ParquetEngine.Default_Multithreaded)
                    this.multithreadedParquetEngineToolStripMenuItem.Checked = true;

                foreach (ToolStripMenuItem toolStripItem in this.columnSizingToolStripMenuItem.DropDown.Items)
                {
                    if (toolStripItem.Tag?.Equals(AppSettings.AutoSizeColumnsMode.ToString()) == true)
                    {
                        toolStripItem.Checked = true;
                        break;
                    }
                }

                if (AppSettings.RememberLastRowCount)
                    this.rememberRecordCountToolStripMenuItem.Checked = true;
                else
                    this.rememberRecordCountToolStripMenuItem.Checked = false;
            }
            catch (Exception ex)
            {
                this.ShowError(ex);
            }
        }

        #region Event Handlers
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
            int offset = 0;
            if (int.TryParse(((TextBox)sender).Text, out offset))
                this.CurrentOffset = offset;
            else
                ((TextBox)sender).Text = this.CurrentOffset.ToString();
        }

        private void recordsToTextBox_TextChanged(object sender, EventArgs e)
        {
            if (int.TryParse(((TextBox)sender).Text, out var recordCount))
                this.CurrentMaxRowCount = recordCount;
            else
                ((TextBox)sender).Text = this.CurrentMaxRowCount.ToString();
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.OpenFilePath = null;
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.openParquetFileDialog.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                {
                    this.OpenNewFile(this.openParquetFileDialog.FileName);
                }
            }
            catch (Exception ex)
            {
                this.OpenFilePath = null;
                this.ShowError(ex);
            }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                this.ExportResults(default);
            }
            catch (Exception ex)
            {
                this.ShowError(ex);
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                this.OpenFieldSelectionDialog(true);
            }
            catch (Exception ex)
            {
                this.ShowError(ex);
            }
        }

        private void MainForm_SizeChanged(object sender, EventArgs e)
        {
            if (this.loadingPanel != null)
                this.loadingPanel.Location = this.GetFormCenter(loadingPanelWidth / 2, loadingPanelHeight / 2);
        }

        private void runQueryButton_Click(object sender, EventArgs e)
        {
            if (this.IsAnyFileOpen)
            {
                string queryText = this.searchFilterTextBox.Text ?? string.Empty;
                queryText = Regex.Replace(queryText, QueryUselessPartRegex, string.Empty).Trim();

                try
                {
                    this.MainDataSource.DefaultView.RowFilter = queryText;
                }
                catch (Exception ex)
                {
                    this.MainDataSource.DefaultView.RowFilter = null;
                    this.ShowError(ex, "The query doesn't seem to be valid. Please try again.", false);
                }
            }
        }

        private void clearFilterButton_Click(object sender, EventArgs e)
        {
            this.MainDataSource.DefaultView.RowFilter = null;
        }

        private void searchFilterTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == Convert.ToChar(Keys.Return))
            {
                this.runQueryButton_Click(this.runQueryButton, null);
            }
        }

        private void MainForm_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files != null && files.Length > 0)
                {
                    this.OpenNewFile(files[0]);
                }
            }
            catch (Exception ex)
            {
                this.OpenFilePath = null;
                this.ShowError(ex);
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
                this.ShowError(ex, null, false);
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            (new AboutBox()).ShowDialog(this);
        }

        private void userGuideToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start(new ProcessStartInfo(WikiURL) { UseShellExecute = true });
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
            }
        }

        #endregion

        private async void OpenFieldSelectionDialog(bool forceOpenDialog)
        {
            if (string.IsNullOrWhiteSpace(this.OpenFilePath))
            {
                return;
            }

            if (this.openFileSchema == null)
            {
                var cancellationToken = this.ShowLoadingIcon("Analyzing File");

                try
                {
                    var schema = await Task.Run(async () =>
                    {
                        var parquetReader = await ParquetReader.CreateAsync(this.OpenFilePath, new ParquetOptions() { TreatByteArrayAsString = true }, cancellationToken);
                        return parquetReader.Schema;
                    }, cancellationToken);

                    this.openFileSchema = schema;
                }
                catch (Exception ex)
                {
                    if (this.openFileSchema == null)
                    {
                        //cancel file open
                        this.OpenFilePath = null;
                    }

                    if (ex is not OperationCanceledException)
                        throw;

                    return;
                }
                finally
                {
                    this.HideLoadingIcon();
                }
            }

            var fields = this.openFileSchema.Fields;
            if (fields != null && fields.Count() > 0)
            {
                if (AppSettings.AlwaysSelectAllFields && !forceOpenDialog)
                {
                    this.SelectedFields = fields.Where(f => !FieldsToLoadForm.UnsupportedSchemaTypes.Contains(f.SchemaType)).Select(f => f.Name).ToList();
                }
                else
                {
                    var fieldSelectionForm = new FieldsToLoadForm(fields, this.MainDataSource?.GetColumnNames() ?? Array.Empty<string>());
                    if (fieldSelectionForm.ShowDialog(this) == DialogResult.OK)
                    {
                        if (fieldSelectionForm.NewSelectedFields != null && fieldSelectionForm.NewSelectedFields.Count > 0)
                            this.SelectedFields = fieldSelectionForm.NewSelectedFields;
                        else
                            this.SelectedFields = fields.Select(f => f.Name).ToList(); //By default, show all fields
                    }
                }
            }
            else
            {
                throw new FileLoadException("The selected file doesn't have any fields");
            }
        }

        private async void LoadFileToGridview()
        {
            try
            {
                if (this.IsAnyFileOpen)
                {
                    if (!File.Exists(this.OpenFilePath))
                    {
                        throw new Exception(string.Format("The specified file no longer exists: {0}{1}Please try opening a new file", this.OpenFilePath, Environment.NewLine));
                    }

                    var cancellationToken = this.ShowLoadingIcon("Loading Data");
                    var finalResult = await Task.Run(async () =>
                    {
                        var results = new ConcurrentDictionary<int, ParquetReadResult>();
                        if (AppSettings.ReadingEngine == ParquetEngine.Default)
                        {
                            using (var parquetReader = await ParquetReader.CreateAsync(this.OpenFilePath, new ParquetOptions() { TreatByteArrayAsString = true }))
                            {
                                DataTable result = await UtilityMethods.ParquetReaderToDataTable(parquetReader, this.SelectedFields, this.CurrentOffset, this.CurrentMaxRowCount, cancellationToken);
                                results.TryAdd(1, new ParquetReadResult(result, parquetReader.ThriftMetadata.Num_rows));
                            }
                        }
                        else
                        {
                            int i = 0;
                            var fieldGroups = new List<(int, List<string>)>();
                            foreach (List<string> fields in UtilityMethods.Split(this.SelectedFields, (int)(this.SelectedFields.Count / Environment.ProcessorCount)))
                            {
                                fieldGroups.Add((i++, fields));
                            }

                            var options = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount, CancellationToken = cancellationToken };
                            await Parallel.ForEachAsync(fieldGroups, options,
                                async (fieldGroup, _cancellationToken) =>
                                {
                                    using (var parquetReader = await ParquetReader.CreateAsync(this.OpenFilePath, new ParquetOptions() { TreatByteArrayAsString = true }))
                                    {
                                        DataTable result = await UtilityMethods.ParquetReaderToDataTable(parquetReader, fieldGroup.Item2, this.CurrentOffset, this.CurrentMaxRowCount, _cancellationToken);
                                        results.TryAdd(fieldGroup.Item1, new ParquetReadResult(result, parquetReader.ThriftMetadata.Num_rows));
                                    }
                                });
                        }

                        if (results.IsEmpty)
                        {
                            throw new FileLoadException("Something went wrong while processing this file. If the issue persists please open a bug ticket on the repo. Help -> About");
                        }

                        DataTable mergedDataTables = UtilityMethods.MergeTables(results.OrderBy(f => f.Key).Select(f => f.Value.Result).AsEnumerable());
                        ParquetReadResult finalResult = new ParquetReadResult(mergedDataTables, results.First().Value.TotalNumberOfRecordsInFile);

                        return finalResult;
                    }, cancellationToken);

                    this.recordCountStatusBarLabel.Text = string.Format("{0} to {1}", this.CurrentOffset, this.CurrentOffset + finalResult.Result.Rows.Count);
                    this.totalRowCountStatusBarLabel.Text = finalResult.TotalNumberOfRecordsInFile.ToString();
                    this.actualShownRecordCountLabel.Text = finalResult.Result.Rows.Count.ToString();

                    this.MainDataSource = finalResult.Result;
                }
            }
            catch (Exception ex)
            {
                if (ex is not OperationCanceledException)
                    this.ShowError(ex);
            }
            finally
            {
                this.HideLoadingIcon();
            }
        }

        private void ShowError(Exception ex, string customMessage = null, bool showStackTrace = true)
        {
            MessageBox.Show(string.Concat(customMessage ?? "Something went wrong:", Environment.NewLine, showStackTrace ? ex.ToString() : ex.Message), ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private CancellationToken ShowLoadingIcon(string message)
        {
            var cancellationToken = new CancellationTokenSource();
            this.loadingPanel = new Panel();
            this.loadingPanel.Size = new Size(loadingPanelWidth, loadingPanelHeight);
            this.loadingPanel.Location = this.GetFormCenter(loadingPanelWidth / 2, loadingPanelHeight / 2);

            this.loadingPanel.Controls.Add(new Label()
            {
                Name = "loadingmessagelabel",
                Text = message,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top
            });

            var pictureBox = new PictureBox()
            {
                Name = "loadingpicturebox",
                Image = Properties.Resources.hourglass,
                Size = new Size(200, 200)
            };
            this.loadingPanel.Controls.Add(pictureBox);

            Button cancelButton = new Button()
            {
                Name = "cancelloadingbutton",
                Text = "Cancel",
                Dock = DockStyle.Bottom,
                Enabled = cancellationToken.Token.CanBeCanceled
            };
            cancelButton.Click += (object buttonSender, EventArgs buttonClickEventArgs) =>
            {
                cancellationToken.Cancel();

                ((Button)buttonSender).Enabled = false;
                ((Button)buttonSender).Text = "Cancelling...";
            };
            this.loadingPanel.Controls.Add(cancelButton);
            cancelButton.BringToFront();

            Cursor.Current = Cursors.WaitCursor;

            this.Controls.Add(this.loadingPanel);

            this.loadingPanel.BringToFront();
            this.loadingPanel.Show();

            this.mainTableLayoutPanel.Enabled = false;
            this.mainMenuStrip.Enabled = false;

            return cancellationToken.Token;
        }

        private void HideLoadingIcon()
        {
            this.mainTableLayoutPanel.Enabled = true;
            this.mainMenuStrip.Enabled = true;
            Cursor.Current = Cursors.Default;

            if (this.loadingPanel != null)
                this.loadingPanel.Dispose();
        }

        private Point GetFormCenter(int offsetX, int offsetY)
        {
            return new Point((this.Size.Width / 2) - offsetX, (this.Size.Height / 2) - offsetY);
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

        private void OpenNewFile(string filePath)
        {
            this.OpenFilePath = filePath;

            this.offsetTextBox.SetTextQuiet(DefaultOffset.ToString());
            this.currentMaxRowCount = DefaultRowCount;
            this.recordCountTextBox.SetTextQuiet(DefaultRowCount.ToString());
            this.currentOffset = DefaultOffset;

            this.OpenFieldSelectionDialog(false);
        }

        private void WriteDataToCSVFile(string path, CancellationToken cancellationToken)
        {
            using var writer = new StreamWriter(path, false, Encoding.UTF8);

            var rowBuilder = new StringBuilder();
            bool isFirst = true;
            foreach (DataColumn column in this.MainDataSource.Columns)
            {
                if (!isFirst)
                {
                    rowBuilder.Append(',');
                }
                else
                {
                    isFirst = false;
                }

                rowBuilder.Append(
                    column.ColumnName
                        .Replace("\r", string.Empty)
                        .Replace("\n", string.Empty)
                        .Replace(",", string.Empty));
            }
            writer.WriteLine(rowBuilder.ToString());

            foreach (DataRowView row in this.MainDataSource.DefaultView)
            {
                rowBuilder.Clear();

                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                isFirst = true;
                string dateFormat = AppSettings.DateTimeDisplayFormat.GetDateFormat();
                foreach (object value in row.Row.ItemArray)
                {
                    if (!isFirst)
                    {
                        rowBuilder.Append(',');
                    }
                    else
                    {
                        isFirst = false;
                    }

                    if (value is DateTime dt)
                    {
                        rowBuilder.Append(UtilityMethods.CleanCSVValue(dt.ToString(dateFormat)));
                    }
                    else
                    {
                        rowBuilder.Append(UtilityMethods.CleanCSVValue(value.ToString()));
                    }
                }

                writer.WriteLine(rowBuilder.ToString());
            }
        }

        private void WriteDataToExcelFile(string path, CancellationToken cancellationToken)
        {
            string dateFormat = AppSettings.DateTimeDisplayFormat.GetDateFormat();
            using var fs = new FileStream(path, FileMode.OpenOrCreate);
            var excelWriter = new ExcelWriter(fs);
            excelWriter.BeginWrite();

            //Write headers
            for (int i = 0; i < this.MainDataSource.Columns.Count; i++)
            {
                excelWriter.WriteCell(0, i, this.MainDataSource.Columns[i].ColumnName);
            }

            //Write data
            for (int i = 0; i < this.MainDataSource.DefaultView.Count; i++)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                for (int j = 0; j < this.MainDataSource.Columns.Count; j++)
                {
                    var value = this.mainDataSource.DefaultView[i][j];

                    if (value is DateTime dt)
                    {
                        excelWriter.WriteCell(i + 1, j, dt.ToString(dateFormat));
                    }
                    else
                    {
                        excelWriter.WriteCell(i + 1, j, value?.ToString() ?? string.Empty);
                    }
                }
            }

            excelWriter.EndWrite();
        }

        private async void ExportResults(FileType defaultFileType)
        {
            string filePath = null;
            try
            {
                if (this.mainGridView.RowCount > 0)
                {
                    this.exportFileDialog.Title = string.Format("{0} records will be exported", this.mainGridView.RowCount);
                    this.exportFileDialog.Filter = "CSV file (*.csv)|*.csv|Excel file (*.xls)|*.xls";
                    this.exportFileDialog.FilterIndex = (int)defaultFileType + 1;
                    if (this.exportFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        filePath = this.exportFileDialog.FileName;
                        var selectedFileType = Path.GetExtension(filePath).Equals(FileType.XLS.GetExtension()) ? FileType.XLS : FileType.CSV;

                        var cancellationToken = this.ShowLoadingIcon("Exporting Data");
                        if (selectedFileType == FileType.CSV)
                        {
                            await Task.Run(() => this.WriteDataToCSVFile(filePath, cancellationToken));
                        }
                        else if (selectedFileType == FileType.XLS)
                        {
                            await Task.Run(() => this.WriteDataToExcelFile(filePath, cancellationToken));
                        }
                        else
                        {
                            throw new Exception(string.Format("Unsupported export type: '{0}'", selectedFileType.ToString()));
                        }

                        if (cancellationToken.IsCancellationRequested)
                        {
                            CleanupFile(filePath);
                            MessageBox.Show("Export has been cancelled", "Export Cancelled", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("Export successful!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }
            catch (Exception)
            {
                CleanupFile(filePath);
                throw;
            }
            finally
            {
                this.HideLoadingIcon();
            }

            void CleanupFile(string filePath)
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(filePath))
                        File.Delete(filePath);
                }
                catch (Exception) { /*Swallow*/ }
            }
        }

        #region Helper Types
        private class ParquetReadResult
        {
            public long TotalNumberOfRecordsInFile { get; private set; }
            public DataTable Result { get; private set; }

            public ParquetReadResult(DataTable result, long totalNumberOfRecordsInFile)
            {
                this.TotalNumberOfRecordsInFile = totalNumberOfRecordsInFile;
                this.Result = result;
            }
        }
        #endregion

        private void GetSQLCreateTableScriptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                string tableName = Path.GetFileNameWithoutExtension(this.OpenFilePath) ?? DefaultTableName;
                if (this.mainDataSource?.Columns.Count > 0)
                {
                    var dataset = new DataSet();

                    this.mainDataSource.TableName = tableName;
                    dataset.Tables.Add(this.mainDataSource);

                    var scriptAdapter = new CustomScriptBasedSchemaAdapter();
                    string sql = scriptAdapter.GetSchemaScript(dataset, false);

                    Clipboard.SetText(sql);
                    MessageBox.Show(this, "Create table script copied to clipboard!", "Parquet Viewer", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                    MessageBox.Show(this, "Please select some fields first to get the SQL script", "Parquet Viewer", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                this.ShowError(ex);
            }
        }

        private void DateFormatMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (sender is ToolStripMenuItem item)
                {
                    var selectedDateFormat = (DateFormat)(int.Parse((string)item.Tag));
                    AppSettings.DateTimeDisplayFormat = selectedDateFormat;
                    this.RefreshDateFormatMenuItemSelection();
                    this.MainDataSource = this.MainDataSource; //Will cause a refresh of the date formats
                }
            }
            catch (Exception ex)
            {
                this.ShowError(ex);
            }
        }

        /// <summary>
        /// Checks <see cref="AppSettings.DateTimeDisplayFormat"/> and checks/unchecks 
        /// the appropriate date format options located in the menu bar.
        /// </summary>
        private void RefreshDateFormatMenuItemSelection()
        {
            this.defaultToolStripMenuItem.Checked = false;
            this.defaultDateOnlyToolStripMenuItem.Checked = false;
            this.iSO8601ToolStripMenuItem.Checked = false;
            this.iSO8601DateOnlyToolStripMenuItem.Checked = false;
            this.iSO8601Alt1ToolStripMenuItem.Checked = false;
            this.iSO8601Alt2ToolStripMenuItem.Checked = false;

            switch (AppSettings.DateTimeDisplayFormat)
            {
                case DateFormat.Default:
                    this.defaultToolStripMenuItem.Checked = true;
                    break;
                case DateFormat.Default_DateOnly:
                    this.defaultDateOnlyToolStripMenuItem.Checked = true;
                    break;
                case DateFormat.ISO8601:
                    this.iSO8601ToolStripMenuItem.Checked = true;
                    break;
                case DateFormat.ISO8601_DateOnly:
                    this.iSO8601DateOnlyToolStripMenuItem.Checked = true;
                    break;
                case DateFormat.ISO8601_Alt1:
                    this.iSO8601Alt1ToolStripMenuItem.Checked = true;
                    break;
                case DateFormat.ISO8601_Alt2:
                    this.iSO8601Alt2ToolStripMenuItem.Checked = true;
                    break;
                default:
                    break;
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

        private void DefaultParquetEngineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                AppSettings.ReadingEngine = ParquetEngine.Default;
                this.defaultParquetEngineToolStripMenuItem.Checked = true;
                this.multithreadedParquetEngineToolStripMenuItem.Checked = false;
            }
            catch (Exception ex)
            {
                this.ShowError(ex);
            }
        }

        private void MultithreadedParquetEngineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                AppSettings.ReadingEngine = ParquetEngine.Default_Multithreaded;
                this.defaultParquetEngineToolStripMenuItem.Checked = false;
                this.multithreadedParquetEngineToolStripMenuItem.Checked = true;
            }
            catch (Exception ex)
            {
                this.ShowError(ex);
            }
        }

        private async void MetadataViewerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                using var parquetReader = await ParquetReader.CreateAsync(this.OpenFilePath, new ParquetOptions() { TreatByteArrayAsString = true });
                using var metadataViewer = new MetadataViewer(parquetReader);

                metadataViewer.ShowDialog(this);
            }
            catch (Exception ex)
            {
                this.ShowError(ex);
            }
        }

        private void changeColumnSizingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                DataGridViewAutoSizeColumnsMode columnSizingMode;
                if (sender is ToolStripMenuItem tsi && tsi.Tag != null
                    && Enum.TryParse(tsi.Tag.ToString(), out columnSizingMode)
                    && AppSettings.AutoSizeColumnsMode != columnSizingMode)
                {
                    AppSettings.AutoSizeColumnsMode = columnSizingMode;
                    foreach (ToolStripMenuItem toolStripItem in tsi.GetCurrentParent().Items)
                    {
                        toolStripItem.Checked = toolStripItem.Tag?.Equals(tsi.Tag) == true;
                    }

                    var tempMainDataSource = this.MainDataSource;
                    if (columnSizingMode == DataGridViewAutoSizeColumnsMode.None)
                        this.MainDataSource = null; //Need to reload the entire grid to return to the default sizing

                    this.MainDataSource = tempMainDataSource; //Will cause a refresh of the column rendering
                }
            }
            catch (Exception ex)
            {
                this.ShowError(ex);
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
        }

        private void rememberRecordCountToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                this.rememberRecordCountToolStripMenuItem.Checked = !this.rememberRecordCountToolStripMenuItem.Checked;
                AppSettings.RememberLastRowCount = this.rememberRecordCountToolStripMenuItem.Checked;
                AppSettings.LastRowCount = this.CurrentMaxRowCount;
            }
            catch (Exception ex)
            {
                this.ShowError(ex);
            }
        }
    }
}
