using DuckDB.NET.Data;
using DuckDB.NET.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ParquetViewer.Engine.DuckDB
{
    public class ParquetMetadata : IParquetMetadata
    {
        public int ParquetVersion { get; }

        public int RowGroupCount { get; }

        public int RowCount { get; }

        public string CreatedBy { get; }

        public ICollection<IRowGroupMetadata> RowGroups { get; }

        public IParquetSchemaElement SchemaTree { get; }

        private ParquetMetadata()
        {
            SchemaTree = null; //TODO
        }

        public static async Task<ParquetMetadata> FromDuckDBAsync(DuckDBConnection db, string parquetFilePath)
        {
            return new ParquetMetadata();
        }
    }

    public class RowGroupMetadata : IRowGroupMetadata
    {
        public int Ordinal { get; }
        public int RowCount { get; }
        public ICollection<ISortingColumnMetadata>? SortingColumns { get; }
        public long FileOffset { get; }
        public long TotalByteSize { get; }
        public long TotalCompressedSize { get; }
        public RowGroupMetadata()
        {
        }
    }

    public class ParquetSchemaElement : IParquetSchemaElement
    {
        public string Path { get; }

        public Type Type { get; }

        public int? TypeLength { get; }

        public string LogicalType { get; }

        public IParquetSchemaElement.RepetitionTypeId RepetitionType { get; }

        public string ConvertedType { get; }

        public ICollection<IParquetSchemaElement> Children { get; }

        public ParquetSchemaElement()
        {
            this.Children = new List<IParquetSchemaElement>();
        }
    }
}
