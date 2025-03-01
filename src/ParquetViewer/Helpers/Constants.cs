using System;
using System.Drawing;
using System.Security.Principal;
using System.Threading;
using System.Windows.Forms;

namespace ParquetViewer.Helpers
{
    public static class Constants
    {
        public const string WikiURL = "https://github.com/mukunku/ParquetViewer/wiki";
        public const string DateFormatDocsURL = "https://docs.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings";

        public static Theme DarkModeTheme => new Theme(
            Color.FromArgb(13, 17, 23),
            Color.FromArgb(240, 246, 252),
            Color.FromArgb(21, 27, 35),
            Color.FromArgb(30, 34, 38),
            Color.FromArgb(30, 34, 38),
            DataGridViewHeaderBorderStyle.Single,
            Color.FromArgb(33, 33, 33),
            Color.FromArgb(61, 68, 77),
            Color.FromArgb(68, 147, 248),
            Color.FromArgb(145, 152, 161)
            );

        public static Theme LightModeTheme => new Theme(
            Color.White,
            SystemColors.WindowText,
            Color.White,
            SystemColors.ControlLight,
            Color.Gray,
            DataGridViewHeaderBorderStyle.Raised,
            Color.FromArgb(160, 160, 160),
            Color.FromArgb(100, 100, 100),
            Color.Blue,
            SystemColors.ActiveCaptionText
            );
    }

    public static class User
    {
        public static bool IsAdministrator =>
            new WindowsPrincipal(WindowsIdentity.GetCurrent())
                .IsInRole(WindowsBuiltInRole.Administrator);

        public static char NumberDecimalSeparator = 
            Thread.CurrentThread.CurrentUICulture.NumberFormat.NumberDecimalSeparator.ToCharArray()[0];

        public static Theme PreferredTheme => AppSettings.DarkMode ? Constants.DarkModeTheme : Constants.LightModeTheme;
    }

    public record Theme(
            Color CellBackgroundColor,
            Color TextColor,
            Color AlternateRowsCellBackgroundColor,
            Color ColumnHeaderColor,
            Color RowHeaderColor,
            DataGridViewHeaderBorderStyle RowHeaderBorderStyle,
            Color GridBackgroundColor,
            Color GridColor,
            Color HyperlinkColor,
            Color CellPlaceholderTextColor
            );

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
