using ParquetViewer.Engine.ParquetNET;
using ParquetViewer.Engine.Types;

namespace ParquetViewer.Tests;

[TestClass]
public class ListValueBuilderTests
{
    //Levels follow the standard three-tier encoding: each list contributes two non-required
    //nodes (the optional LIST group and the repeated wrapper), and the _type is assumed required.
    //The values below are hand-built to exercise the null/empty classifier at each nesting depth.

    private static object Read(object[] data, int[] repetitionLevels, int[] definitionLevels, int[] listDefinitionLevels)
    {
        var builder = new ListValueBuilder(repetitionLevels, definitionLevels, data, typeof(int));
        var levels = new FieldLevels(listDefinitionLevels[^1] + 1, listDefinitionLevels);
        return builder.ReadRows(0, 1, levels, default).Single() as object ?? DBNull.Value;
    }

    [TestMethod]
    public void TwoTierEmptyList_RendersAsEmpty_NotNull()
    {
        var result = Read([DBNull.Value], [0], [0], listDefinitionLevels: [0]);

        Assert.IsInstanceOfType<ListValue>(result);
        Assert.AreEqual("[]", result.ToString());
    }

    [TestMethod]
    public void TwoTierNullList_RendersAsNull()
    {
        //When the repeated field's (optional) parent is absent the definition level is below the
        //list level, so the list is null rather than empty.
        var result = Read([DBNull.Value], [0], [0], listDefinitionLevels: [1]);

        Assert.AreEqual(DBNull.Value, result);
    }

    [TestMethod]
    public void TwoTierListWithValues_RendersValues()
    {
        var result = Read([10, 20], [0, 1], [1, 1], listDefinitionLevels: [0]);

        Assert.IsInstanceOfType<ListValue>(result);
        Assert.AreEqual("[10,20]", result.ToString());
    }

    [TestMethod]
    public void ListOfList_NullInnerList_RendersAsNullElement()
    {
        //[null] => outer list present with a single null inner list (def stops at the outer wrapper)
        var result = Read([DBNull.Value], [0], [2], listDefinitionLevels: [1, 3]);

        Assert.IsInstanceOfType<ListValue>(result);
        Assert.AreEqual("[null]", result.ToString());
    }

    [TestMethod]
    public void TwoNestedLists_OuterEmpty_RendersEmpty()
    {
        var result = Read([DBNull.Value], [0], [1], listDefinitionLevels: [1, 3]);

        Assert.AreEqual("[]", result.ToString());
    }

    [TestMethod]
    public void TwoTierNestedListEmpty_RendersAsEmptyInner()
    {
        var result = Read([DBNull.Value], [0], [3], listDefinitionLevels: [1, 3]);

        Assert.AreEqual("[[]]", result.ToString());
    }

    [TestMethod]
    public void ThreeNestedLists_IntermediateEmpty_RendersEmptyAtCorrectDepth()
    {
        //[[]] => outer list has one element, which is an empty list (def stops at the middle list).
        //This is the case the old outermost/innermost shortcut misclassified as null.
        var result = Read([DBNull.Value], [0], [3], listDefinitionLevels: [1, 3, 5]);

        Assert.AreEqual("[[]]", result.ToString());
    }

    [TestMethod]
    public void ThreeNestedLists_InnermostEmpty_RendersEmptyAtInnermostDepth()
    {
        var result = Read([DBNull.Value], [0], [5], listDefinitionLevels: [1, 3, 5]);

        Assert.AreEqual("[[[]]]", result.ToString());
    }

    [TestMethod]
    public void ThreeNestedLists_IntermediateNull_RendersNullAtCorrectDepth()
    {
        var result = Read([DBNull.Value], [0], [2], listDefinitionLevels: [1, 3, 5]);

        Assert.AreEqual("[null]", result.ToString());
    }

    [TestMethod]
    public void ThreeNestedLists_InnermostNull_RendersNullAtInnermostDepth()
    {
        var result = Read([DBNull.Value], [0], [4], listDefinitionLevels: [1, 3, 5]);

        Assert.AreEqual("[[null]]", result.ToString());
    }

    [TestMethod]
    public void ThreeNestedLists_OuterNull_RendersNull()
    {
        var result = Read([DBNull.Value], [0], [0], listDefinitionLevels: [1, 3, 5]);

        Assert.AreEqual(DBNull.Value, result);
    }
}