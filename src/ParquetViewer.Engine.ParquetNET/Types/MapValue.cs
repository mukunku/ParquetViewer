using System.Collections;

namespace ParquetViewer.Engine.ParquetNET.Types
{
    public class MapValue : MapValueBase
    {
        public MapValue(ArrayList keys, Type keyType, ArrayList values, Type valueType)
            : base(keys, keyType, values, valueType)
        {

        }
    }
}