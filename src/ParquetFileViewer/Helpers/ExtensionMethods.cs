using System;
using System.Collections.Generic;
using System.Data;

namespace ParquetFileViewer.Helpers
{
    public static class ExtensionMethods
    {
        private const string DefaultDateTimeFormat = "g";
        private const string DateOnlyDateTimeFormat = "d";
        private const string ISO8601DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";
        private const string ISO8601DateOnlyFormat = "yyyy-MM-dd";
        private const string ISO8601Alt1DateTimeFormat = "yyyy-MM-dd HH:mm:ss.fff";
        private const string ISO8601Alt2DateTimeFormat = "yyyy-MM-dd HH:mm:ss";

        /// <summary>
        /// Returns a list of all column names within a given datatable
        /// </summary>
        /// <param name="datatable">The datatable to retrieve the column names from</param>
        /// <returns></returns>
        public static IList<string> GetColumnNames(this DataTable datatable)
        {
            List<string> columns = new List<string>(datatable.Columns.Count);
            foreach (DataColumn column in datatable.Columns)
            {
                columns.Add(column.ColumnName);
            }
            return columns;
        }

        /// <summary>
        /// Gets the corresponding date format string for the provided <paramref name="dateFormat"/>
        /// </summary>
        /// <param name="dateFormat">Date format to get formatting string for</param>
        /// <returns>A formatting string such as: YYYY-MM-dd that is passible to DateTime.ToString()</returns>
        public static string GetDateFormat(this DateFormat dateFormat) => dateFormat switch
        {
            DateFormat.ISO8601 => ISO8601DateTimeFormat,
            DateFormat.ISO8601_DateOnly => ISO8601DateOnlyFormat,
            DateFormat.Default_DateOnly => DateOnlyDateTimeFormat,
            DateFormat.ISO8601_Alt1 => ISO8601Alt1DateTimeFormat,
            DateFormat.ISO8601_Alt2 => ISO8601Alt2DateTimeFormat,
            DateFormat.Default => DefaultDateTimeFormat,
            _ => null
        };

        /// <summary>
        /// Returns true if the date format does not contain a time component
        /// </summary>
        /// <param name="dateFormat">Date format to check</param>
        /// <returns>True if the date format has time information</returns>
        public static bool IsDateOnlyFormat(this DateFormat dateFormat) => dateFormat switch
        {
            DateFormat.ISO8601_DateOnly => true,
            DateFormat.Default_DateOnly => true,
            _ => false
        };

        public static string GetExtension(this FileType fileType) => fileType switch
        {
            FileType.CSV => ".csv",
            FileType.XLS => ".xls",
            _ => throw new ArgumentOutOfRangeException(nameof(fileType))
        };
    }
}
