using System;
using System.Security.Principal;
using System.Threading;

namespace ParquetViewer.Helpers
{
    public static class Constants
    {
        public const string WikiURL = "https://github.com/mukunku/ParquetViewer/wiki";
        public const string DateFormatDocsURL = "https://docs.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings";
    }

    public static class User
    {
        public static bool IsAdministrator =>
            new WindowsPrincipal(WindowsIdentity.GetCurrent())
                .IsInRole(WindowsBuiltInRole.Administrator);

        public static char NumberDecimalSeparator = 
            Thread.CurrentThread.CurrentUICulture.NumberFormat.NumberDecimalSeparator.ToCharArray()[0];
    }

    public enum ParquetEngine
    {
        Default = 0,
        Default_Multithreaded
    }

    public enum DateFormat
    {
        Default = 0,
        ISO8601 = 2,
        [Obsolete]
        ISO8601_Alt1 = 4,
        [Obsolete]
        ISO8601_Alt2 = 5,
        Custom = 6
    }

    public enum FileType
    {
        CSV = 0,
        XLS,
        JSON,
        PARQUET
    }

    public enum AutoSizeColumnsMode
    {
        //Needs to match System.Windows.Forms.DataGridViewAutoSizeColumnsMode values
        None = 1,
        ColumnHeader = 2,
        AllCells = 6
    }
}
