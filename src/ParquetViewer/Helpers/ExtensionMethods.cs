using ParquetViewer.Common;
using ParquetViewer.Engine;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace ParquetViewer.Helpers
{
    public static class ExtensionMethods
    {
        private const string WHITESPACE_BUFFER = "#";

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
                    var newColumnSize = MeasureStringWidth(gridTable.Columns[i].ColumnName + WHITESPACE_BUFFER); //Fit header by default
                    bool isComplexType = typeof(IComplexValue).IsAssignableFrom(gridTable.Columns[i].DataType);
                    if (!isComplexType)
                    {
                        // Leverage Linq enumerator to rapidly collect all the rows into a string array, making sure to exclude null values.
                        IEnumerable<string> colStringCollection = gridTable.AsEnumerable()
                            .Select(r => r.Field<object>(i)?.ToString())
                            .Where(v => v is not null);

                        // Sort the string array by string lengths.
                        colStringCollection = colStringCollection.OrderBy((x) => x.Length);

                        // Get the last and longest string in the array.
                        string longestColString = colStringCollection.Last();

                        if (gridTable.Columns[i].ColumnName.Length > longestColString.Length)
                            longestColString = gridTable.Columns[i].ColumnName + WHITESPACE_BUFFER;

                        var maxColWidth = MeasureStringWidth(longestColString + WHITESPACE_BUFFER);

                        // If the calculated width is larger than the column header width, use that instead
                        if (maxColWidth > newColumnSize)
                            newColumnSize = maxColWidth;
                    }

                    targetGrid.Columns[i].Width = newColumnSize;
                }

                int MeasureStringWidth(string input) => (int)gfx.MeasureString(input, targetGrid.Font).Width;
            }
        }

        public static DataGridViewAutoSizeColumnsMode ToDGVMode(this AutoSizeColumnsMode mode) => mode switch
        {
            AutoSizeColumnsMode.ColumnHeader => DataGridViewAutoSizeColumnsMode.ColumnHeader,
            AutoSizeColumnsMode.AllCells => DataGridViewAutoSizeColumnsMode.AllCells,
            AutoSizeColumnsMode.None => DataGridViewAutoSizeColumnsMode.None,
            _ => DataGridViewAutoSizeColumnsMode.None
        };
    }
}
