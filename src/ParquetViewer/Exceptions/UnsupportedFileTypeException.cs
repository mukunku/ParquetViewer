using ParquetViewer.Helpers;
using System;

namespace ParquetViewer.Exceptions;

internal class UnsupportedFileTypeException : Exception
{
    public UnsupportedFileTypeException(string fileExtension)
        //We don't consider file extension private info so we don't wrap it in backticks: `...`
        : base(Resources.Errors.UnsupportedExportTypeFormat.Format(fileExtension))
    {

    }
}
