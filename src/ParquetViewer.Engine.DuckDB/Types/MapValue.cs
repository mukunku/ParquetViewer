using ParquetViewer.Engine.Types;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParquetViewer.Engine.DuckDB.Types
{
    internal class MapValue : IMapValue
    {
        public ArrayList Keys => throw new NotImplementedException();

        public Type KeyType => throw new NotImplementedException();

        public ArrayList Values => throw new NotImplementedException();

        public Type ValueType => throw new NotImplementedException();

        public int CompareTo(IMapValue? other)
        {
            throw new NotImplementedException();
        }

        public int CompareTo(object? obj)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<(object Key, object Value)> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
