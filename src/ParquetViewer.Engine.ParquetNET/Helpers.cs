namespace ParquetViewer.Engine.ParquetNET;

internal static class Helpers
{
    //This logic isn't perfect. It blends https://www.aloneguid.uk/posts/2023/04/parquet-empty-vs-null
    //with some of my understanding of how the dremel algorithm works. No way will it work for all cases.

    public static bool IsNull(this ParquetColumnData dataColumn, int index, ParquetSchemaElement field)
        => dataColumn.DefinitionLevels?.Length > index && dataColumn.DefinitionLevels[index] <= field.CurrentDefinitionLevel - 1;

    public static bool IsEmpty(this ParquetColumnData dataColumn, int index, ParquetSchemaElement field)
        => dataColumn.DefinitionLevels?.Length > index && dataColumn.DefinitionLevels[index] == field.CurrentDefinitionLevel
                && field.DataField?.MaxDefinitionLevel != dataColumn.DefinitionLevels[index] /*Fixes STRUCT_TYPE_TEST*/;
}