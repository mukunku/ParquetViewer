namespace ParquetViewer.Engine.Exceptions
{
    public class UnsupportedFieldException : Exception
    {
        public UnsupportedFieldException(string message, Exception? ex = null) : base(message, ex)
        {

        }
    }
}
