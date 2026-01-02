using ParquetViewer.Engine.ParquetNET;
using System.Numerics;

namespace ParquetViewer.Engine.ParquetNET
{
    internal static class Helpers
    {
        #region Dubious Functions
        //This logic is a cluster f... right now. It blends https://www.aloneguid.uk/posts/2023/04/parquet-empty-vs-null
        //with some of my understanding of how the dremel algorithm works. No way will it work for all cases.

        public static bool IsNull(this Parquet.Data.DataColumn dataColumn, int index, ParquetSchemaElement field)
            => dataColumn.DefinitionLevels?.Length > index && dataColumn.DefinitionLevels[index] <= field.CurrentDefinitionLevel - 1;

        public static bool IsEmpty(this Parquet.Data.DataColumn dataColumn, int index, ParquetSchemaElement field)
            => dataColumn.DefinitionLevels?.Length > index && dataColumn.DefinitionLevels[index] == field.CurrentDefinitionLevel
                    && field.DataField?.MaxDefinitionLevel != dataColumn.DefinitionLevels[index] /*Fixes STRUCT_TYPE_TEST*/;
        #endregion

        /// <summary>
        /// Some parquet writers don't write null entries into the data array for empty and null lists.
        /// This throws off our logic so lets find all empty/null lists and add a null entry into 
        /// the data array to align it with the repetition/definition levels.
        /// </summary>
        /// <param name="dataColumn">The parquet data column</param>
        public static IEnumerable<object> GetDataWithPaddedNulls(this Parquet.Data.DataColumn dataColumn, ParquetSchemaElement field)
        {
            var dataEnumerable = dataColumn.Data.Cast<object?>().Select(d => d ?? DBNull.Value);
            
            int levelCount = dataColumn.DefinitionLevels?.Length ?? 0;
            if (levelCount > dataColumn.Data.Length)
            {
                dataEnumerable = GetDataWithPaddedNulls();

                IEnumerable<object> GetDataWithPaddedNulls()
                {
                    var index = -1;
                    foreach (var data in dataColumn.Data)
                    {
                        index++;

                        while (dataColumn.IsEmpty(index, field) || dataColumn.IsNull(index, field))
                        {
                            yield return DBNull.Value;
                            index++;
                        }

                        yield return data ?? DBNull.Value;
                    }

                    //Need to handle case where last N rows are null/empty
                    while (levelCount > index + 1)
                    {
                        yield return DBNull.Value;
                        index++;
                    }
                }
            }

            return dataEnumerable;
        }
    }
}
