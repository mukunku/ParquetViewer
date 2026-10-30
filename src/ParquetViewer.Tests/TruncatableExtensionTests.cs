using ParquetViewer.Engine.Types;
using ParquetViewer.Helpers;
using System.Collections;

namespace ParquetViewer.Tests;

[TestClass]
public class TruncatableExtensionTests
{
    [TestMethod]
    public void RETURNS_THE_TRUNCATED_STRING()
    {
        var list = new ListValue(new ArrayList { 1, 2, 3 }, typeof(int));
        Assert.AreEqual("[1,2,3]", list.ToStringTruncatedOrError(100, "#ERR"));
    }

    [TestMethod]
    public void RETURNS_THE_ERROR_TEXT_INSTEAD_OF_THROWING()
    {
        Assert.AreEqual("#ERR", new ThrowingValue().ToStringTruncatedOrError(100, "#ERR"));
    }

    private sealed class ThrowingValue : ITruncatable
    {
        public string ToStringTruncated(int desiredLength) => throw new InvalidOperationException("Can't format this");
    }
}
