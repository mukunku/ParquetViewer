using ParquetViewer.Engine.Types;
using System.Collections;
using System.Text;

namespace ParquetViewer.Engine.DuckDB.Types
{
    internal class ListValue : ListValueBase
    {
        public ListValue(ArrayList data, Type itemType) : base(data, itemType)
        {

        }
    }
}