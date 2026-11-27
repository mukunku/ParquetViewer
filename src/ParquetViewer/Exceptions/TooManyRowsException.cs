using System;

namespace ParquetViewer.Exceptions;

/// <summary>
/// Thrown when the grid can't display the loaded rows because laying out its scroll bar overflows.
/// </summary>
public class TooManyRowsException : Exception
{
    public TooManyRowsException(Exception innerException) : base("Too many rows to display.", innerException)
    {
    }
}