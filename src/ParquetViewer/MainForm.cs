using ParquetViewer.Analytics;
using ParquetViewer.Controls;
using ParquetViewer.Engine;
using ParquetViewer.Engine.Exceptions;
using ParquetViewer.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ParquetViewer;

public partial class MainForm : FormBase
{
    private const int DefaultOffset = 0;
    private const int DefaultRowCountValue = 1000;
    private readonly string _defaultFormTitle;

    #region Members
    private readonly string? _fileToLoadOnLaunch = null;
    private string? _openFileOrFolderPath;
    private string? OpenFileOrFolderPath
    {
        get => _openFileOrFolderPath;
        set
        {
            _openFileOrFolderPath = value;
            _openParquetEngine?.Dispose();
            _openParquetEngine = null;
            SelectedFields = null;
            changeFieldsMenuStripButton.Enabled = false;
            getSQLCreateTableScriptToolStripMenuItem.Enabled = false;
            saveAsToolStripMenuItem.Enabled = false;
            metadataViewerToolStripMenuItem.Enabled = false;
            recordCountStatusBarLabel.Text = "0";
            totalRowCountStatusBarLabel.Text = "0";
            actualShownRecordCountLabel.Text = "0";
            mainGridView.DisposeAudioCells();
            MainDataSource?.Dispose();
            MainDataSource = null;
            loadAllRowsButton.Enabled = false;
            searchFilterTextBox.PlaceholderText = "WHERE ";
            offsetTextBox.SetTextQuiet(DefaultOffset.ToString());
            _currentOffset = DefaultOffset;
            mainGridView.ClearQuickPeekForms();
            mainGridView.ClearColumnFormatOverrides();
            ResetGetSQLCreateTableScriptToolStripMenuItemToolTipText();

            if (string.IsNullOrWhiteSpace(_openFileOrFolderPath))
            {
                Text = _defaultFormTitle;
            }
            else
            {
                if (File.Exists(_openFileOrFolderPath))
                    Text = string.Format(Resources.Strings.MainWindowOpenFileTitleFormat, _openFileOrFolderPath);
                else
                    Text = string.Format(Resources.Strings.MainWindowOpenFolderTitleFormat, _openFileOrFolderPath);

                changeFieldsMenuStripButton.Enabled = true;
                saveAsToolStripMenuItem.Enabled = true;
                getSQLCreateTableScriptToolStripMenuItem.Enabled = true;
                metadataViewerToolStripMenuItem.Enabled = true;
            }
        }
    }

