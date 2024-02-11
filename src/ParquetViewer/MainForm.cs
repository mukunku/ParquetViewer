using ParquetViewer.Analytics;
using ParquetViewer.Engine.Exceptions;
using ParquetViewer.Engine.Types;
using ParquetViewer.Exceptions;
using ParquetViewer.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ParquetViewer
{
    public partial class MainForm : Form
    {
        private const int DefaultOffset = 0;
        private const int DefaultRowCountValue = 1000;
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
                this.loadAllRowsButton.Enabled = false;
                this.searchFilterTextBox.PlaceholderText = "WHERE ";

                if (string.IsNullOrWhiteSpace(this._openFileOrFolderPath))
                {
                    this.Text = this.DefaultFormTitle;
                }
                else
                {
                    this.Text = string.Concat(
                        File.Exists(this._openFileOrFolderPath) ? $"File: " : "Folder: ",
                        this._openFileOrFolderPath);
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
                this.selectedFields = value?.ToList();

                //Check for duplicate fields (We don't support case sensitive field names unfortunately)
                var duplicateFields = this.selectedFields?.GroupBy(f => f.ToUpperInvariant()).Where(g => g.Count() > 1).SelectMany(g => g).ToList();
                if (duplicateFields?.Count > 0)
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

        private static int DefaultRowCount => DefaultRowCountValue;

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
            => !string.IsNullOrWhiteSpace(this.OpenFileOrFolderPath)
                && this._openParquetEngine is not null;

        private DataTable mainDataSource;
        private DataTable MainDataSource
        {
            get => this.mainDataSource;
            set
            {
                var dataTable = value;
                this.mainDataSource = dataTable;
                this.mainGridView.DataSource = this.mainDataSource;

                this.loadAllRowsButton.Enabled = dataTable.Rows.Count < (this._openParquetEngine?.RecordCount ?? default);
                SetSampleQueryAsPlaceHolder();
            }
        }

        private Engine.ParquetEngine _openParquetEngine = null;
        #endregion

        public MainForm()
        {
            InitializeComponent();
            this.DefaultFormTitle = this.Text;
            this.offsetTextBox.SetTextQuiet(DefaultOffset.ToString());
            this.recordCountTextBox.SetTextQuiet(DefaultRowCount.ToString());
            this.MainDataSource = new DataTable();
            this.OpenFileOrFolderPath = null;

            //Have to set this here because it gets deleted from the .Designer.cs file for some reason
            this.metadataViewerToolStripMenuItem.Image = Properties.Resources.text_file_icon.ToBitmap();
        }

        public MainForm(string fileToOpenPath) : this()
        {
            //The code below will be executed after the default constructor => this()
            this.fileToLoadOnLaunch = fileToOpenPath;
        }

        private async void MainForm_Load(object sender, EventArgs e)
        {
            //Open existing file on first load (Usually this means user "double clicked" a parquet file with this utility as the default program).
            if (!string.IsNullOrWhiteSpace(this.fileToLoadOnLaunch))
            {
                await this.OpenNewFileOrFolder(this.fileToLoadOnLaunch);
            }

            //Setup date format checkboxes
            this.RefreshDateFormatMenuItemSelection();

            foreach (ToolStripMenuItem toolStripItem in this.columnSizingToolStripMenuItem.DropDown.Items)
            {
                if (toolStripItem.Tag?.Equals(AppSettings.AutoSizeColumnsMode.ToString()) == true)
                {
                    toolStripItem.Checked = true;
                    break;
                }
            }

            this.alwaysLoadAllRecordsToolStripMenuItem.Checked = AppSettings.AlwaysLoadAllRecords;

            //Get user's consent to gather analytics; and update the toolstrip menu item accordingly
            Program.GetUserConsentToGatherAnalytics();
            this.shareAnonymousUsageDataToolStripMenuItem.Checked = AppSettings.AnalyticsDataGatheringConsent;
        }

        private async Task<List<string>> OpenFieldSelectionDialog(bool forceOpenDialog)
        {
            if (string.IsNullOrWhiteSpace(this.OpenFileOrFolderPath))
            {
                return null;
            }

            LoadingIcon loadingIcon = null;
            if (this._openParquetEngine == null)
            {
                loadingIcon = this.ShowLoadingIcon("Loading Fields");

                try
                {
                    this._openParquetEngine = await Engine.ParquetEngine.OpenFileOrFolderAsync(this.OpenFileOrFolderPath, loadingIcon.CancellationToken);
                }
                catch (Exception ex)
                {
                    loadingIcon.Dispose();

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

                    return null;
                }
            }

            var fields = this._openParquetEngine.Schema.Fields;
            if (fields != null && fields.Count > 0)
            {
                if (AppSettings.AlwaysSelectAllFields && !forceOpenDialog)
                {
                    loadingIcon?.Dispose();
                    this.Cursor = Cursors.WaitCursor;

                    try
                    {
                        return fields.Where(FieldsToLoadForm.IsSupportedFieldType).Select(f => f.Name).ToList();
                    }
                    finally
                    {
                        this.Cursor = Cursors.Default;
                    }
                }
                else
                {
                    await Task.Delay(125); //Give the UI thread some time to render the loading icon
                    this.Cursor = Cursors.WaitCursor;
                    try
                    {
                        loadingIcon?.Dispose();
                        var fieldSelectionForm = new FieldsToLoadForm(fields, this.MainDataSource?.GetColumnNames() ?? Array.Empty<string>());
                        if (fieldSelectionForm.ShowDialog(this) == DialogResult.OK && fieldSelectionForm.NewSelectedFields?.Count > 0)
                        {
                            return fieldSelectionForm.NewSelectedFields;
                        }
                        else
                        {
                            return null;
                        }
                    }
                    finally
                    {
                        this.Cursor = Cursors.Default;
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
            var stopwatch = Stopwatch.StartNew(); var loadTime = TimeSpan.Zero; var indexTime = TimeSpan.Zero;
            LoadingIcon loadingIcon = null;
            try
            {
                if (!this.IsAnyFileOpen)
                    return;

                if (!File.Exists(this.OpenFileOrFolderPath) && !Directory.Exists(this.OpenFileOrFolderPath))
                {
                    ShowError($"The specified file/folder no longer exists: {this.OpenFileOrFolderPath}{Environment.NewLine}Please try opening a new file or folder");
                    return;
                }

                long cellCount = this.SelectedFields.Count * Math.Min(this.CurrentMaxRowCount, this._openParquetEngine.RecordCount - this.CurrentOffset);
                loadingIcon = this.ShowLoadingIcon("Loading Data", cellCount);

                var intermediateResult = await Task.Run(async () =>
                {
                    return await this._openParquetEngine.ReadRowsAsync(this.SelectedFields, this.CurrentOffset, this.CurrentMaxRowCount, loadingIcon.CancellationToken, loadingIcon);

                }, loadingIcon.CancellationToken);

                loadTime = stopwatch.Elapsed;
                bool showIndexingProgress = false;
                if (loadTime > TimeSpan.FromSeconds(4))
                {
                    //Don't bother showing the indexing step if the data load was really fast because we know 
                    //indexing will be instantaneous. It looks better this way in my opinion.
                    loadingIcon.Reset("Indexing");
                    showIndexingProgress = true;
                }

                var finalResult = await Task.Run(() => intermediateResult.Invoke(showIndexingProgress), loadingIcon.CancellationToken);
                indexTime = stopwatch.Elapsed - loadTime;

                this.recordCountStatusBarLabel.Text = string.Format("{0} to {1}", this.CurrentOffset, this.CurrentOffset + finalResult.Rows.Count);
                this.totalRowCountStatusBarLabel.Text = finalResult.ExtendedProperties[Engine.ParquetEngine.TotalRecordCountExtendedPropertyKey].ToString();
                this.actualShownRecordCountLabel.Text = finalResult.Rows.Count.ToString();

                this.MainDataSource = finalResult;

                FileOpenEvent.FireAndForget(Directory.Exists(this.OpenFileOrFolderPath), this._openParquetEngine.NumberOfPartitions, this._openParquetEngine.RecordCount, this._openParquetEngine.ThriftMetadata.RowGroups.Count(),
                    this._openParquetEngine.Fields.Count(), finalResult.Columns.Cast<DataColumn>().Select(column => column.DataType.Name).Distinct().Order().ToArray(), this.CurrentOffset, this.CurrentMaxRowCount, finalResult.Columns.Count, stopwatch.ElapsedMilliseconds);
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
                    throw;
            }
            finally
            {
                //Little secret performance counter
                stopwatch.Stop();

                TimeSpan totalTime = stopwatch.Elapsed;
                TimeSpan renderTime = totalTime - loadTime - indexTime;
                this.showingStatusBarLabel.ToolTipText = $"Total time: {totalTime:mm\\:ss\\.ff}" + Environment.NewLine +
                $"    Load time: {loadTime:mm\\:ss\\.ff}" + Environment.NewLine +
                $"    Index time: {indexTime:mm\\:ss\\.ff}" + Environment.NewLine +
                $"    Render time: {renderTime:mm\\:ss\\.ff}" + Environment.NewLine;

                loadingIcon?.Dispose();
            }
        }

        private async Task OpenNewFileOrFolder(string fileOrFolderPath)
        {
            this.OpenFileOrFolderPath = fileOrFolderPath;

            this.offsetTextBox.SetTextQuiet(DefaultOffset.ToString());
            this.currentOffset = DefaultOffset;
            this.mainGridView.ClearQuickPeekForms();
            this.searchFilterTextBox.PlaceholderText = "WHERE ";

            var fieldList = await this.OpenFieldSelectionDialog(false);

            if (AppSettings.AlwaysLoadAllRecords)
            {
                this.currentMaxRowCount = (int)this._openParquetEngine.RecordCount;
                this.recordCountTextBox.SetTextQuiet(this._openParquetEngine.RecordCount.ToString());
            }
            else
            {
                this.currentMaxRowCount = DefaultRowCount;
                this.recordCountTextBox.SetTextQuiet(DefaultRowCount.ToString());
            }

            if (fieldList is not null)
                this.SelectedFields = fieldList; //triggers a file load
        }

        private void runQueryButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.IsAnyFileOpen)
                {
                    string queryText = this.searchFilterTextBox.Text ?? string.Empty;
                    queryText = QueryUselessPartRegex().Replace(queryText, string.Empty).Trim();

                    //Treat list, map, and struct types as strings by casting them automatically
                    foreach (var complexField in this.mainGridView.Columns.OfType<DataGridViewColumn>()
                        .Where(c => c.ValueType == typeof(ListValue) || c.ValueType == typeof(MapValue)
                            || c.ValueType == typeof(StructValue) || c.ValueType == typeof(ByteArrayValue))
                        .Select(c => c.Name))
                    {
                        //This isn't perfect but it should handle most cases
                        queryText = queryText.Replace(complexField, $"CONVERT({complexField}, System.String)", StringComparison.InvariantCultureIgnoreCase);
                    }                    

                    try
                    {
                        this.MainDataSource.DefaultView.RowFilter = queryText;
                    }
                    catch (Exception ex)
                    {
                        this.MainDataSource.DefaultView.RowFilter = null;
                        throw new InvalidQueryException(ex);
                    }
                }
            }
            catch (InvalidQueryException ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + Environment.NewLine + ex.InnerException?.Message,
                    "Invalid Query", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception)
            {
                throw;
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

        /// <summary>
        /// Provides the user a sample query in <see cref="searchFilterTextBox"/> using 
        /// the first primitive field available in the dataset. If none are found, the
        /// placeholder won't contain a sample. Only the "WHERE ".
        /// </summary>
        private void SetSampleQueryAsPlaceHolder()
        {
            this.searchFilterTextBox.PlaceholderText = "WHERE ";

            if (this.MainDataSource is null || this.MainDataSource.Rows.Count == 0)
                return;

            var simpleColumn = this.MainDataSource.Columns.AsEnumerable().FirstOrDefault(c => c.DataType.IsSimple());
            if (simpleColumn is null)
                return;

            //find a value we can use as a sample
            object sampleSimpleValue = DBNull.Value; int counter = 1000;
            foreach (DataRow row in this.MainDataSource.Rows)
            {
                sampleSimpleValue = row[simpleColumn];
                if (counter <= 0 || (sampleSimpleValue != DBNull.Value))
                {
                    break;
                }
                counter--;
            }

            if (sampleSimpleValue == DBNull.Value)
                return;

            var dataType = simpleColumn.DataType;
            if (dataType == typeof(DateTime))
            {
                this.searchFilterTextBox.PlaceholderText =
                    $"WHERE {simpleColumn.ColumnName} = #{((DateTime)sampleSimpleValue).ToString(AppSettings.DateTimeDisplayFormat.GetDateFormat())}#";
            }
            else if (dataType.IsNumber())
            {
                this.searchFilterTextBox.PlaceholderText = $"WHERE {simpleColumn.ColumnName} = {sampleSimpleValue}";
            }
            else
            {
                string placeholder = sampleSimpleValue.ToString();
                if (placeholder.Length < 40)
                    this.searchFilterTextBox.PlaceholderText = $"WHERE {simpleColumn.ColumnName} = '{sampleSimpleValue}'";
            }
        }
    }
}
