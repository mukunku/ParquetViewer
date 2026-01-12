namespace ParquetViewer.Engine.Exceptions
{
    public class ParquetEngineException : Exception
    {
        public ParquetEngineException(string? message = null, Exception? exception = null) : base(message, exception)
        {

        }
    }
}