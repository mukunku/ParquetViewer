namespace ParquetViewer.Engine.Types
{
    public class ByteArrayValue : IComparable<ByteArrayValue>, IComparable
    {
        public string Name { get; }
        public byte[] Data { get; }

        public ByteArrayValue(string name, byte[] data)
        {
            this.Name = name;
            this.Data = data;
        }

        public override string ToString() => BitConverter.ToString(this.Data);

        public int CompareTo(ByteArrayValue? other)
        {
            if (other?.Data is null)
                return 1;
            else if (this.Data is null)
                return -1;
            else
                return ByteArraysEqual(this.Data, other.Data);
        }

        private static int ByteArraysEqual(ReadOnlySpan<byte> a1, ReadOnlySpan<byte> a2) => a1.SequenceCompareTo(a2);

        public int CompareTo(object? obj)
        {
            if (obj is ByteArrayValue byteArray)
                return CompareTo(byteArray);
            else
                return 1;
        }
    }
}
