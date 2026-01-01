using ParquetViewer.Engine.Exceptions;
using System.Data;
using static ParquetViewer.Engine.DataTableLite;

namespace ParquetViewer.Engine
{
    public class DataTableLite
    {
        public record ColumnLite(string Name, Type Type, IParquetSchemaElement ParentSchema, int Ordinal);

        private int _ordinal = 0;
        private readonly Dictionary<string, ColumnLite> _columns = new();
        private readonly List<object[]> _rows;

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
        public IReadOnlyList<object[]> Rows => _rows;

        public DataTableLite(int expectedRowCount = 1000)
        {
            this._rows = new(expectedRowCount);
        }

        public ColumnLite AddColumn(string name, Type type, IParquetSchemaElement parent)
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
                    var columnPath = columnLite.ParentSchema + "/" + columnLite.Name;
                    throw new NotSupportedException($"Duplicate column '{columnPath}' detected. Column names are case insensitive and must be unique.");
                }

                var columnType = columnLite.Type;
                dataTable.Columns.Add(new DataColumn(columnName: columnLite.Name, dataType: columnType));
            }

            dataTable.BeginLoadData();
            for (var i = 0; i < _rows.Count; i++)
            {
                token.ThrowIfCancellationRequested();
                LoadRow(dataTable, _rows[i]);
                progress?.Report(_columns.Count);
            }
            dataTable.EndLoadData();

            return dataTable;

            void LoadRow(DataTable dataTable, object[] values)
            {
                try
                {
                    //supposedly this is the fastest way to load data into a datatable https://stackoverflow.com/a/17123914/1458738
                    dataTable.LoadDataRow(values, false);
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("Type of value has a mismatch with column type"))
                    {
                        //Try figure out where the mismatch is
                        var columnIndex = 0;
                        foreach (var column in this._columns.Values)
                        {
                            if (values[columnIndex] != DBNull.Value && column.Type != values[columnIndex].GetType())
                            {
                                throw new TypeMismatchException($"Value type '{values[columnIndex]?.GetType()}' doesn't match column type {column.Type} for field `{column.Name}`");
                            }
                            columnIndex++;
                        }

                        throw new TypeMismatchException(null, ex);
                    }

                    throw;
                }
            }
        }

        //Gets a reference to the row data at the specified index
        public DataRowLite GetRowAt(int index)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(index, 0, nameof(index));
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, _rows.Count, nameof(index));

            return new DataRowLite(_rows[index], _columns.Values, this);
        }

        public DataTableLite Clone()
        {
            var clone = new DataTableLite();
            foreach (var column in this.Columns.Values)
            {
                clone.AddColumn(column.Name, column.Type, column.ParentSchema);
            }
            return clone;
        }
    }

    public class DataRowLite
    {
        public Dictionary<string, ColumnLite> Columns { get; }
        public object[] Row { get; }
        public DataTableLite Table { get; }

        public DataRowLite(object[] data, IEnumerable<ColumnLite> columns, DataTableLite table)
        {
            ArgumentNullException.ThrowIfNull(data);
            ArgumentNullException.ThrowIfNull(columns);
            ArgumentNullException.ThrowIfNull(table);

            Table = table;
            Row = data;
            Columns = columns.ToDictionary(c => c.Name);
            if (Row.Length != Columns.Count)
            {
                throw new ArgumentException($"Data length {data.Length} doesn't match number of columns {columns.Count()}", nameof(data));
            }
        }

        public DataTable ToDataTable()
        {
            var dt = new DataTable();
            foreach (var column in this.Columns)
            {
                dt.Columns.Add(new DataColumn(column.Key, column.Value.Type));
            }
            var row = dt.NewRow();
            row.ItemArray = this.Row;
            dt.Rows.Add(row);
            return dt;
        }

        public object GetValue(string columnName)
        {
            if (!this.Columns.ContainsKey(columnName))
            {
                throw new IndexOutOfRangeException($"Column `{columnName}` not found");
            }

            var index = 0;
            foreach (var column in this.Columns.Keys)
            {
                if (column.Equals(columnName))
                {
                    return this.Row[index];
                }
                index++;
            }
            throw new IndexOutOfRangeException($"Could not get value for column `{columnName}`");
        }
    }
}
