using ParquetViewer.Helpers;
using System;

namespace ParquetViewer.Exceptions;

internal class UnsupportedFileTypeException : Exception
{
    public UnsupportedFileTypeException(string fileExtension)
        : base(Resources.Errors.UnsupportedExportTypeFormat.Format(fileExtension))
    {

    }
}
