namespace ParquetViewer.Engine
{
    public interface IParquetSchemaElement
    {
        string Path { get; }
        Type Type { get; }
        int? TypeLength { get; }
        string LogicalType { get; }
        RepetitionTypeId RepetitionType { get; }
        string ConvertedType { get; }
        ICollection<IParquetSchemaElement> Children { get; }

        public enum RepetitionTypeId
        {
            Required,
            Optional,
            Repeated
        }
    }
}
