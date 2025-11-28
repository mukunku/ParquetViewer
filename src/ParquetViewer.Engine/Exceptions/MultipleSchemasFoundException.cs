namespace ParquetViewer.Engine.Exceptions
{
    public class MultipleSchemasFoundException : Exception
    {
        public List<IParquetSchema> Schemas;

        public MultipleSchemasFoundException(List<IParquetSchema> parquetSchemas) : base("Multiple schemas found in directory.")
        {
            Schemas = parquetSchemas ?? new List<IParquetSchema>();
        }
    }
}
