namespace ParquetViewer.Engine.Exceptions
{
    internal class ParquetEngineException : Exception
    {
        public ParquetEngineException(string? message = null, Exception? exception = null) : base(message, exception)
        {

        }
    }
}
