using System;

namespace ParquetViewer.Exceptions;

internal class UnsupportedFileTypeException : Exception
{
    public UnsupportedFileTypeException(string message, Exception innerException)
        : base(message, innerException)
    {

    }

    public UnsupportedFileTypeException(string fileExtension)
        //We don't consider file extension private info so we don't wrap it in backticks: `...`
        : base($"Unsupported file extension '{fileExtension}'")
    {

    }
}