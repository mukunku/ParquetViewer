using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;

namespace ParquetFileViewer.Helpers
{
    public static class ExtensionMethods
    {
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
    }
}
