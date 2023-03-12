using System;
using System.Collections.Generic;

namespace ParquetViewer.Engine.Exceptions
{
    public class MultipleSchemasFoundException : Exception
    {
        public List<Parquet.Schema.ParquetSchema> Schemas;

        internal MultipleSchemasFoundException(List<Parquet.Schema.ParquetSchema> parquetSchemas) : base("Multiple schemas found in directory.")
        {
            Schemas = parquetSchemas ?? new List<Parquet.Schema.ParquetSchema>();
        }
    }
}
