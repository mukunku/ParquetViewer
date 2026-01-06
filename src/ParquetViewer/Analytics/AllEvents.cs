using ParquetViewer.Exceptions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace ParquetViewer.Analytics
{
    public class FileOpenEvent : AmplitudeEvent
    {
        private const string EVENT_TYPE = "file.open";

        public bool IsFolder { get; set; }
        public int NumPartitions { get; set; }
        public long NumRows { get; set; }
        public int NumRowGroups { get; set; }
        public int NumFields { get; set; }
        public string[] FieldTypes { get; set; }
        public long RecordOffset { get; set; }
        public long RecordCount { get; set; }
        public int NumLoadedFields { get; set; }
        public long LoadTimeMS { get; set; }
        public long ReadTimeMS { get; set; }
        public long IndexTimeMS { get; set; }
        public long RenderTimeMS { get; set; }
        [JsonIgnore]
        public ParquetEngineTypeId EngineType { get; set; }
        public string EngineTypeName => EngineType.ToString();

        public FileOpenEvent() : base(EVENT_TYPE)
        {
            this.FieldTypes = [];
        }

        public static void FireAndForget(bool isFolder, int numPartitions, long numRows, int numRowGroups, int numFields,
            string[] fieldTypes, long recordOffset, long recordCount, int numLoadedFields, long totalLoadTimeMilliseconds,
            long readTimeMS, long indexTimeMS, long renderTimeMS, ParquetEngineTypeId engineType)
        {
            var _ = new FileOpenEvent
            {
                IsFolder = isFolder,
                NumPartitions = numPartitions,
                NumRows = numRows,
                NumRowGroups = numRowGroups,
                NumFields = numFields,
                FieldTypes = fieldTypes,
                RecordOffset = recordOffset,
                RecordCount = recordCount,
                NumLoadedFields = numLoadedFields,
                LoadTimeMS = totalLoadTimeMilliseconds,
                ReadTimeMS = readTimeMS,
                IndexTimeMS = indexTimeMS,
                RenderTimeMS = renderTimeMS,
                EngineType = engineType,
            }.Record();
        }

        public enum ParquetEngineTypeId
        {
            ParquetNET,
            DuckDB
        }
    }

    public class FileExportEvent : AmplitudeEvent
    {
        private const string EVENT_TYPE = "file.saveas";

        [JsonIgnore]
        public Helpers.FileType FileType { get; set; }
        public string FileTypeName => FileType.ToString();
        public long FileSize { get; set; }
        public long RowCount { get; set; }
        public int ColumnCount { get; set; }
        public long ExportTimeMS { get; set; }

        public FileExportEvent() : base(EVENT_TYPE)
        {

        }

        public static void FireAndForget(Helpers.FileType fileType, long fileSize, int rowCount, int columnCount, long exportTimeInMilliseconds)
        {
            var _ = new FileExportEvent
            {
                FileType = fileType,
                FileSize = fileSize,
                RowCount = rowCount,
                ColumnCount = columnCount,
                ExportTimeMS = exportTimeInMilliseconds
            }.Record();
        }
    }

    public class MenuBarClickEvent : AmplitudeEvent
    {
        private const string EVENT_TYPE = "menubar.click";

        [JsonIgnore]
        public ActionId Action { get; set; }

        public string ActionName => Action.ToString();

        public MenuBarClickEvent() : base(EVENT_TYPE)
        {

        }

        public static void FireAndForget(ActionId action)
        {
            var _ = new MenuBarClickEvent { Action = action }.Record();
        }

        public enum ActionId
        {
            None = 0,
            FileNew,
            FileOpen,
            FolderOpen,
            Exit,
            ChangeFields,
            SQLCreateTable,
            MetadataViewer,
            AboutBox,
            UserGuide,
            DragDrop,
            LoadAllRows
        }
    }

    public class ExceptionEvent : AmplitudeEvent
    {
        public const string MASK_SENTINEL = "*****";

        private const string EVENT_TYPE = "exception.thrown";

        [JsonIgnore]
        public System.Exception Exception { get; }

        public string Message
        {
            get
            {
                var message = Exception.Message;

                if (string.IsNullOrWhiteSpace(message))
                    return message;

                //Mask sensitive text that is wrapped with ` `
                message = Regex.Replace(message, "`[^`]+`", MASK_SENTINEL);
                return message;
            }
        }

        public string? StackTrace => Exception.StackTrace?.ToString();
        public string? InnerException => Exception.InnerException?.ToString();
        public string Type => Exception.GetType().Name;

        [JsonExtensionData]
        public IDictionary<string, object> AdditionalData
        {
            get
            {
                var dictionary = new Dictionary<string, object>();
                foreach (DictionaryEntry keyValuePair in this.Exception.Data)
                {
                    if (keyValuePair.Key is string key && keyValuePair.Value is not null)
                    {
                        dictionary.Add(key, keyValuePair.Value);
                    }
                }
                return dictionary;
            }
        }

        public ExceptionEvent(Exception ex, AmplitudeConfiguration? amplitudeConfiguration = null)
            : base(EVENT_TYPE, amplitudeConfiguration)
        {
            this.Exception = ex ?? throw new ArgumentNullException(nameof(ex));
        }

        public static void FireAndForget(Exception ex)
        {
            if (ex is RowsReadException rre)
            {
                //Record two separate exceptions for both parquet.net and duckdb
                var _ = new ExceptionEvent(rre.ParquetNetException).Record()
                    .ContinueWith((_) => _ = new ExceptionEvent(rre.DuckDbException).Record());
            }
            else
            {
                var _ = new ExceptionEvent(ex).Record();
            }
        }
    }

    public class QuickPeekEvent : AmplitudeEvent
    {
        private const string EVENT_TYPE = "quickpeek.show";

        [JsonIgnore]
        public DataTypeId DataType { get; set; }

        public string DataTypeName => this.DataType.ToString();

        public QuickPeekEvent() : base(EVENT_TYPE)
        {

        }

        public enum DataTypeId
        {
            Unknown,
            List,
            Map,
            Struct,
            Image,
            Audio,
        }

        public static void FireAndForget(DataTypeId dataType)
        {
            var _ = new QuickPeekEvent { DataType = dataType }.Record();
        }
    }

    public class ExecuteQueryEvent : AmplitudeEvent
    {
        private const string EVENT_TYPE = "sql.execute";

        public bool IsValid { get; set; }
        public int RecordCountTotal { get; set; }
        public int? RecordCountFiltered { get; set; }
        public int ColumnCount { get; set; }
        public long RunTimeMS { get; set; }

        public ExecuteQueryEvent() : base(EVENT_TYPE)
        {

        }
    }

    public class ColumnFormattedEvent : AmplitudeEvent
    {
        private const string EVENT_TYPE = "column.format";

        public string FormatName { get; }

        public ColumnFormattedEvent(string? formatName = null) : base(EVENT_TYPE)
        {
            FormatName = formatName ?? string.Empty;
        }

        public static void FireAndForget(string? formatName)
        {
            var _ = new ColumnFormattedEvent(formatName).Record();
        }
    }
}