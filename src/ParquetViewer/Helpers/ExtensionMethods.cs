using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace ParquetViewer.Helpers
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

        /// <summary>
        /// Provides very fast and basic column sizing for large data sets.
        /// </summary>
        public static void FastAutoSizeColumns(this DataGridView targetGrid)
        {
            // Cast out a DataTable from the target grid datasource.
            // We need to iterate through all the data in the grid and a DataTable supports enumeration.
            var gridTable = targetGrid.DataSource as DataTable;

            if (gridTable is null)
                return;

            // Create a graphics object from the target grid. Used for measuring text size.
            using (var gfx = targetGrid.CreateGraphics())
            {
                // Iterate through the columns.
                for (int i = 0; i < gridTable.Columns.Count; i++)
                {
                    // Leverage Linq enumerator to rapidly collect all the rows into a string array, making sure to exclude null values.
                    IEnumerable<string> colStringCollection = gridTable.AsEnumerable()
                        .Where(r => r.Field<object>(i) != null)
                        .Select(r => r.Field<object>(i).ToString());

                    // Sort the string array by string lengths.
                    colStringCollection = colStringCollection.OrderBy((x) => x.Length);

                    // Get the last and longest string in the array.
                    string longestColString = colStringCollection.Last();

                    if (gridTable.Columns[i].ColumnName.Length > longestColString.Length)
                        longestColString = gridTable.Columns[i].ColumnName;

                    // Use the graphics object to measure the string size. (With some added buffer)
                    var colWidth = gfx.MeasureString(longestColString + "#", targetGrid.Font);

                    // If the calculated width is larger than the column header width, set the new column width.
                    if (colWidth.Width > targetGrid.Columns[i].HeaderCell.Size.Width)
                    {
                        targetGrid.Columns[i].Width = (int)colWidth.Width;
                    }
                    else // Otherwise, set the column width to the header width.
                    {
                        targetGrid.Columns[i].Width = targetGrid.Columns[i].HeaderCell.Size.Width;
                    }
                }
            }
        }
    }
}
