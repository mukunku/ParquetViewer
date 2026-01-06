using System;

namespace ParquetViewer.Exceptions
{
    public class InvalidQueryException : Exception
    {
        public InvalidQueryException(Exception? ex = null) : base(Resources.Errors.InvalidQueryErrorMessage, ex) { }
    }
}