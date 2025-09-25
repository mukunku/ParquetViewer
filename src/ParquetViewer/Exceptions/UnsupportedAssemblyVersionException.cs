using ParquetViewer.Analytics;
using System;

namespace ParquetViewer.Exceptions
{
    internal class UnsupportedAssemblyVersionException : Exception
    {
        public UnsupportedAssemblyVersionException(string unsupportedAssemblyVersion, Exception? ex = null) : base($"An unexpected assembly version was encountered: {unsupportedAssemblyVersion}", ex) { }

        public static void Record(string unsupportedAssemblyVersion) => ExceptionEvent.FireAndForget(new UnsupportedAssemblyVersionException(unsupportedAssemblyVersion));
    }
}
