using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParquetViewer.Engine.Types
{
    public interface IStructValue : IComparable<IStructValue>, IComparable
    {
        public string Name { get; }

        public DataRow Data { get; }

        string ToStringTruncated(int desiredLength);
    }
}
