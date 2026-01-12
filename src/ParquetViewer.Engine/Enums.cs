namespace ParquetViewer.Engine
{
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