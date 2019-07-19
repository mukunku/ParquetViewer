using Parquet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
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
        private readonly string DefaultFormTitle;

        #region Members
        private string fileToLoadOnLaunch = null;
        private string openFilePath;
        private string OpenFilePath
        {
            get
            {
                return this.openFilePath;
            }
            set
            {
                this.openFileSchema = null;
                this.openFilePath = value;
                if (string.IsNullOrWhiteSpace(value))
                {
                    this.Text = this.DefaultFormTitle;
                    this.changeFieldsMenuStripButton.Enabled = false;
                    this.recordCountStatusBarLabel.Text = "0";
                    this.totalRowCountStatusBarLabel.Text = "0";
                    this.MainDataSource.Clear();
                    this.MainDataSource.Columns.Clear();
                }
                else
                {
                    this.Text = string.Concat("Open File: ", value);
                    this.changeFieldsMenuStripButton.Enabled = true;
                }
            }
        }

        private List<string> selectedFields = null;
        private List<string> SelectedFields
        {
            get
            {
                return this.selectedFields;
            }
            set
            {
                if (value != null && value.Count > 0)
                {
                    this.selectedFields = value;
                    LoadFileToGridview();
                }
            }
        }

        private int currentOffset = DefaultOffset;
        private int CurrentOffset
        {
            get { return this.currentOffset; }
            set
            {
                this.currentOffset = value;
                LoadFileToGridview();
            }
        }

        private int currentMaxRowCount = DefaultRowCount;
        private int CurrentMaxRowCount
        {
            get { return this.currentMaxRowCount; }
            set
            {
                this.currentMaxRowCount = value;
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
            get { return this.mainDataSource; }
            set
            {
                this.mainDataSource = value;
                this.mainGridView.DataSource = this.mainDataSource;
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
            if (!System.Windows.Forms.SystemInformation.TerminalServerSession)
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
            {
                this.CurrentOffset = offset;
            }
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
                        ContextMenu menu = new ContextMenu();
                        var copyMenuItem = new MenuItem("Copy");

                        copyMenuItem.Click += (object clickSender, EventArgs clickArgs) =>
                        {
                            Clipboard.SetText(dgv[columnIndex, rowIndex].Value.ToString());
                        };

                        menu.MenuItems.Add(copyMenuItem);
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

        #endregion

        private void OpenFieldSelectionDialog()
        {
            if (!string.IsNullOrWhiteSpace(this.OpenFilePath))
            {
                if (this.openFileSchema == null)
                {
                    this.ShowLoadingIcon("Analyzing File", this.FileSchemaBackgroundWorker);
                    this.FileSchemaBackgroundWorker.RunWorkerAsync();
                }
                else
                    this.FileSchemaBackgroundWorker_RunWorkerCompleted(null, new System.ComponentModel.RunWorkerCompletedEventArgs(this.openFileSchema, null, false));
            }
        }

        private void LoadFileToGridview()
        {
            try
            {
                if (this.IsAnyFileOpen)
                {
                    if (File.Exists(this.OpenFilePath))
                    {
                        ParquetReadArgs args = new ParquetReadArgs();
                        args.Count = this.CurrentMaxRowCount;
                        args.Offset = this.CurrentOffset;
                        args.Fields = this.SelectedFields.ToArray();

                        this.ShowLoadingIcon("Loading Data", this.ReadDataBackgroundWorker);
                        this.ReadDataBackgroundWorker.RunWorkerAsync(args);
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

            System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;

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
            System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;

            if (this.loadingPanel != null)
                this.loadingPanel.Dispose();
        }

        private void FileSchemaBackgroundWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            var schema = (Parquet.Data.Schema)e.Argument;
            if (schema != null)
                e.Result = schema;
            else
            {
                //Parquet.NET doesn't have any async methods or readers that allow sequential records reading so we need to use the ThreadPool to support cancellation.
                var task = Task<Parquet.Data.DataSet>.Run(() =>
                {
                    //Unfortunately there's no way to quickly get the metadata from a parquet file without reading an actual data row
                    return ParquetReader.ReadFile(this.OpenFilePath, new ParquetOptions() { TreatByteArrayAsString = true }, new ReaderOptions() { Count = 1, Offset = 0 });
                });

                while (!task.IsCompleted && !((BackgroundWorker)sender).CancellationPending)
                {
                    task.Wait(1000);
                }

                e.Cancel = ((BackgroundWorker)sender).CancellationPending;

                if (task.IsCompleted)
                    e.Result = task.Result.Schema;
            }
        }

        private void FileSchemaBackgroundWorker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
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
                    string[] fields = this.openFileSchema.FieldNames;
                    if (fields != null && fields.Length > 0)
                    {
                        var fieldSelectionForm = new FieldsToLoadForm(fields, UtilityMethods.GetDataTableColumns(this.MainDataSource));
                        if (fieldSelectionForm.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                        {
                            if (fieldSelectionForm.NewSelectedFields != null && fieldSelectionForm.NewSelectedFields.Count > 0)
                                this.SelectedFields = fieldSelectionForm.NewSelectedFields;
                            else
                                this.SelectedFields = new List<string>(fields); //By default, show all fields
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
            var args = (ParquetReadArgs)e.Argument;

            //Parquet.NET doesn't have any async methods or readers that allow sequential records reading so we need to use the ThreadPool to support cancellation.
            var task = Task<Parquet.Data.DataSet>.Run(() =>
            {
                //Unfortunately there's no way to quickly get the metadata from a parquet file without reading an actual data row
                //BUG: Parquet.NET doesn't always respect the Count parameter, sometimes returning more than the passed value...
                return ParquetReader.ReadFile(this.OpenFilePath,
                            new ParquetOptions() { TreatByteArrayAsString = true },
                            new ReaderOptions() { Count = args.Count, Offset = args.Offset, Columns = args.Fields });
            });

            while (!task.IsCompleted && !((BackgroundWorker)sender).CancellationPending)
            {
                task.Wait(1000);
            }

            e.Cancel = ((BackgroundWorker)sender).CancellationPending;

            if (task.IsCompleted)
                e.Result = task.Result;
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

                    var ds = (Parquet.Data.DataSet)e.Result;
                    this.recordCountStatusBarLabel.Text = string.Format("{0} to {1}", this.CurrentOffset, this.CurrentOffset + ds.RowCount);
                    this.totalRowCountStatusBarLabel.Text = ds.TotalRowCount.ToString();
                    this.actualShownRecordCountLabel.Text = ds.RowCount.ToString();

                    this.MainDataSource = UtilityMethods.ParquetDataSetToDataTable(ds);
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
        }

        private void OpenNewFile(string filePath)
        {
            this.OpenFilePath = null;
            this.offsetTextBox.Text = DefaultOffset.ToString();
            this.recordCountTextBox.Text = DefaultRowCount.ToString();

            this.OpenFilePath = filePath;
            this.OpenFieldSelectionDialog();
        }

        private void ExportFileBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var args = (ExportToFileArgs)e.Argument;
            bool encouteredException = false;
            try
            {
                using (var writer = new StreamWriter(args.FilePath, false, System.Text.Encoding.UTF8))
                {
                    if (args.FileType == FileType.CSV)
                        this.WriteDataToCSVFile(writer, (BackgroundWorker)sender, e);
                    else
                        throw new Exception(string.Format("Unsupported export type: '{0}'", args.FileType.ToString()));
                }
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

                    rowBuilder.Append(UtilityMethods.CleanCSVValue(value.ToString())); //Default formatting for all data types for now
                }

                writer.WriteLine(rowBuilder.ToString());
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
                this.exportFileDialog.FilterIndex = (int)fileType;
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
            public int Count;
            public int Offset;
            public string[] Fields;
        }

        private enum FileType
        {
            CSV = 0//should match Filter Index in the exportFileDialog control's Filter property
        }

        private struct ExportToFileArgs
        {
            public string FilePath;
            public FileType FileType;
        }
        #endregion
    }
}
