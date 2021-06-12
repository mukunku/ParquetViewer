namespace ParquetFileViewer
{
    public static class Constants
    {
        public const string FILL_WEIGHT_EXCEPTION_MESSAGE = "FillWeight";
        public const string ISO8601_DATETIME_FORMAT = "yyyy-MM-ddTHH:mm:ss.fffZ";
    }

    public enum ParquetEngine
    {
        Default,
        Default_Multithreaded
    }
}
