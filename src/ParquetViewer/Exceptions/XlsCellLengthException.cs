using System;

namespace ParquetViewer.Exceptions
{
    public class XlsCellLengthException(string? message = null, Exception? innerException = null) : Exception(message, innerException)
    {
    }
}
