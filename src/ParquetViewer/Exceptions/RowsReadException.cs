using System;

namespace ParquetViewer.Exceptions
{
    public class RowsReadException : Exception
    {
        public RowsReadException(Exception parquetNetEx, Exception duckDbEx, string? message = null) : base(message, new AggregateException([parquetNetEx, duckDbEx]))
        {
            this.ParquetNetException = parquetNetEx;
            this.DuckDbException = duckDbEx;
        }

        public Exception ParquetNetException { get; }
        public Exception DuckDbException { get; }
    }
}