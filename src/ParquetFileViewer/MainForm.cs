using ParquetFileViewer.Exceptions;
using ParquetFileViewer.Helpers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
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
        private const int DefaultRowCountValue = 1000;
        private const int PerformanceWarningCellCount = 350000;
        private readonly string DefaultFormTitle;

        #region Members
        private readonly string fileToLoadOnLaunch = null;

        private string _openFileOrFolderPath;
        private string OpenFileOrFolderPath
        {
            get => this._openFileOrFolderPath;
            set
            {
                this._openFileOrFolderPath = value;
                this._openParquetEngine = null;
                this.SelectedFields = null;
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

        private static int DefaultRowCount
        {
            get => AppSettings.LastRowCount ?? DefaultRowCountValue;
            set => AppSettings.LastRowCount = value;
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

        private bool IsAnyFileOpen
            => !string.IsNullOrWhiteSpace(this.OpenFileOrFolderPath)
                && this.SelectedFields is not null
                && this._openParquetEngine is not null;

        private DataTable mainDataSource;
        private DataTable MainDataSource
        {
            get => this.mainDataSource;
            set
            {
                //Check for performance issues
                int? cellsToRender = value?.Columns.Count * value?.Rows.Count;
                if (cellsToRender > PerformanceWarningCellCount && AppSettings.AutoSizeColumnsMode == DataGridViewAutoSizeColumnsMode.AllCells)
                {
                    //Don't spam the user so ask only once per app update
                    if (AppSettings.WarningBypassedOnVersion != AboutBox.AssemblyVersion)
                    {
                        var choice = MessageBox.Show(this, $"Looks like you're loading a lot of data with column sizing set to 'Fit Headers & Content'. This might cause significant load times. " +
                            Environment.NewLine + Environment.NewLine + $"If you experience performance issues try changing the default column sizing: Edit -> Column Sizing" +
                            Environment.NewLine + Environment.NewLine + "Continue loading file anyway?", "Performance Warning",
                            MessageBoxButtons.OKCancel);

                        if (choice == DialogResult.Cancel)
                            return;
                        else
                            AppSettings.WarningBypassedOnVersion = AboutBox.AssemblyVersion;
                    }
                }

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

        private Helpers.ParquetEngine _openParquetEngine = null;
        #endregion

        public MainForm()
        {
            InitializeComponent();
            this.DefaultFormTitle = this.Text;
            this.offsetTextBox.SetTextQuiet(DefaultOffset.ToString());
            this.recordCountTextBox.SetTextQuiet(DefaultRowCount.ToString());
            this.MainDataSource = new DataTable();
            this.OpenFileOrFolderPath = null;

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
                    this.OpenNewFileOrFolder(this.fileToLoadOnLaunch);
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
                ShowError(ex);
            }
        }

        private async void OpenFieldSelectionDialog(bool forceOpenDialog)
        {
            if (string.IsNullOrWhiteSpace(this.OpenFileOrFolderPath))
            {
                return;
            }

            if (this._openParquetEngine == null)
            {
                var cancellationToken = this.ShowLoadingIcon("Analyzing File");

                try
                {
                    this._openParquetEngine = await Helpers.ParquetEngine.OpenFileOrFolderAsync(this.OpenFileOrFolderPath, cancellationToken);
                }
                catch (Exception ex)
                {

                    if (this._openParquetEngine == null)
                    {
                        //cancel file open
                        this.OpenFileOrFolderPath = null;
                    }

                    if (ex is AllFilesSkippedException afse)
                    {
                        HandleAllFilesSkippedException(afse);
                    }
                    else if (ex is SomeFilesSkippedException sfse)
                    {
                        HandleSomeFilesSkippedException(sfse);
                    }
                    else if (ex is FileReadException fre)
                    {
                        HandleFileReadException(fre);
                    }
                    else if (ex is MultipleSchemasFoundException msfe)
                    {
                        HandleMultipleSchemasFoundException(msfe);
                    }
                    else if (ex is not OperationCanceledException)
                        throw;

                    return;
                }
                finally
                {
                    this.HideLoadingIcon();
                }
            }

            var fields = this._openParquetEngine.Schema.Fields;
            if (fields != null && fields.Count > 0)
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
                    if (!File.Exists(this.OpenFileOrFolderPath) && !Directory.Exists(this.OpenFileOrFolderPath))
                    {
                        throw new Exception(string.Format("The specified file/folder no longer exists: {0}{1}Please try opening a new file or folder", this.OpenFileOrFolderPath, Environment.NewLine));
                    }

                    var cancellationToken = this.ShowLoadingIcon("Loading Data");
                    var finalResult = await Task.Run(async () =>
                    {
                        var results = new ConcurrentDictionary<int, DataTable>();
                        if (AppSettings.ReadingEngine == ParquetEngine.Default)
                        {
                            var dataTable = await this._openParquetEngine.ReadRowsAsync(this.SelectedFields, this.CurrentOffset, this.CurrentMaxRowCount, cancellationToken);
                            results.TryAdd(1, dataTable);
                        }
                        else
                        {
                            int i = 0;
                            var fieldGroups = new List<(int Index, List<string> SubSetOfFields)>();
                            foreach (var fields in UtilityMethods.Split(this.SelectedFields, Environment.ProcessorCount))
                            {
                                fieldGroups.Add((i++, fields.ToList()));
                            }

                            var options = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount, CancellationToken = cancellationToken };
                            await Parallel.ForEachAsync(fieldGroups, options,
                                async (fieldGroup, _cancellationToken) =>
                                {
                                    using var parquetEngine = await this._openParquetEngine.CloneAsync(cancellationToken);
                                    var dataTable = await parquetEngine.ReadRowsAsync(fieldGroup.SubSetOfFields, this.CurrentOffset, this.CurrentMaxRowCount, cancellationToken);
                                    results.TryAdd(fieldGroup.Index, dataTable);// new ParquetReadResult(dataTable, (long)dataTabe.ExtendedProperties[Helpers.ParquetEngine.TotalRecordCountExtendedPropertyKey]));
                                });
                        }

                        if (results.IsEmpty)
                        {
                            throw new FileLoadException("Something went wrong while processing this file. If the issue persists please open a bug ticket on the repo. Help -> About");
                        }

                        DataTable mergedDataTables = UtilityMethods.MergeTables(results.OrderBy(f => f.Key).Select(f => f.Value).AsEnumerable());
                        return mergedDataTables;
                    }, cancellationToken);

                    this.recordCountStatusBarLabel.Text = string.Format("{0} to {1}", this.CurrentOffset, this.CurrentOffset + finalResult.Rows.Count);
                    this.totalRowCountStatusBarLabel.Text = finalResult.ExtendedProperties[Helpers.ParquetEngine.TotalRecordCountExtendedPropertyKey].ToString();
                    this.actualShownRecordCountLabel.Text = finalResult.Rows.Count.ToString();

                    this.MainDataSource = finalResult;
                }
            }
            catch (AllFilesSkippedException ex)
            {
                HandleAllFilesSkippedException(ex);
            }
            catch (SomeFilesSkippedException ex)
            {
                HandleSomeFilesSkippedException(ex);
            }
            catch (FileReadException ex)
            {
                HandleFileReadException(ex);
            }
            catch (MultipleSchemasFoundException ex)
            {
                HandleMultipleSchemasFoundException(ex);
            }
            catch (Exception ex)
            {
                if (ex is not OperationCanceledException)
                    ShowError(ex);
            }
            finally
            {
                this.HideLoadingIcon();
            }
        }

        private static void HandleAllFilesSkippedException(AllFilesSkippedException ex)
        {
            var sb = new StringBuilder();
            sb.AppendLine("No valid Parquet files found in folder. Invalid Parquet files:");
            foreach (var skippedFile in ex.SkippedFiles)
            {
                sb.AppendLine($"-{skippedFile.FileName}");
            }
            ShowError(new Exception(sb.ToString(), ex.SkippedFiles.FirstOrDefault()?.Exception));
        }

        private static void HandleSomeFilesSkippedException(SomeFilesSkippedException ex)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Some files could not be loaded. Invalid Parquet files:");
            foreach (var skippedFile in ex.SkippedFiles)
            {
                sb.AppendLine($"-{skippedFile.FileName}");
            }
            ShowError(new Exception(sb.ToString(), ex.SkippedFiles.FirstOrDefault()?.Exception));
        }

        private static void HandleFileReadException(FileReadException ex)
        {
            ShowError(new Exception($"Could not load parquet file.{Environment.NewLine}{Environment.NewLine}" +
                $"If the problem persists please consider opening a bug ticket in the project repo: Help -> About{Environment.NewLine}", ex));
        }

        private static void HandleMultipleSchemasFoundException(MultipleSchemasFoundException ex)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Multiple schemas detected. ");

            int schemaIndex = 1;
            const int topCount = 5;
            foreach (var schema in ex.Schemas)
            {
                sb.AppendLine($"SCHEMA-{schemaIndex++} FIELDS (TOP 5):");
                for (var i = 0; i < topCount; i++)
                {
                    if (i == schema.Fields.Count)
                        break;

                    sb.AppendLine($"  {schema.Fields.ElementAt(i).Name}");
                }
            }
            ShowError(new Exception(sb.ToString(), ex));
        }

        private void OpenNewFileOrFolder(string fileOrFolderPath)
        {
            this.OpenFileOrFolderPath = fileOrFolderPath;

            this.offsetTextBox.SetTextQuiet(DefaultOffset.ToString());
            this.currentMaxRowCount = DefaultRowCount;
            this.recordCountTextBox.SetTextQuiet(DefaultRowCount.ToString());
            this.currentOffset = DefaultOffset;

            this.OpenFieldSelectionDialog(false);
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
                    ShowError(ex, "The query doesn't seem to be valid. Please try again.", false);
                }
            }
        }

        private void clearFilterButton_Click(object sender, EventArgs e)
        {
            this.MainDataSource.DefaultView.RowFilter = null;
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
    }
}
