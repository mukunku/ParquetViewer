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
        ISO8601 = 2,
        ISO8601_Alt1 = 4,
        ISO8601_Alt2 = 5
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
