namespace ParquetFileViewer
{
    public static class Constants
    {
        public const string FILL_WEIGHT_EXCEPTION_MESSAGE = "FillWeight";
    }

    public enum ParquetEngine
    {
        Default = 0,
        Default_Multithreaded
    }

    public enum DateFormat
    {
        Default = 0,
        Default_DateOnly,
        ISO8601,
        ISO8601_DateOnly,
        ISO8601_Alt1
    }
}
