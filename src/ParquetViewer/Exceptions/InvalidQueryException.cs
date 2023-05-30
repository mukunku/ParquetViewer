using System;

namespace ParquetViewer.Exceptions
{
    public class InvalidQueryException : Exception
    {
        public InvalidQueryException(Exception ex = null) : base("The query doesn't seem to be valid. Please try again.", ex) { }
    }
}
