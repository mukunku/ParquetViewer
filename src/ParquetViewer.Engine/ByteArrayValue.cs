using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParquetViewer.Engine
{
    public class ByteArrayValue
    {
        public string Name { get; }
        public byte[] Data { get; }

        public ByteArrayValue(string name, byte[] data)
        {
            this.Name = name;
            this.Data = data;
        }

        public override string ToString() => BitConverter.ToString(this.Data);
    }
}
