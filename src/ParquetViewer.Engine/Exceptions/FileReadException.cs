using System;

namespace ParquetViewer.Engine.Exceptions
{
    public class FileReadException : Exception
    {
        public FileReadException(Exception exception) : base("Encountered an error reading file.", exception)
        {

        }
    }
}
