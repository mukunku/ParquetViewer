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
