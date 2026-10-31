using ParquetViewer.Engine.Types;
using System.Collections;

namespace ParquetViewer.Tests;

[TestClass]
public class ListValueTests
{
    private static ListValue ListOf<T>(IEnumerable<T> values) where T : notnull
        => new(new ArrayList(values.ToArray()), typeof(T));

    [TestMethod]
    public void TO_STRING_WRITES_THE_WHOLE_LIST()
    {
        var list = ListOf(Enumerable.Range(0, 100));
        Assert.AreEqual($"[{string.Join(",", Enumerable.Range(0, 100))}]", list.ToString());
    }

    [TestMethod]
    public void SHORT_LISTS_ARE_NOT_TRUNCATED()
    {
        var list = ListOf([1, 2, 3]);
        Assert.AreEqual("[1,2,3]", list.ToStringTruncated(100));
    }

    [TestMethod]
    public void LONG_LISTS_STOP_ONCE_PAST_THE_DESIRED_LENGTH()
    {
        //E.g. an embedding vector
        const int elementCount = 1536;
        const int desiredLength = 100;
        var list = ListOf(Enumerable.Repeat(0.123f, elementCount));

        var truncated = list.ToStringTruncated(desiredLength);

        Assert.StartsWith("[0.123,0.123,", truncated);
        Assert.EndsWith("[...]", truncated);

        //It stops after the element that passes the desired length, so it can only go over by about one element
        const int oneElement = 6; //"0.123,"
        Assert.IsLessThanOrEqualTo(desiredLength + oneElement + "[...]".Length, truncated.Length);
        Assert.IsLessThan(list.ToString().Length, truncated.Length);
    }

    [TestMethod]
    public void TRUNCATION_STOPS_BETWEEN_ELEMENTS()
    {
        var list = ListOf(Enumerable.Range(1000, 50));

        var truncated = list.ToStringTruncated(20);

        //Never cuts an element in half
        Assert.AreEqual("[1000,1001,1002,1003[...]", truncated);
    }

    [TestMethod]
    public void LISTS_THAT_END_PAST_THE_DESIRED_LENGTH_ARE_STILL_COMPLETE()
    {
        //The last element pushes the length past 20, but there's nothing left to cut so it closes normally
        var list = ListOf([1000, 1001, 1002, 1003]);
        Assert.AreEqual("[1000,1001,1002,1003]", list.ToStringTruncated(20));
    }

    [TestMethod]
    public void NESTED_LISTS_STOP_PART_WAY_THROUGH()
    {
        var inner = ListOf(Enumerable.Range(1000, 100));
        var outer = new ListValue([inner, inner], typeof(ListValue));

        Assert.AreEqual("[[1000,1001,1002,1003[...]", outer.ToStringTruncated(20));
    }

    [TestMethod]
    public void LISTS_NESTED_IN_STRUCTS_STOP_PART_WAY_THROUGH()
    {
        var structValue = new StructValue(["a", "b"], [ListOf(Enumerable.Range(1000, 100)), 1]);

        Assert.AreEqual("{\"a\":[1000,1001,1002[...]", structValue.ToStringTruncated(20));
    }

    [TestMethod]
    public void MAPS_NESTED_IN_LISTS_STOP_PART_WAY_THROUGH()
    {
        var keys = new ArrayList(Enumerable.Range(0, 100).ToArray());
        var map = new MapValue(keys, typeof(int), new ArrayList(Enumerable.Range(0, 100).ToArray()), typeof(int));
        var list = new ListValue([map], typeof(MapValue));

        var truncated = list.ToStringTruncated(30);

        Assert.StartsWith("[[{\"key\":0,\"value\":0},", truncated);
        Assert.EndsWith("}[...]", truncated); //Stops between entries
        Assert.IsLessThan(100, truncated.Length);
    }

    [TestMethod]
    public void UNTRUNCATED_NESTED_VALUES_ARE_WRITTEN_IN_FULL()
    {
        var inner = ListOf(Enumerable.Range(0, 100));
        var outer = new ListValue([inner], typeof(ListValue));

        Assert.AreEqual($"[[{string.Join(",", Enumerable.Range(0, 100))}]]", outer.ToString());
    }

    [TestMethod]
    public void LONG_STRINGS_ARE_CUT_TO_THE_REMAINING_LENGTH()
    {
        //Only 19 of the 20 chars are left after the '['
        var list = ListOf([new string('a', 5000), "b"]);
        Assert.AreEqual($"[\"{new string('a', 19)}\"[...]", list.ToStringTruncated(20));
    }

    [TestMethod]
    public void LONG_STRINGS_IN_STRUCTS_ARE_CUT_TO_THE_REMAINING_LENGTH()
    {
        //'{' + "\"name\":" uses 8 of the 20 chars
        var structValue = new StructValue(["name", "other"], [new string('a', 5000), 1]);
        Assert.AreEqual($"{{\"name\":\"{new string('a', 12)}\"[...]", structValue.ToStringTruncated(20));
    }

    [TestMethod]
    public void LONG_STRINGS_ARE_NOT_CUT_WITHOUT_A_DESIRED_LENGTH()
    {
        var longString = new string('a', 5000);
        Assert.AreEqual($"[\"{longString}\"]", ListOf([longString]).ToString());
    }

    [TestMethod]
    public void CUTTING_A_STRING_NEVER_SPLITS_A_SURROGATE_PAIR()
    {
        var emojis = string.Concat(Enumerable.Repeat("😀", 1000)); //Each emoji is 2 chars

        //Cutting at 3 chars would leave half an emoji, which the JSON writer would throw on, so it finishes the pair instead
        Assert.AreEqual("😀😀", Engine.Helpers.TruncateString(emojis, 3));
        Assert.AreEqual("😀", Engine.Helpers.TruncateString(emojis, 1));
        Assert.AreEqual("😀😀", Engine.Helpers.TruncateString(emojis, 4)); //Already on a pair boundary

        var truncated = ListOf([emojis]).ToStringTruncated(4); //Must not throw
        Assert.EndsWith("[...]", truncated);
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(-5)]
    public void TRUNCATING_TO_A_NON_POSITIVE_LENGTH_THROWS(int length)
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => Engine.Helpers.TruncateString("abc", length));
    }

    [TestMethod]
    public void TRUNCATED_LISTS_STILL_WRITE_NON_FINITE_NUMBERS()
    {
        var list = new ListValue([double.NaN, 1.5d, double.PositiveInfinity], typeof(double));
        Assert.AreEqual("[\"NaN\",1.5,\"Infinity\"]", list.ToStringTruncated(100));
    }
}