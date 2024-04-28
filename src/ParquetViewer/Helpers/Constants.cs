namespace ParquetViewer.Helpers
{
    public static class Constants
    {
        public const string FILL_WEIGHT_EXCEPTION_MESSAGE = "FillWeight";
        public const string WikiURL = "https://github.com/mukunku/ParquetViewer/wiki";
    }

    public enum ParquetEngine
    {
        Default = 0,
        Default_Multithreaded
    }

    public enum DateFormat
    {
        Default = 0,
        Default_DateOnly, // obsolete
        ISO8601,
        ISO8601_DateOnly, // obsolete
        ISO8601_Alt1,
        ISO8601_Alt2
    }

    public enum FileType
    {
        CSV = 0,
        XLS
    }

    public enum AutoSizeColumnsMode
    {
        //Needs to match System.Windows.Forms.DataGridViewAutoSizeColumnsMode values
        None = 1,
        ColumnHeader = 2,
        AllCells = 6
    }
}
