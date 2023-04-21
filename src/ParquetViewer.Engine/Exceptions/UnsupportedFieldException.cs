namespace ParquetViewer.Engine.Exceptions
{
    public class UnsupportedFieldException : Exception
    {
        public UnsupportedFieldException(string fieldName) : base(fieldName)
        {

        }
    }
}
