namespace ParquetViewer.Engine.Exceptions
{
    public class MalformedFieldException : Exception
    {
        public MalformedFieldException(string message, Exception? ex = null) : base(message, ex) { }
    }
}