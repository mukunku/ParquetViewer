using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParquetViewer.Engine.Types
{
    public interface IListValue : IComparable<IListValue>, IComparable, IEnumerable<object>
    {
        public IList Data { get; }
        public Type Type { get; }
    }
}