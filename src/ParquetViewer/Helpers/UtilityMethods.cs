using System;

namespace ParquetViewer.Helpers
{
    public static class UtilityMethods
    {
        public static string CleanCSVValue(string value, bool alwaysEncloseInQuotes = false)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                //In RFC 4180 we escape quotes with double quotes
                string formattedValue = value.Replace("\"", "\"\"");

                //Enclose value with quotes if it contains commas, line feeds, or other quotes
                foreach (char c in formattedValue)
                {
                    if (c == ',' || c == '\r' || c == '\n' || c == '\"' || alwaysEncloseInQuotes)
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
    }
}
