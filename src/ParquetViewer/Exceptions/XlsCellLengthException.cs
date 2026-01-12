using ParquetViewer.Helpers;
using System;

namespace ParquetViewer.Exceptions
{
    public class XlsCellLengthException : Exception
    {
        public readonly FileType FileType = FileType.XLS;

        public int MaxLength { get; }

        public XlsCellLengthException(int maxLength, string? message = null, Exception? innerException = null)
           : base(message, innerException)
        {
            this.MaxLength = maxLength;
        }
    }
}