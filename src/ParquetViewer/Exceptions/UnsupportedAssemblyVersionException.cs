using ParquetViewer.Analytics;
using System;

namespace ParquetViewer.Exceptions
{
    internal class UnsupportedAssemblyVersionException : Exception
    {
        public UnsupportedAssemblyVersionException(string unsupportedAssemblyVersion, Exception? ex = null) 
            : base(string.Format(Resources.Errors.UnexpectedAssemblyVersionErrorFormat, unsupportedAssemblyVersion), ex) { }

        public static void Record(string unsupportedAssemblyVersion) => ExceptionEvent.FireAndForget(new UnsupportedAssemblyVersionException(unsupportedAssemblyVersion));
    }
}
