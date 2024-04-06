using System;
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

        public FileOpenEvent() : base(EVENT_TYPE)
        {
            this.FieldTypes = [];
        }

        public static void FireAndForget(bool isFolder, int numPartitions, long numRows, int numRowGroups, int numFields,
            string[] fieldTypes, long recordOffset, long recordCount, int numLoadedFields, long loadTimeMilliseconds)
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
                LoadTimeMS = loadTimeMilliseconds
            }.Record();
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
        public System.Exception? Exception { get; set; }

        public string? Message
        {
            get
            {
                var message = Exception?.Message;

                if (string.IsNullOrWhiteSpace(message))
                    return message;

                //Mask sensitive text that is wrapped with ` `
                message = Regex.Replace(message, "`[^`]+`", MASK_SENTINEL);
                return message;
            }
        }

        public string? StackTrace => Exception?.StackTrace?.ToString();
        public string? InnerException => Exception?.InnerException?.ToString();

        public ExceptionEvent(AmplitudeConfiguration? amplitudeConfiguration = null)
            : base(EVENT_TYPE, amplitudeConfiguration)
        {

        }

        public static void FireAndForget(System.Exception ex)
        {
            var _ = new ExceptionEvent { Exception = ex }.Record();
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
            Image
        }

        public static void FireAndForget(DataTypeId dataType)
        {
            var _ = new QuickPeekEvent { DataType = dataType }.Record();
        }
    }
}
