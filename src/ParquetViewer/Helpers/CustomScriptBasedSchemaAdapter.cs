using ParquetViewer.Engine.Types;
using System;
using System.Collections;
using System.Data;
using System.Text;

namespace ParquetViewer.Helpers
{
    public class CustomScriptBasedSchemaAdapter
    {
        internal readonly static Hashtable TypeMap = new()
            {
                { typeof(ulong), "BIGINT {1}NULL" },
                { typeof(long), "BIGINT {1}NULL" },
                { typeof(bool), "BIT {1}NULL" },
                { typeof(char), "CHAR {1}NULL" },
                { typeof(DateTime), "DATETIME {1}NULL" },
                { typeof(double), "FLOAT {1}NULL" },
                { typeof(uint), "INT {1}NULL" },
                { typeof(int), "INT {1}NULL" },
                { typeof(Guid), "UNIQUEIDENTIFIER {1}NULL" },
                { typeof(ushort), "SMALLINT {1}NULL" },
                { typeof(short), "SMALLINT {1}NULL" },
                { typeof(decimal), "DECIMAL {1}NULL" },
                { typeof(float), "FLOAT {1}NULL" },
                { typeof(byte), "TINYINT {1}NULL" },
                { typeof(sbyte), "TINYINT {1}NULL" },
                { typeof(string), "NVARCHAR({0}) {1}NULL" },
                { typeof(TimeSpan), "INT {1}NULL" },
                { typeof(byte[]), "VARBINARY {1}NULL" },
                { typeof(ListValue), "sql_variant {1}NULL /*LIST*/" },
                { typeof(MapValue), "sql_variant {1}NULL /*MAP*/" },
                { typeof(StructValue), "sql_variant {1}NULL /*STRUCT*/" },
                { typeof(ByteArrayValue), "VARBINARY({0}) {1}NULL" },
            };

        public string? TablePrefix { get; set; }
        public bool CascadeDeletes { get; set; }

        public string GetCreateScript(string databaseName)
        {
            if (databaseName == null || databaseName.Trim().Length == 0)
            {
                throw new ArgumentException(string.Format("The database name passed is {0}", databaseName == null ? "null" : "empty"), "databaseName");
            }

            return string.Format("IF NOT EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = N'{0}') BEGIN CREATE DATABASE {1};\n END\n", databaseName, MakeSafe(databaseName));
        }

        public string GetSchemaScript(DataSet dataSet, bool markTablesAsLocalTemp)
        {
            if (dataSet == null)
            {
                throw new ArgumentException("null is not a valid parameter value", "dataSet");
            }
            StringBuilder stringBuilder = new StringBuilder();
            foreach (DataTable table in dataSet.Tables)
            {
                try
                {
                    stringBuilder.Append(MakeTable(table, markTablesAsLocalTemp));
                }
                catch (ArgumentException argumentException)
                {
                    throw new ArgumentException("Table does not contain any columns", table.TableName, argumentException);
                }
            }
            foreach (DataTable dataTable in dataSet.Tables)
            {
                if (dataTable.PrimaryKey.Length <= 0)
                {
                    continue;
                }
                string str = MakeSafe(string.Concat(TablePrefix, dataTable.TableName));
                string str1 = MakeSafe(string.Concat("PK_", TablePrefix, dataTable.TableName));
                string str2 = MakeList(dataTable.PrimaryKey);
                stringBuilder.AppendFormat("IF OBJECT_ID('{1}', 'PK') IS NULL BEGIN ALTER TABLE {0} WITH NOCHECK ADD CONSTRAINT {1} PRIMARY KEY CLUSTERED ({2}); END\n", str, str1, str2);
            }
            foreach (DataRelation relation in dataSet.Relations)
            {
                try
                {
                    stringBuilder.Append(MakeRelation(relation));
                }
                catch (ArgumentException argumentException1)
                {
                    throw new ArgumentException("Relationship has an empty column list", relation.RelationName, argumentException1);
                }
            }
            return stringBuilder.ToString();
        }

        protected string GetTypeFor(DataColumn column)
        {
            var item = TypeMap[column.DataType] as string;
            if (item == null)
            {
                throw new NotSupportedException(string.Format("No type mapping is provided for {0}", column.DataType.Name));
            }
            bool useMaxKeyword = column.DataType == typeof(string) || column.DataType == typeof(ByteArrayValue);
            return string.Format(item, useMaxKeyword ? "MAX" : column.MaxLength.ToString(), column.AllowDBNull ? string.Empty : "NOT ");
        }

        private string MakeList(DataColumn[] columns)
        {
            if (columns == null || columns.Length < 1)
            {
                throw new ArgumentException("Invalid column list!", "columns");
            }
            StringBuilder stringBuilder = new StringBuilder();
            bool flag = true;
            DataColumn[] dataColumnArray = columns;
            for (int i = 0; i < dataColumnArray.Length; i++)
            {
                DataColumn dataColumn = dataColumnArray[i];
                if (!flag)
                {
                    stringBuilder.Append(", ");
                }
                stringBuilder.Append(MakeSafe(dataColumn.ColumnName));
                flag = false;
            }
            return stringBuilder.ToString();
        }

        private string MakeList(DataColumnCollection columns)
        {
            if (columns == null || columns.Count < 1)
            {
                throw new ArgumentException("Invalid column list!", "columns");
            }
            StringBuilder stringBuilder = new StringBuilder();
            bool flag = true;
            foreach (DataColumn column in columns)
            {
                if (!flag)
                {
                    stringBuilder.Append(", ");
                }
                string str = MakeSafe(column.ColumnName);
                string typeFor = GetTypeFor(column);
                stringBuilder.Append($"{Environment.NewLine} {str} {typeFor}");
                flag = false;
            }
            return stringBuilder.ToString();
        }

        private string MakeRelation(DataRelation relation)
        {
            if (relation == null)
            {
                throw new ArgumentException("Invalid argument value (null)", "relation");
            }

            string childTable = MakeSafe(string.Concat(TablePrefix, relation.ChildTable.TableName));
            string parentTable = MakeSafe(string.Concat(TablePrefix, relation.ParentTable.TableName));
            string fkRelationName = MakeSafe(string.Concat(TablePrefix, relation.RelationName)); //Add prefix so same tables can be created using different prefixes. Otherwise collisions occur
            string childTableFKColumns = MakeList(relation.ChildColumns);
            string parentTableFKColumns = MakeList(relation.ParentColumns);

            return $"IF OBJECT_ID('{fkRelationName}', 'F') IS NULL BEGIN ALTER TABLE {childTable} " +
                $"ADD CONSTRAINT {fkRelationName} FOREIGN KEY ({childTableFKColumns}) REFERENCES {parentTable} ({parentTableFKColumns})" +
                $"{(CascadeDeletes ? " ON DELETE CASCADE" : string.Empty)}; END\n";
        }

        protected string MakeSafe(string inputValue)
        {
            string str = inputValue.Trim();
            string str1 = string.Format("[{0}]", str[..Math.Min(128, str.Length)]);
            return str1;
        }

        private string MakeTable(DataTable table, bool markTablesAsLocalTemp)
        {
            StringBuilder stringBuilder = new StringBuilder();
            string str = MakeSafe(string.Concat(markTablesAsLocalTemp ? "#" : string.Empty, TablePrefix, table.TableName));
            string str1 = MakeList(table.Columns);
            stringBuilder.AppendFormat("CREATE TABLE {0} ({1}\n);", str, str1);
            return stringBuilder.ToString();
        }
    }
}
