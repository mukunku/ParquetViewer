namespace ParquetViewer.Engine.Exceptions
{
    public class DecimalOverflowException : Exception
    {
        //C# (and Parquet.NET) max supported decimal size is: DECIMAL(29,28)
        public const int MAX_DECIMAL_PRECISION = 29;
        public const int MAX_DECIMAL_SCALE = 28;

        public bool HasDetailedInfo => FieldName is not null || Precision is not null || Scale is not null;
        public string? FieldName { get; }
        public int? Precision { get; }
        public int? Scale { get; }

        public DecimalOverflowException(string fieldName, int precision, int scale, OverflowException overflowEx) : base(overflowEx.Message, overflowEx)
        {
            this.FieldName = fieldName;
            this.Precision = precision;
            this.Scale = scale;
        }

        public DecimalOverflowException(OverflowException overflowEx) : base(overflowEx.Message, overflowEx)
        {

        }
    }
}