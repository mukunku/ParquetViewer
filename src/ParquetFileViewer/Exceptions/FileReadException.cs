using System;

namespace ParquetFileViewer.Exceptions
{
    public class FileReadException : Exception
    {
        public FileReadException(Exception exception) : base("Encountered an error reading file.", exception)
        {

        }
    }
}
