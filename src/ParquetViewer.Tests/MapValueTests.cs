using ParquetViewer.Engine.Types;
using System.Collections;

namespace ParquetViewer.Tests;

[TestClass]
public class MapValueTests
{
    private static MapValue MapOf(int count)
    {
        var entries = Enumerable.Range(0, count).ToArray();
        return new MapValue(new ArrayList(entries), typeof(int), new ArrayList(entries), typeof(int));
    }

    [TestMethod]
    public void TO_STRING_WRITES_THE_WHOLE_MAP()
    {
        Assert.AreEqual("[(0,0),(1,1),(2,2)]", MapOf(3).ToString());
    }

    [TestMethod]
    public void SHORT_MAPS_ARE_NOT_TRUNCATED()
    {
        Assert.AreEqual("[(0,0),(1,1),(2,2)]", MapOf(3).ToStringTruncated(100));
    }

    [TestMethod]
    public void LONG_MAPS_STOP_BETWEEN_ENTRIES()
    {
        //Stops before the next entry once the text passes 10 chars
        Assert.AreEqual("[(0,0),(1,1)[...]", MapOf(100).ToStringTruncated(10));
    }

    [TestMethod]
    public void MAPS_THAT_END_PAST_THE_DESIRED_LENGTH_ARE_STILL_COMPLETE()
    {
        //The last entry pushes the length past 10, but there's nothing left to cut so it closes normally
        Assert.AreEqual("[(0,0),(1,1)]", MapOf(2).ToStringTruncated(10));
    }

    [TestMethod]
    public void NESTED_VALUES_SHARE_THE_REMAINING_LENGTH()
    {
        //A map of embedding-like lists: the first value gets what's left of the 20 chars after "[(a," and stops early
        var list = new ListValue(new ArrayList(Enumerable.Range(1000, 100).ToArray()), typeof(int));
        var map = new MapValue(["a", "b"], typeof(string), [list, list], typeof(ListValue));

        Assert.AreEqual("[(a,[1000,1001,1002,1003[...])[...]", map.ToStringTruncated(20));
    }

    [TestMethod]
    public void NESTED_VALUES_ALWAYS_SHOW_THEIR_FIRST_ELEMENT()
    {
        //The first entry uses up the whole budget, but the second value still shows its first element
        var list = new ListValue(new ArrayList(Enumerable.Range(1000, 100).ToArray()), typeof(int));
        var map = new MapValue(["a"], typeof(string), [list], typeof(ListValue));

        Assert.AreEqual("[(a,[1000[...])]", map.ToStringTruncated(1));
    }

    [TestMethod]
    public void UNTRUNCATED_NESTED_VALUES_ARE_WRITTEN_IN_FULL()
    {
        var list = new ListValue(new ArrayList(Enumerable.Range(0, 50).ToArray()), typeof(int));
        var map = new MapValue(["a"], typeof(string), [list], typeof(ListValue));

        Assert.AreEqual($"[(a,[{string.Join(",", Enumerable.Range(0, 50))}])]", map.ToString());
    }

    [TestMethod]
    public void LONG_STRING_VALUES_ARE_CUT_TO_THE_REMAINING_LENGTH()
    {
        //After "[(" and the key "a" there are 18 chars left for the value
        var map = new MapValue(["a"], typeof(string), [new string('b', 5000)], typeof(string));
        Assert.AreEqual($"[(a,{new string('b', 18)}[...])]", map.ToStringTruncated(20));
    }

    [TestMethod]
    public void LONG_STRING_VALUES_ARE_NOT_CUT_WITHOUT_A_DESIRED_LENGTH()
    {
        var longString = new string('b', 5000);
        var map = new MapValue(["a"], typeof(string), [longString], typeof(string));
        Assert.AreEqual($"[(a,{longString})]", map.ToString());
    }

    [TestMethod]
    [DataRow(typeof(IListValue))]
    [DataRow(typeof(IMapValue))]
    [DataRow(typeof(IStructValue))]
    [DataRow(typeof(IByteArrayValue))]
    public void COMPLEX_VALUES_ARE_TRUNCATABLE(Type valueType)
    {
        Assert.IsTrue(typeof(ITruncatable).IsAssignableFrom(valueType));
    }
}