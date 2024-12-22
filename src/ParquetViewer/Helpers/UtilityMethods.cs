﻿using System;
using System.Windows.Forms;

namespace ParquetViewer.Helpers
{
    public static class UtilityMethods
    {
        /// <summary>
        /// Formats a value to make it is RFC-4180 compliant
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
            foreach (FileType fileType in Enum.GetValues(typeof(FileType)))
            {
                if (fileType.GetExtension().Equals(extension))
                    return fileType;
            }
            return null;
        }

        public static bool IsValidDateFormat(string? dateFormat)
        {
            if (string.IsNullOrWhiteSpace(dateFormat))
                return false;

            try
            {
                DateTime.UtcNow.ToString(dateFormat);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
