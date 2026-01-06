namespace ParquetViewer.Engine.Exceptions
{
    public class TypeMismatchException : Exception
    {
        public TypeMismatchException(string? message, Exception? ex = null) : base(message, ex)
        {

        }
    }
}