    private List<string>? _selectedFields = null;
    private List<string>? SelectedFields
    {
        get => _selectedFields;
        set
        {
            _selectedFields = value?.ToList();

            //Check for duplicate fields (We don't support case sensitive field names unfortunately)
            var duplicateFields = _selectedFields?.GroupBy(f => f.ToUpperInvariant()).Where(g => g.Count() > 1).SelectMany(g => g).ToList();
            if (duplicateFields?.Count > 0)
            {
                _selectedFields = _selectedFields!.Where(f => !duplicateFields.Any(df => df.Equals(f, StringComparison.InvariantCultureIgnoreCase))).ToList();

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

    private int _currentOffset = DefaultOffset;
    private int CurrentOffset
    {
        get => _currentOffset;
        set
        {
            _currentOffset = value;
            LoadFileToGridview();
        }
    }

    private static int DefaultRowCount => DefaultRowCountValue;

    private int _currentMaxRowCount = DefaultRowCount;
    private int CurrentMaxRowCount
    {
        get => _currentMaxRowCount;
        set
        {
            _currentMaxRowCount = value;
            LoadFileToGridview();
        }
    }

    private bool IsAnyFileOpen
        => !string.IsNullOrWhiteSpace(OpenFileOrFolderPath)
            && _openParquetEngine is not null;

    private DataTable? _mainDataSource;
    private DataTable? MainDataSource
    {
        get => _mainDataSource;
        set
        {
            _mainDataSource = value;
            mainGridView.DataSource = _mainDataSource;

            if (_mainDataSource is not null)
            {
                loadAllRowsButton.Enabled = _mainDataSource.Rows.Count < (_openParquetEngine?.RecordCount ?? default);
                SetSampleQueryAsPlaceHolder();
            }
        }
    }

    private IParquetEngine? _openParquetEngine = null;

    private (DateTime LastWriteTimeUtc, long Length)? _originalModifiedInfo;
    #endregion

    public MainForm()
    {
        InitializeComponent();
        _defaultFormTitle = Text;
        offsetTextBox.SetTextQuiet(DefaultOffset.ToString());
        recordCountTextBox.SetTextQuiet(DefaultRowCount.ToString());
        MainDataSource = new DataTable();
        OpenFileOrFolderPath = null;

        //Have to set these here because it gets deleted from the .Designer.cs file for some reason
        metadataViewerToolStripMenuItem.Image = Resources.Icons.text_file_icon_16x16.ToBitmap();
        iSO8601ToolStripMenuItem.ToolTipText = ExtensionMethods.ISO8601DateTimeFormat;
    }

    public MainForm(string? fileToOpenPath) : this()
    {
        if (fileToOpenPath is not null)
        {
            //The code below will be executed after the default constructor => this()
            _fileToLoadOnLaunch = fileToOpenPath;
        }
    }

    private async void MainForm_Load(object sender, EventArgs e)
    {
        //Open existing file on first load. Usually this means user double-clicked a parquet file with this utility as the default program.
        if (!string.IsNullOrWhiteSpace(_fileToLoadOnLaunch))
        {
            await OpenNewFileOrFolder(_fileToLoadOnLaunch);
        }

        //Check necessary toolstrip menu items
        RefreshDateFormatMenuItemSelection();
        alwaysLoadAllRecordsToolStripMenuItem.Checked = AppSettings.AlwaysLoadAllRecords;
        darkModeToolStripMenuItem.Checked = AppSettings.DarkMode;
        RefreshExperimentalFeatureToolStrips();
        SetLanguageCheckmark();

        //Get user's consent to gather analytics; and update the toolstrip menu item accordingly
        Program.GetUserConsentToGatherAnalytics();
        shareAnonymousUsageDataToolStripMenuItem.Checked = AppSettings.AnalyticsDataGatheringConsent;

        //Ask the user if they want to enable dark mode (only if their system is in dark mode)
        Program.AskUserIfTheyWantToSwitchToDarkMode();
    }

    private async Task<List<string>?> OpenFieldSelectionDialog(bool forceOpenDialog)
    {
        if (string.IsNullOrWhiteSpace(OpenFileOrFolderPath))
        {
            return null;
        }

        if (_openParquetEngine is null)
        {
            try
            {
                _openParquetEngine = await Engine.ParquetNET.ParquetEngine.OpenFileOrFolderAsync(OpenFileOrFolderPath);
            }
            catch (Exception ex)
            {
                if (_openParquetEngine is null)
                {
                    //cancel the file open
                    OpenFileOrFolderPath = null;
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
            fields = _openParquetEngine.Fields;
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
                return fields;
            }
            else
            {
                using var fieldSelectionForm = new FieldsToLoadForm(fields, MainDataSource?.GetColumnNames() ?? Array.Empty<string>());
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
        if (_openParquetEngine is null)
            return;

#if RELEASE_SELFCONTAINED
        //Self contained release has both Parquet.NET and DuckDB engines included as the file size remains the same.
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
                var duckDbEngine = await Engine.DuckDB.ParquetEngine.OpenFileOrFolderAsync(this.OpenFileOrFolderPath!);
                await LoadFileToGridviewImpl(duckDbEngine);
                SwapEngines(duckDbEngine);
            }
            catch (Exception duckDbEx)
            {
                //If DuckDB fails too, bail
                throw new Exceptions.RowsReadException(unhandledEx, duckDbEx);
            }
        }

        void SwapEngines(IParquetEngine newEngine)
        {
            this._openParquetEngine.DisposeSafely();
            this._openParquetEngine = newEngine;
        }
#else
        await LoadFileToGridviewImpl(_openParquetEngine);
#endif

        _originalModifiedInfo = null;
    }

    private async Task LoadFileToGridviewImpl(IParquetEngine engine)
    {
        var stopwatch = Stopwatch.StartNew(); var loadTime = TimeSpan.Zero; var indexTime = TimeSpan.Zero;
        LoadingIcon? loadingIcon = null;
        var wasSuccessful = false;
        try
        {
            if (!IsAnyFileOpen)
                return;

            if (SelectedFields is null || SelectedFields.Count == 0)
                return;

            if (!File.Exists(OpenFileOrFolderPath) && !Directory.Exists(OpenFileOrFolderPath))
            {
                ShowError(Resources.Errors.OpenFileNoLongerExistsErrorMessageFormat.Format(OpenFileOrFolderPath + Environment.NewLine));
                return;
            }

            long cellCount = SelectedFields.Count * Math.Min(CurrentMaxRowCount, engine.RecordCount - CurrentOffset);
            loadingIcon = ShowLoadingIcon(Resources.Strings.LoadingDataLabelText, cellCount);

            var intermediateResult = await Task.Run(async () =>
            {
                return await engine.ReadRowsAsync(SelectedFields, CurrentOffset, CurrentMaxRowCount, loadingIcon.CancellationToken, loadingIcon);
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

            recordCountStatusBarLabel.Text = string.Format(Resources.Strings.LoadedRecordCountRangeFormat, CurrentOffset, CurrentOffset + finalResult.Rows.Count);
            totalRowCountStatusBarLabel.Text = engine.RecordCount.ToString();
            actualShownRecordCountLabel.Text = finalResult.Rows.Count.ToString();

            MainDataSource = finalResult;
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
        catch (FileReadException ex)
        {
            HandleFileReadException(ex);
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
            showingStatusBarLabel.ToolTipText = $"Total time: {totalTime:mm\\:ss\\.ff}" + Environment.NewLine +
            $"    Load time: {loadTime:mm\\:ss\\.ff}" + Environment.NewLine +
            $"    Index time: {indexTime:mm\\:ss\\.ff}" + Environment.NewLine +
            $"    Render time: {renderTime:mm\\:ss\\.ff}" + Environment.NewLine +
            $"Engine: {(engine is Engine.ParquetNET.ParquetEngine ? "ParquetNET" : "DuckDB")}";

            loadingIcon?.Dispose();

            if (wasSuccessful)
            {
                var engineType = _openParquetEngine is Engine.ParquetNET.ParquetEngine
                    ? FileOpenEvent.ParquetEngineTypeId.ParquetNET
                    : FileOpenEvent.ParquetEngineTypeId.DuckDB;

                FileOpenEvent.FireAndForget(
                    Directory.Exists(OpenFileOrFolderPath),
                    engine.NumberOfPartitions,
                    engine.RecordCount,
                    engine.Metadata.RowGroups.Count,
                    engine.Fields.Count,
                    MainDataSource!.Columns.Cast<DataColumn>().Select(column => column.DataType.Name).Distinct().Order().ToArray(),
                    CurrentOffset,
                    CurrentMaxRowCount,
                    MainDataSource!.Columns.Count,
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
        OpenFileOrFolderPath = fileOrFolderPath;

        var fieldList = await OpenFieldSelectionDialog(false);
        var wasOpenSuccess = _openParquetEngine is not null;

        if (wasOpenSuccess && AppSettings.AlwaysLoadAllRecords)
        {
            var recordCount = _openParquetEngine!.RecordCount;
            if (recordCount == 0 || recordCount > int.MaxValue)
                recordCount = DefaultRowCount;

            _currentMaxRowCount = (int)recordCount;
            recordCountTextBox.SetTextQuiet(recordCount.ToString());
        }
        else
        {
            _currentMaxRowCount = DefaultRowCount;
            recordCountTextBox.SetTextQuiet(DefaultRowCount.ToString());
        }

        if (fieldList is not null)
        {
            SelectedFields = fieldList; //triggers a file load
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
        defaultToolStripMenuItem.Checked = false;
        iSO8601ToolStripMenuItem.Checked = false;
        customDateFormatToolStripMenuItem.Checked = false;

        switch (AppSettings.DateTimeDisplayFormat)
        {
            case DateFormat.Default:
                defaultToolStripMenuItem.Checked = true;
                break;
            case DateFormat.ISO8601:
                iSO8601ToolStripMenuItem.Checked = true;
                break;
            case DateFormat.Custom:
                customDateFormatToolStripMenuItem.Checked = true;
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
        searchFilterTextBox.PlaceholderText = "WHERE ";

        if (MainDataSource is null || MainDataSource.Rows.Count == 0)
            return;

        var simpleColumn = MainDataSource.Columns.AsEnumerable().FirstOrDefault(c => c.DataType.IsSimple());
        if (simpleColumn is null)
            return;

        //find a value we can use as a sample
        object sampleSimpleValue = DBNull.Value; int counter = 1000;
        foreach (DataRow row in MainDataSource.Rows)
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
            searchFilterTextBox.PlaceholderText = $"WHERE {placeholder}";
    }


    private void SetLanguageCheckmark()
    {
        if (AppSettings.UserSelectedCulture is not null)
        {
            languageToolStripMenuItem.DropDownItems.OfType<ToolStripMenuItem>().ToList().ForEach(languageToolStripItem =>
            {
                languageToolStripItem.Checked = languageToolStripItem.Tag?.ToString() == AppSettings.UserSelectedCulture.ToString();
            });
        }
        else
        {
            //We default to English
            englishToolStripMenuItem.Checked = true;
        }
    }
}