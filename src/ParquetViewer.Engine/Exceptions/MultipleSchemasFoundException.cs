namespace ParquetViewer.Engine.Exceptions
{
    public class MultipleSchemasFoundException : Exception
    {
        public List<List<string>> Schemas;

        public MultipleSchemasFoundException(List<List<string>> parquetSchemas) : base("Multiple schemas found in directory.")
        {
            Schemas = parquetSchemas ?? new List<List<string>>();
        }
    }
}