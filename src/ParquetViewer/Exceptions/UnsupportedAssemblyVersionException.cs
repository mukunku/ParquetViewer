using System;

namespace ParquetViewer.Exceptions
{
    internal class UnsupportedAssemblyVersionException : Exception
    {
        public UnsupportedAssemblyVersionException(Exception ex = null) : base("An unexpected assembly version was encountered. Is this a one off release?", ex) { }
    }
}
