using ParquetViewer.Engine.Types;
using System.Collections;
using System.Numerics;

namespace ParquetViewer.Tests;

/// <summary>
/// Covers how values are written as JSON by <see cref="Helpers.WriteValue"/> and
/// <see cref="Utf8JsonWriterWithRunningLength"/>, which list, map and struct cells and JSON exports all go through.
/// </summary>
[TestClass]
public class JsonWriterTests
{
    private static string ListOf(object value) => new ListValue(new ArrayList { value }, value.GetType()).ToString();

    public static IEnumerable<object[]> NumbersAndTheirJson =>
    [
        [(sbyte)-128, "-128"],
        [(byte)255, "255"],
        [(short)-32768, "-32768"],
        [(ushort)65535, "65535"],
        [int.MinValue, "-2147483648"],
        [uint.MaxValue, "4294967295"],
        [long.MinValue, "-9223372036854775808"],
        [ulong.MaxValue, "18446744073709551615"], //Doesn't fit in a long
        [0.1f, "0.1"], //Floats are written in their shortest form, not widened to double (0.10000000149011612)
        [1.5d, "1.5"],
        [0.046799998730421066d, "0.046799998730421066"], //Doubles keep full precision
        [1e300d, "1E+300"], //Too large for a decimal
        [-0.0d, "-0"],
        [12.5m, "12.5"],
        [decimal.MaxValue, "79228162514264337593543950335"],
    ];

    [TestMethod]
    [DynamicData(nameof(NumbersAndTheirJson))]
    public void NUMBERS_ARE_WRITTEN_AS_JSON_NUMBERS(object number, string expectedJson)
    {
        Assert.AreEqual($"[{expectedJson}]", ListOf(number));
    }

    public static IEnumerable<object[]> NonFiniteNumbersAndTheirJson =>
    [
        [double.NaN, "\"NaN\""],
        [double.PositiveInfinity, "\"Infinity\""],
        [double.NegativeInfinity, "\"-Infinity\""],
        [float.NaN, "\"NaN\""],
        [float.PositiveInfinity, "\"Infinity\""],
        [float.NegativeInfinity, "\"-Infinity\""],
    ];

    [TestMethod]
    [DynamicData(nameof(NonFiniteNumbersAndTheirJson))]
    public void NON_FINITE_NUMBERS_ARE_WRITTEN_AS_STRINGS(object number, string expectedJson)
    {
        //JSON has no NaN or Infinity, and these used to throw an OverflowException
        Assert.AreEqual($"[{expectedJson}]", ListOf(number));
    }

    [TestMethod]
    public void LISTS_MIX_NUMBERS_AND_NON_FINITE_VALUES()
    {
        var list = new ListValue(new ArrayList { 1.5d, double.NaN, 1e300d, DBNull.Value }, typeof(double));
        Assert.AreEqual("[1.5,\"NaN\",1E+300,null]", list.ToString());
    }

    [TestMethod]
    public void STRUCT_FIELDS_WITH_NON_FINITE_NUMBERS_SERIALIZE()
    {
        //This used to render as "Error while serializing Struct field..."
        var structValue = new StructValue(["a", "b", "c"], [double.NaN, 2.5f, (long)7]);
        Assert.AreEqual("{\"a\":\"NaN\",\"b\":2.5,\"c\":7}", structValue.ToString());
    }

    [TestMethod]
    [DataRow(typeof(sbyte))]
    [DataRow(typeof(byte))]
    [DataRow(typeof(short))]
    [DataRow(typeof(ushort))]
    [DataRow(typeof(int))]
    [DataRow(typeof(uint))]
    [DataRow(typeof(long))]
    [DataRow(typeof(ulong))]
    [DataRow(typeof(float))]
    [DataRow(typeof(double))]
    [DataRow(typeof(decimal))]
    public void BUILT_IN_NUMBER_TYPES_ARE_NUMBERS(Type type)
    {
        Assert.IsTrue(type.IsNumber());
    }

    [TestMethod]
    [DataRow(typeof(bool))]
    [DataRow(typeof(char))]
    [DataRow(typeof(string))]
    [DataRow(typeof(DateTime))]
    [DataRow(typeof(DBNull))]
    [DataRow(typeof(object))]
    [DataRow(typeof(Half))] //Not a built-in type code, so these are treated as non-numbers
    [DataRow(typeof(Int128))]
    [DataRow(typeof(BigInteger))]
    public void OTHER_TYPES_ARE_NOT_NUMBERS(Type type)
    {
        Assert.IsFalse(type.IsNumber());
    }

    [TestMethod]
    public void BYTE_ARRAYS_ARE_WRITTEN_IN_FULL_WITHOUT_A_MAX_LENGTH()
    {
        //E.g. for JSON exports, which used to lose everything but the first and last 4 bytes
        var bytes = Enumerable.Range(0, 20).Select(i => (byte)i).ToArray();
        var list = new ListValue(new ArrayList { new ByteArrayValue(bytes) }, typeof(ByteArrayValue));

        Assert.AreEqual($"[\"{BitConverter.ToString(bytes)}\"]", list.ToString());
    }

    [TestMethod]
    public void BYTE_ARRAYS_ARE_SHORTENED_FOR_DISPLAY()
    {
        var bytes = Enumerable.Range(0, 20).Select(i => (byte)i).ToArray();
        var list = new ListValue(new ArrayList { new ByteArrayValue(bytes) }, typeof(ByteArrayValue));

        Assert.AreEqual("[\"00-01-02-03[...]10-11-12-13\"]", list.ToStringTruncated(1000));
    }

    [TestMethod]
    public void WRITING_A_NON_NUMBER_AS_A_NUMBER_THROWS()
    {
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriterWithRunningLength(stream);
        Assert.ThrowsExactly<ArgumentException>(() => writer.WriteNumberValue((object)"not a number"));
    }

    [TestMethod]
    [DataRow(12345L, 5)]
    [DataRow(long.MinValue, 20)]
    [DataRow(double.MinValue, 24)]
    public void RUNNING_LENGTH_COUNTS_NUMBERS_AND_A_COMMA(object number, int formattedLength)
    {
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriterWithRunningLength(stream);
        writer.WriteStartArray();
        writer.WriteNumberValue(number);

        Assert.AreEqual(1 /*'['*/ + formattedLength + 1 /*','*/, writer.ApproximateStringLengthSoFar);
    }

    [TestMethod]
    public void RUNNING_LENGTH_FITS_THE_LONGEST_DECIMAL()
    {
        //The longest a number can format is a decimal at 31 chars, which must fit the 32 char measuring buffer
        const decimal longestDecimal = -7.9228162514264337593543950335m;
        Assert.AreEqual(31, longestDecimal.ToString(System.Globalization.CultureInfo.InvariantCulture).Length);

        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriterWithRunningLength(stream);
        writer.WriteStartArray();
        writer.WriteNumberValue(longestDecimal);

        Assert.AreEqual(1 /*'['*/ + 31 + 1 /*','*/, writer.ApproximateStringLengthSoFar);
    }
}
