using ParquetViewer.Engine.Types;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParquetViewer.Engine.DuckDB.Types
{
    internal class StructValue : IStructValue
    {
        public string Name => throw new NotImplementedException();

        public DataRowLite Data => throw new NotImplementedException();

        public int CompareTo(IStructValue? other)
        {
            throw new NotImplementedException();
        }

        public int CompareTo(object? obj)
        {
            throw new NotImplementedException();
        }

        public DataTable ToDataTable()
        {
            throw new NotImplementedException();
        }

        public string ToStringTruncated(int desiredLength)
        {
            throw new NotImplementedException();
        }
    }
}
