namespace ParquetViewer.Engine
{
    public interface IParquetSchemaElement
    {
        string Path { get; }
        ICollection<IParquetSchemaElement> Children { get; }

        bool IsPrimitive { get; }

        FieldTypeId FieldType { get; }

        IParquetSchemaElement GetListField();
        IParquetSchemaElement GetListItemField();
        IParquetSchemaElement GetSingleOrByName(string name);
        IParquetSchemaElement GetMapKeyValueField();
        IParquetSchemaElement GetMapKeyField();
        IParquetSchemaElement GetMapValueField();

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