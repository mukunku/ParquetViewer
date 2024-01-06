using System.Data;

namespace ParquetViewer.Engine
{
    internal class DataTableLite
    {
        internal record ColumnLite(string Name, Type Type, string? Parent, int Ordinal);

        private int _ordinal = 0;
        private readonly Dictionary<string, ColumnLite> _columns = new();
        private readonly List<object[]?> _rows;

        /// <summary>
        /// Total number of rows in the opened parquet file(s)
        /// irregardless of how many records are loaded.
        /// </summary>
        public long DataSetSize = 0;

        /// <summary>
        /// Columns in the dataset
        /// </summary>
        public IReadOnlyDictionary<string, ColumnLite> Columns => _columns;

        /// <summary>
        /// Rows of the dataset
        /// </summary>
        public IReadOnlyList<object[]?> Rows => _rows;

        public DataTableLite(int expectedRowCount = 1000)
        {
            this._rows = new(expectedRowCount);
        }

        public ColumnLite AddColumn(string name, Type type, string? parent = null)
        {
            if (_rows.Count > 0)
            {
                throw new InvalidOperationException("Can't add columns after creating rows");
            }

            var column = new ColumnLite(name, type, parent, _ordinal++);
            _columns.Add(name, column);
            return column;
        }

        public void NewRow()
        {
            var row = new object[Columns.Count];
            _rows.Add(row);
        }

        public ColumnLite GetColumn(string name)
        {
            if (_columns.TryGetValue(name, out var value))
            {
                return value;
            }
            throw new KeyNotFoundException($"{nameof(name)}: {name}");
        }

        public DataTable ToDataTable(CancellationToken token, IProgress<int>? progress = null)
        {
            var dataTable = new DataTable();

            foreach (var column in _columns)
            {
                token.ThrowIfCancellationRequested();

                var columnLite = column.Value;

                if (dataTable.Columns.Contains(columnLite.Name))
                {
                    //DataTable's don't support case sensitive field names unfortunately
                    var columnPath = (columnLite.Parent is not null ? columnLite.Parent + "/" : string.Empty) + columnLite.Name;
                    throw new NotSupportedException($"Duplicate column '{columnPath}' detected. Column names are case insensitive and must be unique.");
                }

                dataTable.Columns.Add(new DataColumn(columnLite.Name, columnLite.Type));
            }

            dataTable.BeginLoadData();
            for (var i = 0; i < _rows.Count; i++)
            {
                token.ThrowIfCancellationRequested();

                //supposedly this is the fastest way to load data into a datatable https://stackoverflow.com/a/17123914/1458738
                dataTable.LoadDataRow(_rows[i]!, false);

                progress?.Report(_columns.Count);
            }
            dataTable.EndLoadData();

            return dataTable;
        }
    }
}
