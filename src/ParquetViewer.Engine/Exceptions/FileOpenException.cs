namespace ParquetViewer.Engine.Exceptions;

public class FileOpenException : Exception
{
    public FileOpenException(Exception exception) : base("Encountered an error opening file.", exception)
    {
    }
}