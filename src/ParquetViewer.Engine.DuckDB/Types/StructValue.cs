using ParquetViewer.Engine.Types;
using System.Data;

namespace ParquetViewer.Engine.DuckDB.Types
{
    public class StructValue : StructValueBase
    {
        public StructValue(string name, DataRowLite data)
            : base(name, data)
        {

        }
    }
}