namespace ParquetViewer.Engine
{
    public interface IParquetSchemaElement
    {
        string Path { get; }
        ICollection<IParquetSchemaElement> Children { get; }

        public bool IsPrimitive { get; }

        public enum RepetitionTypeId
        {
            Required,
            Optional,
            Repeated
        }

        public enum FieldTypeId
        {
            Primitive,
            List,
            Struct,
            Map
        }
    }
}