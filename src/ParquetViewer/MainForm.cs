using ParquetViewer.Analytics;
using ParquetViewer.Controls;
using ParquetViewer.Engine;
using ParquetViewer.Engine.Exceptions;
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
    public partial class MainForm : FormBase
    {
        private const int DefaultOffset = 0;
        private const int DefaultRowCountValue = 1000;
        private readonly string DefaultFormTitle;

        #region Members
        private readonly string? fileToLoadOnLaunch = null;

        private string? _openFileOrFolderPath;
        private string? OpenFileOrFolderPath
        {
            get => this._openFileOrFolderPath;
            set
            {
                this._openFileOrFolderPath = value;
                this._openParquetEngine?.Dispose();
                this._openParquetEngine = null;
                this.SelectedFields = null;
                this.changeFieldsMenuStripButton.Enabled = false;
                this.getSQLCreateTableScriptToolStripMenuItem.Enabled = false;
                this.saveAsToolStripMenuItem.Enabled = false;
                this.metadataViewerToolStripMenuItem.Enabled = false;
                this.recordCountStatusBarLabel.Text = "0";
                this.totalRowCountStatusBarLabel.Text = "0";
                this.actualShownRecordCountLabel.Text = "0";
                this.mainGridView.DisposeAudioCells();
                this.MainDataSource?.Dispose();
                this.MainDataSource = null;
                this.loadAllRowsButton.Enabled = false;
                this.searchFilterTextBox.PlaceholderText = "WHERE ";
                this.offsetTextBox.SetTextQuiet(DefaultOffset.ToString());
                this.currentOffset = DefaultOffset;
                this.mainGridView.ClearQuickPeekForms();
                this.mainGridView.ClearColumnFormatOverrides();
                this.ResetGetSQLCreateTableScriptToolStripMenuItemToolTipText();
                this._queryEditorSavedQueryText = null;

                if (string.IsNullOrWhiteSpace(this._openFileOrFolderPath))
                {
                    this.Text = this.DefaultFormTitle;
                }
                else
                {
                    this.Text = string.Format(
                        File.Exists(this._openFileOrFolderPath) ? Resources.Strings.MainWindowOpenFileTitleFormat : Resources.Strings.MainWindowOpenFolderTitleFormat,
                        this._openFileOrFolderPath);
                    this.changeFieldsMenuStripButton.Enabled = true;
                    this.saveAsToolStripMenuItem.Enabled = true;
                    this.getSQLCreateTableScriptToolStripMenuItem.Enabled = true;
                    this.metadataViewerToolStripMenuItem.Enabled = true;
                }
            }
        }

        private List<string>? selectedFields = null;
        private List<string>? SelectedFields
        {
            get => this.selectedFields;
            set
            {
                this.selectedFields = value?.ToList();

                //Check for duplicate fields (We don't support case sensitive field names unfortunately)
                var duplicateFields = this.selectedFields?.GroupBy(f => f.ToUpperInvariant()).Where(g => g.Count() > 1).SelectMany(g => g).ToList();
                if (duplicateFields?.Count > 0)
                {
                    this.selectedFields = this.selectedFields!.Where(f => !duplicateFields.Any(df => df.Equals(f, StringComparison.InvariantCultureIgnoreCase))).ToList();

                    MessageBox.Show($"The following duplicate fields could not be loaded: {string.Join(',', duplicateFields)}. " +
                            $"{Environment.NewLine}{Environment.NewLine}Case sensitive field names are not currently supported.",
                            "Duplicate fields detected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                LoadFileToGridview();
            }
        }

        private bool IsAnyFileOpen
            => !string.IsNullOrWhiteSpace(this.OpenFileOrFolderPath)
                && this._openParquetEngine is not null;

        private DataTable? mainDataSource;
        private DataTable? MainDataSource
        {
            get => this.mainDataSource;
            set
            {
                this.mainDataSource = value;
                this.mainGridView.DataSource = this.mainDataSource;

                if (this.mainDataSource is not null)
                {
                    this.loadAllRowsButton.Enabled = this.mainDataSource.Rows.Count < (this._openParquetEngine?.RecordCount ?? default);
                    SetSampleQueryAsPlaceHolder();
                }
            }
        }

        private IParquetEngine? _openParquetEngine = null;
        #endregion

        public MainForm()
        {
            this.ForeColor = System.Drawing.Color.Red;
            InitializeComponent();
            this.DefaultFormTitle = this.Text;
            this.offsetTextBox.SetTextQuiet(DefaultOffset.ToString());
            this.recordCountTextBox.SetTextQuiet(DefaultRowCount.ToString());
            this.MainDataSource = new DataTable();
            this.OpenFileOrFolderPath = null;

            //Have to set these here because it gets deleted from the .Designer.cs file for some reason
            this.metadataViewerToolStripMenuItem.Image = Resources.Icons.text_file_icon_16x16.ToBitmap();
            this.iSO8601ToolStripMenuItem.ToolTipText = ExtensionMethods.ISO8601DateTimeFormat;
        }

        public MainForm(string? fileToOpenPath) : this()
        {
            if (fileToOpenPath is not null)
            {
                //The code below will be executed after the default constructor => this()
                this.fileToLoadOnLaunch = fileToOpenPath;
            }
        }

        private async void MainForm_Load(object sender, EventArgs e)
        {
            //Open existing file on first load. Usually this means user double-clicked a parquet file with this utility as the default program.
            if (!string.IsNullOrWhiteSpace(this.fileToLoadOnLaunch))
            {
                await this.OpenNewFileOrFolder(this.fileToLoadOnLaunch);
            }

            //Check necessary toolstrip menu items
            this.RefreshDateFormatMenuItemSelection();
            this.alwaysLoadAllRecordsToolStripMenuItem.Checked = AppSettings.AlwaysLoadAllRecords;
            this.darkModeToolStripMenuItem.Checked = AppSettings.DarkMode;
            this.RefreshExperimentalFeatureToolStrips();
            this.SetLanguageCheckmark();

            //Get user's consent to gather analytics; and update the toolstrip menu item accordingly
            Program.GetUserConsentToGatherAnalytics();
            this.shareAnonymousUsageDataToolStripMenuItem.Checked = AppSettings.AnalyticsDataGatheringConsent;

            //Ask the user if they want to enable dark mode (only if their system is in dark mode)
            Program.AskUserIfTheyWantToSwitchToDarkMode();
        }

        private async Task<List<string>?> OpenFieldSelectionDialog(bool forceOpenDialog)
        {
            if (string.IsNullOrWhiteSpace(this.OpenFileOrFolderPath))
            {
                return null;
            }

            if (this._openParquetEngine == null)
            {
                try
                {
                    this._openParquetEngine = await Engine.ParquetNET.ParquetEngine.OpenFileOrFolderAsync(this.OpenFileOrFolderPath, default);
                }
                catch (Exception ex)
                {
                    if (this._openParquetEngine == null)
                    {
                        //cancel the file open
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
                    else if (ex is Engine.Exceptions.FileReadException fre)
                    {
                        MainForm.HandleFileReadException(fre);
                    }
                    else if (ex is MultipleSchemasFoundException msfe)
                    {
                        HandleMultipleSchemasFoundException(msfe);
                    }
                    else if (ex is FileNotFoundException fnfe)
                    {
                        HandleFileNotFoundException(fnfe);
                    }
                    else if (ex is not OperationCanceledException)
                    {
                        throw;
                    }

                    return null;
                }
            }

            List<string>? fields = null;
            try
            {
                fields = this._openParquetEngine.Fields;
            }
            catch (ArgumentException ex) when (ex.Message.StartsWith("at least one field is required"))
            { /*swallow: This exception is thrown from Parquet.Net when the schema has no fields*/ }
            catch (Exception ex)
            {
                throw new Parquet.ParquetException(Resources.Errors.ParquetSchemaReadErrorMessage, ex);
            }

            if (fields?.Count > 0)
            {
                if (AppSettings.AlwaysSelectAllFields && !forceOpenDialog)
                {
                    return fields.Where(FieldsToLoadForm.IsSupportedFieldType).Select(f => f).ToList();
                }
                else
                {
                    using var fieldSelectionForm = new FieldsToLoadForm(fields, this.MainDataSource?.GetColumnNames() ?? Array.Empty<string>());
                    if (fieldSelectionForm.ShowDialog(this) == DialogResult.OK && fieldSelectionForm.NewSelectedFields?.Count > 0)
                    {
                        return fieldSelectionForm.NewSelectedFields;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            else
            {
                ShowError(Resources.Errors.NoFieldsFoundErrorMessage, Resources.Errors.NoFieldsFoundErrorTitle);
                return null;
            }
        }

        private async void LoadFileToGridview()
        {
            if (this._openParquetEngine is null)
                return;

            try
            {
                await this.LoadFileToGridviewImpl(this._openParquetEngine);
            }
            catch (Exception unhandledEx)
            {
                //Try DuckDB if Parquet.NET fails
                if (this._openParquetEngine is Engine.DuckDB.ParquetEngine)
                    throw;

                try
                {
                    var duckDbEngine = await Engine.DuckDB.ParquetEngine.OpenFileOrFolderAsync(this.OpenFileOrFolderPath!, default);
                    await LoadFileToGridviewImpl(duckDbEngine);
                    this.SwapEngines(duckDbEngine);
                }
                catch (Exception duckDbEx)
                {
                    //If DuckDB fails too, bail
                    throw new RowsReadException(unhandledEx, duckDbEx);
                }
            }
        }

        private async Task LoadFileToGridviewImpl(IParquetEngine engine)
        {
            var stopwatch = Stopwatch.StartNew(); var loadTime = TimeSpan.Zero; var indexTime = TimeSpan.Zero;
            LoadingIcon? loadingIcon = null;
            var wasSuccessful = false;
            try
            {
                if (!this.IsAnyFileOpen)
                    return;

                if (this.SelectedFields is null || this.SelectedFields.Count == 0)
                    return;

                if (!File.Exists(this.OpenFileOrFolderPath) && !Directory.Exists(this.OpenFileOrFolderPath))
                {
                    ShowError(Resources.Errors.OpenFileNoLongerExistsErrorMessageFormat.Format(this.OpenFileOrFolderPath + Environment.NewLine));
                    return;
                }

                long cellCount = this.SelectedFields.Count * Math.Min(this.CurrentMaxRowCount, engine.RecordCount - this.CurrentOffset);
                loadingIcon = this.ShowLoadingIcon(Resources.Strings.LoadingDataLabelText, cellCount);

                var intermediateResult = await Task.Run(async () =>
                {
                    return await engine.ReadRowsAsync(this.SelectedFields, this.CurrentOffset, this.CurrentMaxRowCount, loadingIcon.CancellationToken, loadingIcon);
                }, loadingIcon.CancellationToken);

                loadTime = stopwatch.Elapsed;
                bool showIndexingProgress = false;
                if (loadTime > TimeSpan.FromSeconds(4))
                {
                    //Don't bother showing the indexing step if the data load was really fast because we know 
                    //indexing will be instantaneous. It looks better this way in my opinion.
                    loadingIcon.Reset(Resources.Strings.IndexingDataLabelText);
                    showIndexingProgress = true;
                }

                var finalResult = await Task.Run(() => intermediateResult.Invoke(showIndexingProgress), loadingIcon.CancellationToken);
                indexTime = stopwatch.Elapsed - loadTime;

                this.recordCountStatusBarLabel.Text = string.Format(Resources.Strings.LoadedRecordCountRangeFormat, this.CurrentOffset, this.CurrentOffset + finalResult.Rows.Count);
                this.totalRowCountStatusBarLabel.Text = engine.RecordCount.ToString();
                this.actualShownRecordCountLabel.Text = finalResult.Rows.Count.ToString();

                this.MainDataSource = finalResult;
                wasSuccessful = true;
            }
            catch (AllFilesSkippedException ex)
            {
                HandleAllFilesSkippedException(ex);
            }
            catch (SomeFilesSkippedException ex)
            {
                HandleSomeFilesSkippedException(ex);
            }
            catch (Engine.Exceptions.FileReadException ex)
            {
                MainForm.HandleFileReadException(ex);
            }
            catch (MultipleSchemasFoundException ex)
            {
                HandleMultipleSchemasFoundException(ex);
            }
            catch (MalformedFieldException ex)
            {
                HandleMalformedFieldException(ex);
            }
            catch (DecimalOverflowException ex)
            {
                HandleDecimalOverflowException(ex);
            }
            catch (Exception ex)
            {
                if (ex is not OperationCanceledException)
                    throw;
            }
            finally
            {
                stopwatch.Stop();

                TimeSpan totalTime = stopwatch.Elapsed;
                TimeSpan renderTime = totalTime - loadTime - indexTime;

                //Little secret performance counter
                this.showingStatusBarLabel.ToolTipText = $"Total time: {totalTime:mm\\:ss\\.ff}" + Environment.NewLine +
                $"    Load time: {loadTime:mm\\:ss\\.ff}" + Environment.NewLine +
                $"    Index time: {indexTime:mm\\:ss\\.ff}" + Environment.NewLine +
                $"    Render time: {renderTime:mm\\:ss\\.ff}" + Environment.NewLine +
                $"Engine: {(engine is Engine.ParquetNET.ParquetEngine ? "ParquetNET" : "DuckDB")}";

                loadingIcon?.Dispose();

                if (wasSuccessful)
                {
                    var engineType = this._openParquetEngine is Engine.DuckDB.ParquetEngine
                        ? FileOpenEvent.ParquetEngineTypeId.DuckDB
                        : FileOpenEvent.ParquetEngineTypeId.ParquetNET;

                    FileOpenEvent.FireAndForget(
                        Directory.Exists(this.OpenFileOrFolderPath),
                        engine.NumberOfPartitions,
                        engine.RecordCount,
                        engine.Metadata.RowGroups.Count,
                        engine.Fields.Count,
                        this.MainDataSource!.Columns.Cast<DataColumn>().Select(column => column.DataType.Name).Distinct().Order().ToArray(),
                        this.CurrentOffset,
                        this.CurrentMaxRowCount,
                        this.MainDataSource!.Columns.Count,
                        (long)totalTime.TotalMilliseconds,
                        (long)loadTime.TotalMilliseconds,
                        (long)indexTime.TotalMilliseconds,
                        (long)renderTime.TotalMilliseconds,
                        engineType);
                }
            }
        }

        private async Task OpenNewFileOrFolder(string fileOrFolderPath)
        {
            this.OpenFileOrFolderPath = fileOrFolderPath;

            var fieldList = await this.OpenFieldSelectionDialog(false);
            var wasOpenSuccess = this._openParquetEngine is not null;

            if (wasOpenSuccess && AppSettings.AlwaysLoadAllRecords)
            {
                this.currentMaxRowCount = (int)this._openParquetEngine!.RecordCount;
                this.recordCountTextBox.SetTextQuiet(this._openParquetEngine.RecordCount.ToString());
            }
            else
            {
                this.currentMaxRowCount = DefaultRowCount;
                this.recordCountTextBox.SetTextQuiet(DefaultRowCount.ToString());
            }

            if (fieldList is not null)
            {
                this.SelectedFields = fieldList; //triggers a file load
                AppSettings.OpenedFileCount++;
                Program.AskUserForFileExtensionAssociation();
            }
        }

        /// <summary>
        /// Checks <see cref="AppSettings.DateTimeDisplayFormat"/> and checks/unchecks 
        /// the appropriate date format options located in the menu bar.
        /// </summary>
        private void RefreshDateFormatMenuItemSelection()
        {
            this.defaultToolStripMenuItem.Checked = false;
            this.iSO8601ToolStripMenuItem.Checked = false;
            this.customDateFormatToolStripMenuItem.Checked = false;

            switch (AppSettings.DateTimeDisplayFormat)
            {
                case DateFormat.Default:
                    this.defaultToolStripMenuItem.Checked = true;
                    break;
                case DateFormat.ISO8601:
                    this.iSO8601ToolStripMenuItem.Checked = true;
                    break;
                case DateFormat.Custom:
                    this.customDateFormatToolStripMenuItem.Checked = true;
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Provides the user with a sample query in <see cref="searchFilterTextBox"/> 
        /// using the first primitive field available in the dataset. If none are found,
        /// the placeholder won't contain a sample. Only the "WHERE ".
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

            string placeholder = ParquetGridView.GenerateFilterQuery(simpleColumn.ColumnName, simpleColumn.DataType, sampleSimpleValue);
            if (placeholder.Length < 100) //Only set the placeholder query if it's reasonably short
                this.searchFilterTextBox.PlaceholderText = $"WHERE {placeholder}";
        }


        private void SetLanguageCheckmark()
        {
            if (AppSettings.UserSelectedCulture is not null)
            {
                this.languageToolStripMenuItem.DropDownItems.OfType<ToolStripMenuItem>().ToList().ForEach(languageToolStripItem =>
                {
                    languageToolStripItem.Checked = languageToolStripItem.Tag?.ToString() == AppSettings.UserSelectedCulture.ToString();
                });
            }
            else
            {
                //We default to English
                this.englishToolStripMenuItem.Checked = true;
            }
        }

        private void SwapEngines(IParquetEngine newEngine)
        {
            this._openParquetEngine.DisposeSafely();
            this._openParquetEngine = newEngine;
        }
    }
}