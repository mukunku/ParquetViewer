using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace ParquetViewer.Helpers;

public static class UtilityMethods
{
    /// <summary>
    /// Formats a value to make it RFC-4180 compliant
    /// </summary>
    /// <param name="value">Raw string value to be added to a CSV file</param>
    /// <returns>Possibly formatted value</returns>
    public static string CleanCSVValue(string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            //Escape quotes with double quotes
            string formattedValue = value.Replace("\"", "\"\"");

            //Enclose value with quotes if it contains commas, line feeds, or other quotes
            foreach (char c in formattedValue)
            {
                if (c == ',' || c == '\r' || c == '\n' || c == '\"')
                {
                    formattedValue = string.Concat("\"", formattedValue, "\"");
                    break;
                }
            }

            return formattedValue;
        }
        else
            return string.Empty;
    }

    /// <summary>
    /// Returns a <see cref="FileType"/> if a matching one is found for a given file extension.
    /// </summary>
    /// <param name="extension">File extension in ".xyz" format</param>
    /// <returns>null if no matching file type is found</returns>
    public static FileType? ExtensionToFileType(string extension)
    {
        foreach (FileType fileType in Enum.GetValues<FileType>())
        {
            if (fileType.GetExtension().Equals(extension))
                return fileType;
        }
        return null;
    }

    public static bool IsSupportedDateFormat(string? dateFormat)
    {
        if (string.IsNullOrWhiteSpace(dateFormat))
            return false;

        try
        {
            //Test on DateTime
            _ = DateTime.UtcNow.ToString(dateFormat);

            //Test on DateOnly
            var dateOnlyFormat = UtilityMethods.StripTimeComponentsFromDateTimeFormat(dateFormat);
            _ = DateOnly.FromDateTime(DateTime.UtcNow).ToString(dateOnlyFormat);

            //Test on TimeOnly
            var timeOnlyFormat = UtilityMethods.StripDateComponentsFromDateTimeFormat(dateFormat);
            _ = TimeOnly.FromDateTime(DateTime.UtcNow).ToString(timeOnlyFormat);

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public static bool TryParseCultureInfo(string? cultureString, [NotNullWhen(true)] out CultureInfo? cultureInfo)
    {
        if (string.IsNullOrWhiteSpace(cultureString))
            throw new ArgumentNullException(nameof(cultureString));

        try
        {
            cultureInfo = new CultureInfo(cultureString);
            return true;
        }
        catch (CultureNotFoundException)
        {
            cultureInfo = null;
            return false;
        }
    }

    /// <summary>
    /// Starts a new ParquetViewer instance and terminates the current one.
    /// </summary>
    public static void RestartApplication()
    {
        //Start a new ParquetViewer instance
        var startInfo = new ProcessStartInfo
        {
            UseShellExecute = true, // Do not wait - make the process standalone
            FileName = Application.ExecutablePath
        };
        Process.Start(startInfo);

        //Terminate this instance
        Application.Exit();
    }

    private static readonly char[] _dateTimeSeparatorChars = ['/', '-', '.', ' ', ',', '_', ':', '\u202F', '\u200F'];

    /// <summary>
    /// Best effort attempt at stripping time components from a datetime format string.
    /// </summary>
    /// <param name="dateFormat">Date format with potential time components</param>
    /// <returns>Date format with no time components</returns>
    /// <remarks>If empty string is returned, formatting with it will use the users default culture format</remarks>
    public static string StripTimeComponentsFromDateTimeFormat(string dateFormat)
    {
        var timeComponents = new string[] { "H", "h", "m", "s", "f", "F", "t", "z", "K", "'ч'" };
        foreach (var component in timeComponents)
        {
            dateFormat = dateFormat.Replace(component, string.Empty);
        }
        dateFormat = Regex.Replace(dateFormat, @"\s{2,}", " ");
        dateFormat = dateFormat.Trim(_dateTimeSeparatorChars);
        return dateFormat.Trim();
    }

    /// <summary>
    /// Best effort attempt at stripping date components from a datetime format string.
    /// </summary>
    /// <param name="dateFormat">Date format with potential date components</param>
    /// <returns>Time format with no date components</returns>
    /// <remarks>If empty string is returned, formatting with it will use the users default culture format</remarks>
    public static string StripDateComponentsFromDateTimeFormat(string dateFormat)
    {
        var dateComponents = new string[] { "y", "M", "d", "g" };
        foreach (var component in dateComponents)
        {
            dateFormat = dateFormat.Replace(component, string.Empty);
        }
        dateFormat = Regex.Replace(dateFormat, @"\s{2,}", " ");
        dateFormat = dateFormat.Trim(_dateTimeSeparatorChars);
        return dateFormat.Trim();
    }
}