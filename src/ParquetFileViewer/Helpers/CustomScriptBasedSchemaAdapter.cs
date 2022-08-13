using System;
using System.Collections;
using System.Data;
using System.Text;

namespace ParquetFileViewer.Helpers
{
    public class CustomScriptBasedSchemaAdapter
    {
        internal readonly static Hashtable TypeMap;

        public string TablePrefix { get; set; }
        public bool CascadeDeletes { get; set; }

        static CustomScriptBasedSchemaAdapter()
        {
            Hashtable hashtable = new Hashtable();
            hashtable.Add(typeof(ulong), "bigint {1}NULL");
            hashtable.Add(typeof(long), "bigint {1}NULL");
            hashtable.Add(typeof(bool), "bit {1}NULL");
            hashtable.Add(typeof(char), "char {1}NULL");
            hashtable.Add(typeof(DateTime), "datetime {1}NULL");
            hashtable.Add(typeof(double), "float {1}NULL");
            hashtable.Add(typeof(uint), "int {1}NULL");
            hashtable.Add(typeof(int), "int {1}NULL");
            hashtable.Add(typeof(Guid), "uniqueidentifier {1}NULL");
            hashtable.Add(typeof(ushort), "smallint {1}NULL");
            hashtable.Add(typeof(short), "smallint {1}NULL");
            hashtable.Add(typeof(decimal), "real {1}NULL");
            hashtable.Add(typeof(byte), "tinyint {1}NULL");
            hashtable.Add(typeof(sbyte), "tinyint {1}NULL");
            hashtable.Add(typeof(string), "nvarchar({0}) {1}NULL");
            hashtable.Add(typeof(TimeSpan), "int {1}NULL");
            hashtable.Add(typeof(byte[]), "varbinary {1}NULL");
            TypeMap = hashtable;
        }

        public string GetCreateScript(string databaseName)
        {
            if (databaseName == null || databaseName.Trim().Length == 0)
            {
                throw new ArgumentException(string.Format("The database name passed is {0}", (databaseName == null ? "null" : "empty")), "databaseName");
            }

            return string.Format("IF NOT EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = N'{0}') BEGIN CREATE DATABASE {1};\n END\n", databaseName, this.MakeSafe(databaseName));
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
                    stringBuilder.Append(this.MakeTable(table, markTablesAsLocalTemp));
                }
                catch (ArgumentException argumentException)
                {
                    throw new ArgumentException("Table does not contain any columns", table.TableName, argumentException);
                }
            }
            foreach (DataTable dataTable in dataSet.Tables)
            {
                if ((int)dataTable.PrimaryKey.Length <= 0)
                {
                    continue;
                }
                string str = this.MakeSafe(string.Concat(this.TablePrefix, dataTable.TableName));
                string str1 = this.MakeSafe(string.Concat("PK_", this.TablePrefix, dataTable.TableName));
                string str2 = this.MakeList(dataTable.PrimaryKey);
                stringBuilder.AppendFormat("IF OBJECT_ID('{1}', 'PK') IS NULL BEGIN ALTER TABLE {0} WITH NOCHECK ADD CONSTRAINT {1} PRIMARY KEY CLUSTERED ({2}); END\n", str, str1, str2);
            }
            foreach (DataRelation relation in dataSet.Relations)
            {
                try
                {
                    stringBuilder.Append(this.MakeRelation(relation));
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
            string item = (string)TypeMap[column.DataType];
            if (item == null)
            {
                throw new NotSupportedException(string.Format("No type mapping is provided for {0}", column.DataType.Name));
            }
            return string.Format(item, column.DataType == typeof(string) ? "MAX" : column.MaxLength.ToString(), (column.AllowDBNull ? string.Empty : "NOT "));
        }

        private string MakeList(DataColumn[] columns)
        {
            if (columns == null || (int)columns.Length < 1)
            {
                throw new ArgumentException("Invalid column list!", "columns");
            }
            StringBuilder stringBuilder = new StringBuilder();
            bool flag = true;
            DataColumn[] dataColumnArray = columns;
            for (int i = 0; i < (int)dataColumnArray.Length; i++)
            {
                DataColumn dataColumn = dataColumnArray[i];
                if (!flag)
                {
                    stringBuilder.Append(", ");
                }
                stringBuilder.Append(this.MakeSafe(dataColumn.ColumnName));
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
                    stringBuilder.Append(" , ");
                }
                string str = this.MakeSafe(column.ColumnName);
                string typeFor = this.GetTypeFor(column);
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

            string childTable = this.MakeSafe(string.Concat(this.TablePrefix, relation.ChildTable.TableName));
            string parentTable = this.MakeSafe(string.Concat(this.TablePrefix, relation.ParentTable.TableName));
            string fkRelationName = this.MakeSafe(string.Concat(this.TablePrefix, relation.RelationName)); //Add prefix so same tables can be created using different prefixes. Otherwise collisions occur
            string childTableFKColumns = this.MakeList(relation.ChildColumns);
            string parentTableFKColumns = this.MakeList(relation.ParentColumns);

            return $"IF OBJECT_ID('{fkRelationName}', 'F') IS NULL BEGIN ALTER TABLE {childTable} " +
                $"ADD CONSTRAINT {fkRelationName} FOREIGN KEY ({childTableFKColumns}) REFERENCES {parentTable} ({parentTableFKColumns})" +
                $"{(this.CascadeDeletes ? " ON DELETE CASCADE" : string.Empty)}; END\n";
        }

        protected string MakeSafe(string inputValue)
        {
            string str = inputValue.Trim();
            string str1 = string.Format("[{0}]", str.Substring(0, Math.Min(128, str.Length)));
            return str1;
        }

        private string MakeTable(DataTable table, bool markTablesAsLocalTemp)
        {
            StringBuilder stringBuilder = new StringBuilder();
            string str = this.MakeSafe(string.Concat(markTablesAsLocalTemp ? "#" : string.Empty, this.TablePrefix, table.TableName));
            string str1 = this.MakeList(table.Columns);
            stringBuilder.AppendFormat("CREATE TABLE {0} ({1});\n", str, str1);
            return stringBuilder.ToString();
        }
    }
}
