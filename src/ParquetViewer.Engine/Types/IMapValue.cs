using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParquetViewer.Engine.Types
{
    public interface IMapValue : IComparable<IMapValue>, IComparable, IEnumerable<(object Key, object Value)>
    {
        public ArrayList Keys { get; }
        public Type KeyType { get; }
        public ArrayList Values { get; }
        public Type ValueType { get; }
    }
}
