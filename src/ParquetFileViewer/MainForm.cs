using ClosedXML.Excel;
using Parquet;
using ParquetFileViewer.Helpers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ParquetFileViewer
{
    public partial class MainForm : Form
    {
        private const string WikiURL = "https://github.com/mukunku/ParquetViewer/wiki";
        private const int DefaultOffset = 0;
        private const int DefaultRowCount = 1000;
        private const int loadingPanelWidth = 200;
        private const int loadingPanelHeight = 200;
        private const string QueryUselessPartRegex = "^WHERE ";
        private const string ISO8601DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";
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
                    this.Text = string.Concat("Open File: ", value);
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
                if (value != null && value.Count > 0)
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
                if (this.IsAnyFileOpen)
                    LoadFileToGridview();
            }
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
                    string dateFormat = AppSettings.UseISODateFormat ? ISO8601DateTimeFormat : string.Empty;
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
        private Parquet.Data.Schema openFileSchema;
        #endregion

        public MainForm()
        {
            InitializeComponent();
            this.DefaultFormTitle = this.Text;
            this.offsetTextBox.Text = DefaultOffset.ToString();
            this.recordCountTextBox.Text = DefaultRowCount.ToString();
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
            //Open existing file on first load (Usually this means user "double clicked" a parquet file with this utility as the default program).
            if (!string.IsNullOrWhiteSpace(this.fileToLoadOnLaunch))
            {
                this.OpenNewFile(this.fileToLoadOnLaunch);
            }

            //Setup date format checkboxes
            if (AppSettings.UseISODateFormat)
                this.iSO8601ToolStripMenuItem.Checked = true;
            else
                this.defaultToolStripMenuItem.Checked = true;

            //Setup export time checkbox
            if (AppSettings.ExportTime)
                this.exportTimeWithCSVToolStripMenuItem.Checked = true;
            else
                this.exportTimeWithCSVToolStripMenuItem.Checked = false;

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
            int recordCount = 0;
            if (int.TryParse(((TextBox)sender).Text, out recordCount))
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

        private void cSVToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.ExportResults(FileType.CSV);
        }

        private void excelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.ExportResults(FileType.XLSX);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.OpenFieldSelectionDialog();
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
    WHERE field_name = 'string value'
MULTIPLE CONDITIONS: 
    WHERE (field_1 > #01/01/2000# AND field_1 < #01/01/2001#) OR field_2 = 100 OR field_3 = 'string value'", "Filtering Query Syntax Examples");
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
            System.Diagnostics.Process.Start(WikiURL);
        }

        private void MainGridView_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            if (e.Value == null || e.Value == DBNull.Value)
            {
                e.Paint(e.CellBounds, DataGridViewPaintParts.All
                    & ~(DataGridViewPaintParts.ContentForeground));

                var font = new Font(e.CellStyle.Font, FontStyle.Italic);
                var color = SystemColors.ActiveCaptionText;
                if (this.mainGridView.SelectedCells.Contains(((DataGridView)sender)[e.ColumnIndex, e.RowIndex]))
                    color = Color.White;

                TextRenderer.DrawText(e.Graphics, "NULL", font, e.CellBounds, color, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);

                e.Handled = true;
            }
        }

        #endregion

        private void OpenFieldSelectionDialog()
        {
            if (!string.IsNullOrWhiteSpace(this.OpenFilePath) && !this.FileSchemaBackgroundWorker.IsBusy)
            {
                if (this.openFileSchema == null)
                {
                    this.ShowLoadingIcon("Analyzing File", this.FileSchemaBackgroundWorker);
                    this.FileSchemaBackgroundWorker.RunWorkerAsync();
                }
                else
                    this.FileSchemaBackgroundWorker_RunWorkerCompleted(-1, new System.ComponentModel.RunWorkerCompletedEventArgs(this.openFileSchema, null, false));
            }
        }

        private void LoadFileToGridview()
        {
            try
            {
                if (this.IsAnyFileOpen && !this.ReadDataBackgroundWorker.IsBusy)
                {
                    if (File.Exists(this.OpenFilePath))
                    {
                        ParquetReadArgs args = new ParquetReadArgs();
                        args.Count = this.CurrentMaxRowCount;
                        args.Offset = this.CurrentOffset;
                        args.Fields = this.SelectedFields.ToArray();

                        this.ShowLoadingIcon("Loading Data", this.ReadDataBackgroundWorker);
                        this.ReadDataBackgroundWorker.RunWorkerAsync();
                    }
                    else
                    {
                        throw new Exception(string.Format("The specified file no longer exists: {0}{1}Please try opening a new file", this.OpenFilePath, Environment.NewLine));
                    }
                }
            }
            catch (Exception ex)
            {
                this.ShowError(ex);
            }
        }

        private void ShowError(Exception ex, string customMessage = null, bool showStackTrace = true)
        {
            MessageBox.Show(string.Concat(customMessage ?? "Something went wrong:", Environment.NewLine, showStackTrace ? ex.ToString() : ex.Message), ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void ShowLoadingIcon(string message, BackgroundWorker worker)
        {
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
                Enabled = worker != null && worker.WorkerSupportsCancellation
            };
            cancelButton.Click += (object buttonSender, EventArgs buttonClickEventArgs) =>
            {
                if (worker != null && worker.IsBusy)
                    worker.CancelAsync();

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
        }

        private void HideLoadingIcon()
        {
            this.mainTableLayoutPanel.Enabled = true;
            this.mainMenuStrip.Enabled = true;
            Cursor.Current = Cursors.Default;

            if (this.loadingPanel != null)
                this.loadingPanel.Dispose();
        }

        private void FileSchemaBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var schema = (Parquet.Data.Schema)e.Argument;
            if (schema != null)
                e.Result = schema;
            else
            {
                //Parquet.NET doesn't have any async methods or readers that allow sequential reading so we need to use the ThreadPool to support cancellation.
                var task = Task<ParquetReader>.Run(() =>
                {
                    //Unfortunately there's no way to quickly get the metadata from a parquet file without reading an actual data row
                    using (var parquetReader = ParquetReader.OpenFromFile(this.OpenFilePath, new ParquetOptions() { TreatByteArrayAsString = true }))
                    {
                        return parquetReader.Schema;
                    }
                });

                while (!task.IsCompleted && !((BackgroundWorker)sender).CancellationPending)
                {
                    task.Wait(1000);
                }

                e.Cancel = ((BackgroundWorker)sender).CancellationPending;

                if (task.IsCompleted)
                    e.Result = task.Result;
            }
        }

        private void FileSchemaBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.HideLoadingIcon();
            try
            {
                if (e.Cancelled)
                {
                    if (this.openFileSchema == null)
                    {
                        //cancel file open
                        this.OpenFilePath = null;
                    }
                    return;
                }

                if (e.Error == null)
                {
                    this.openFileSchema = (Parquet.Data.Schema)e.Result;
                    var fields = this.openFileSchema.Fields;
                    if (fields != null && fields.Count > 0)
                    {
                        if (AppSettings.AlwaysSelectAllFields && sender?.GetType() != typeof(int)) //We send -1 from the field selection tooltip item so we can force the field selection form to be shown
                        {
                            this.SelectedFields = fields.Where(f => !FieldsToLoadForm.UnsupportedSchemaTypes.Contains(f.SchemaType)).Select(f => f.Name).ToList();
                        }
                        else
                        {
                            var fieldSelectionForm = new FieldsToLoadForm(fields, this.MainDataSource?.GetColumnNames() ?? new string[0]);
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
                        throw new FileLoadException("The selected file doesn't have any fields");
                }
                else
                    throw e.Error;
            }
            catch (Exception ex)
            {
                this.ShowError(ex);
            }
        }

        private Point GetFormCenter(int offsetX, int offsetY)
        {
            return new Point((this.Size.Width / 2) - offsetX, (this.Size.Height / 2) - offsetY);
        }

        private void ReadDataBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            //Parquet.NET doesn't have any async methods or readers that allow sequential records reading so we need to use the ThreadPool to support cancellation.
            Task task = null;
            var results = new ConcurrentDictionary<int, ParquetReadResult>();
            var cancellationToken = new System.Threading.CancellationTokenSource();
            if (AppSettings.ReadingEngine == ParquetEngine.Default)
            {
                task = Task.Run(() =>
                {
                    using (var parquetReader = ParquetReader.OpenFromFile(this.OpenFilePath, new ParquetOptions() { TreatByteArrayAsString = true }))
                    {
                        DataTable result = UtilityMethods.ParquetReaderToDataTable(parquetReader, this.SelectedFields, this.CurrentOffset, this.CurrentMaxRowCount, cancellationToken.Token);
                        results.TryAdd(1, new ParquetReadResult(result, parquetReader.ThriftMetadata.Num_rows));
                    }
                });
            }
            else
            {
                int i = 0;
                var fieldGroups = new List<(int, List<string>)>();
                foreach (List<string> fields in UtilityMethods.Split(this.SelectedFields, (int)(this.selectedFields.Count / Environment.ProcessorCount)))
                {
                    fieldGroups.Add((i++, fields));
                }

                task = ParallelAsync.ForeachAsync(fieldGroups, Environment.ProcessorCount,
                    async fieldGroup =>
                    {
                        await Task.Run(() =>
                        {
                            using (Stream parquetStream = new FileStream(this.OpenFilePath, FileMode.Open, FileAccess.Read))
                            using (var parquetReader = new ParquetReader(parquetStream, new ParquetOptions() { TreatByteArrayAsString = true }))
                            {
                                DataTable result = UtilityMethods.ParquetReaderToDataTable(parquetReader, fieldGroup.Item2, this.CurrentOffset, this.CurrentMaxRowCount, cancellationToken.Token);
                                results.TryAdd(fieldGroup.Item1, new ParquetReadResult(result, parquetReader.ThriftMetadata.Num_rows));
                            }
                        });
                    });
            }

            while (!task.IsCompleted && !((BackgroundWorker)sender).CancellationPending)
            {
                task.Wait(1000);
            }

            if (((BackgroundWorker)sender).CancellationPending)
            {
                cancellationToken.Cancel();
                e.Cancel = true;
            }

            if (task.IsCompleted)
            {
                if (results.Count > 0)
                {
                    DataTable mergedDataTables = UtilityMethods.MergeTables(results.OrderBy(f => f.Key).Select(f => f.Value.Result).AsEnumerable());
                    ParquetReadResult finalResult = new ParquetReadResult(mergedDataTables, results.First().Value.TotalNumberOfRecordsInFile);
                    e.Result = finalResult;
                }
                else
                {
                    //The code should never reach here
                    e.Result = new ParquetReadResult(new DataTable(), 0);
                }
            }
        }

        private void ReadDataBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                if (e.Error == null)
                {
                    if (e.Cancelled)
                    {
                        return;
                    }

                    var result = (ParquetReadResult)e.Result;
                    this.recordCountStatusBarLabel.Text = string.Format("{0} to {1}", this.CurrentOffset, this.CurrentOffset + result.Result.Rows.Count);
                    this.totalRowCountStatusBarLabel.Text = result.TotalNumberOfRecordsInFile.ToString();
                    this.actualShownRecordCountLabel.Text = result.Result.Rows.Count.ToString();

                    this.MainDataSource = result.Result;
                }
                else
                    throw e.Error;
            }
            catch (Exception ex)
            {
                this.ShowError(ex);
            }
            finally
            {
                this.HideLoadingIcon();
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

        private void OpenNewFile(string filePath)
        {
            this.OpenFilePath = filePath;
            this.offsetTextBox.Text = string.IsNullOrWhiteSpace(this.offsetTextBox.Text) ? DefaultOffset.ToString() : this.offsetTextBox.Text;
            this.recordCountTextBox.Text = string.IsNullOrWhiteSpace(this.recordCountTextBox.Text) ? DefaultRowCount.ToString() : this.recordCountTextBox.Text;

            this.OpenFieldSelectionDialog();
        }

        private void ExportFileBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var args = (ExportToFileArgs)e.Argument;
            bool encouteredException = false;
            try
            {
                if (args.FileType == FileType.CSV)
                {
                    using (var writer = new StreamWriter(args.FilePath, false, System.Text.Encoding.UTF8))
                        this.WriteDataToCSVFile(writer, (BackgroundWorker)sender, e);
                }
                else if (args.FileType == FileType.XLSX)
                {
                    this.WriteDataToExcelFile(args.FilePath, (BackgroundWorker)sender, e);
                }
                else
                    throw new Exception(string.Format("Unsupported export type: '{0}'", args.FileType.ToString()));
            }
            catch (Exception)
            {
                encouteredException = true;
                throw;
            }
            finally
            {
                if (e.Cancel || encouteredException)
                {
                    try
                    {
                        File.Delete(args.FilePath);
                    }
                    catch (Exception) { /*Swallow for now*/ }
                }
            }
        }

        private void WriteDataToCSVFile(StreamWriter writer, BackgroundWorker worker, DoWorkEventArgs e)
        {
            StringBuilder rowBuilder = new StringBuilder();
            bool isFirst = true;
            bool exportTime = AppSettings.ExportTime;
            foreach (DataColumn column in this.MainDataSource.Columns)
            {
                if (!isFirst)
                    rowBuilder.Append(",");
                else
                    isFirst = false;

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

                if (worker.CancellationPending)
                {
                    e.Cancel = true;
                    break;
                }

                isFirst = true;
                foreach (object value in row.Row.ItemArray)
                {
                    if (!isFirst)
                        rowBuilder.Append(",");
                    else
                        isFirst = false;
                    if (!exportTime && value.GetType() == typeof(DateTime))
                    {
                        DateTime dateToDisplay = (DateTime)value;
                        rowBuilder.Append(UtilityMethods.CleanCSVValue(dateToDisplay.ToString("d")));
                    }
                    else
                        rowBuilder.Append(UtilityMethods.CleanCSVValue(value.ToString())); //Default formatting for all data types for now
                }

                writer.WriteLine(rowBuilder.ToString());
            }
        }

        private void WriteDataToExcelFile(string path, BackgroundWorker worker, DoWorkEventArgs e)
        {
            using (var workbook = new XLWorkbook())
            {
                workbook.Worksheets.Add(this.MainDataSource.DefaultView.ToTable(), "data");
                workbook.SaveAs(path);
            }
        }

            private void ExportFileBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.HideLoadingIcon();
            try
            {
                if (e.Cancelled)
                    MessageBox.Show("Export has been cancelled", "Export Cancelled", MessageBoxButtons.OK, MessageBoxIcon.Information);
                else if (e.Error != null)
                    throw e.Error;
                else
                    MessageBox.Show("Export successful!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                this.ShowError(ex);
            }
        }

        private void ExportResults(FileType fileType)
        {
            if (this.mainGridView.RowCount > 0)
            {
                this.exportFileDialog.Title = string.Format("{0} records will be exported", this.mainGridView.RowCount);
                this.exportFileDialog.Filter = "CSV file (*.csv)|*.csv|Excel file (*.xlsx)|*.xlsx";
                this.exportFileDialog.FilterIndex = (int)fileType + 1;
                if (this.exportFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    var args = new ExportToFileArgs()
                    {
                        FilePath = this.exportFileDialog.FileName,
                        FileType = fileType
                    };
                    this.ExportFileBackgroundWorker.RunWorkerAsync(args);
                    this.ShowLoadingIcon("Exporting Data", this.ExportFileBackgroundWorker);
                }
            }
        }

        #region Helper Types
        private struct ParquetReadArgs
        {
            public long Count;
            public long Offset;
            public string[] Fields;
        }

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

        private enum FileType
        {
            CSV = 0,//should match Filter Index in the exportFileDialog control's Filter property
            XLSX = 1
        }

        private struct ExportToFileArgs
        {
            public string FilePath;
            public FileType FileType;
        }
        #endregion

        private void GetSQLCreateTableScriptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string tableName = DefaultTableName;
            try
            {
                tableName = Path.GetFileNameWithoutExtension(this.OpenFilePath);
            }
            catch { /* just in case */ }

            try
            {
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

        private void DefaultToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                AppSettings.UseISODateFormat = false;
                this.defaultToolStripMenuItem.Checked = true;
                this.iSO8601ToolStripMenuItem.Checked = false;
                this.MainDataSource = this.MainDataSource; //Will cause a refresh of the date formats
            }
            catch (Exception ex)
            {
                this.ShowError(ex);
            }
        }

        private void ISO8601ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                AppSettings.UseISODateFormat = true;
                this.defaultToolStripMenuItem.Checked = false;
                this.iSO8601ToolStripMenuItem.Checked = true;
                this.MainDataSource = this.MainDataSource; //Will cause a refresh of the date formats
            }
            catch (Exception ex)
            {
                this.ShowError(ex);
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

        private void MetadataViewerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                using (Stream parquetStream = new FileStream(this.OpenFilePath, FileMode.Open, FileAccess.Read))
                using (var parquetReader = new ParquetReader(parquetStream, new ParquetOptions() { TreatByteArrayAsString = true }))
                using (var metadataViewer = new MetadataViewer(parquetReader))
                {
                    metadataViewer.ShowDialog(this);
                }
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

        private void exportTimeWithCSVToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                AppSettings.ExportTime = !AppSettings.ExportTime;
                this.exportTimeWithCSVToolStripMenuItem.Checked = !this.exportTimeWithCSVToolStripMenuItem.Checked;
            }
            catch (Exception ex)
            {
                this.ShowError(ex);
            }
        }
    }
}
