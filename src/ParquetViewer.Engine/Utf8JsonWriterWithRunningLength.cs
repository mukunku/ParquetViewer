using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace ParquetViewer.Engine;

public sealed class Utf8JsonWriterWithRunningLength : IDisposable
{
    public int ApproximateStringLengthSoFar { get; private set; }

    private readonly Utf8JsonWriter _writer;

    public Utf8JsonWriterWithRunningLength(Stream stream)
    {
        _writer = new Utf8JsonWriter(stream, new JsonWriterOptions()
        {
            //Without 'UnsafeRelaxedJsonEscaping' JSON reserved chars get escaped using unicode instead of backslash (E.g. \u0022 instead of \")
            //https://stackoverflow.com/questions/70849792/system-text-json-utf8jsonwriter-how-to-prevent-breaking-unicode-characters-int
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });
        ApproximateStringLengthSoFar = 0;
    }

    public void WriteStartObject()
    {
        _writer.WriteStartObject();
        ApproximateStringLengthSoFar++; // '{'
    }

    public void WriteEndObject()
    {
        _writer.WriteEndObject();
        ApproximateStringLengthSoFar++; // '}'
    }

    public void WritePropertyName(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        _writer.WritePropertyName(name);
        ApproximateStringLengthSoFar += name.Length + 3; // +2 for the quotes, +1 for the colon ':'
    }

    public void WriteNullValue()
    {
        _writer.WriteNullValue();
        ApproximateStringLengthSoFar += 4; // 'null'
        IncrementForComma();
    }

    public void WriteStringValue(string value)
    {
        _writer.WriteStringValue(value);
        ApproximateStringLengthSoFar += value.Length + 2; // +2 for the quotes
        IncrementForComma();
    }

    public void WriteBooleanValue(bool value)
    {
        _writer.WriteBooleanValue(value);
        if (value)
            ApproximateStringLengthSoFar += 4; // 'true'
        else
            ApproximateStringLengthSoFar += 5; // 'false'

        IncrementForComma();
    }

    public void WriteNumberValue(long value)
    {
        _writer.WriteNumberValue(value);
        IncrementForNumber(value);
    }

    /// <remarks>Throws for <see cref="float.NaN"/> and infinities, which JSON can't represent as numbers</remarks>
    public void WriteNumberValue(float value)
    {
        _writer.WriteNumberValue(value);
        IncrementForNumber(value);
    }

    /// <remarks>Throws for <see cref="double.NaN"/> and infinities, which JSON can't represent as numbers</remarks>
    public void WriteNumberValue(double value)
    {
        _writer.WriteNumberValue(value);
        IncrementForNumber(value);
    }

    public void WriteNumberValue(decimal value)
    {
        _writer.WriteNumberValue(value);
        IncrementForNumber(value);
    }

    public void WriteNumberValue(ulong value)
    {
        _writer.WriteNumberValue(value);
        IncrementForNumber(value);
    }

    /// <summary>
    /// Writes any value whose type passes <see cref="Helpers.IsNumber(Type)"/>.
    /// </summary>
    public void WriteNumberValue(object value)
    {
        switch (value)
        {
            case double @double:
                if (double.IsFinite(@double))
                    WriteNumberValue(@double);
                else
                    WriteStringValue(@double.ToString(CultureInfo.InvariantCulture)); //JSON has no NaN or Infinity
                break;
            case float @float:
                if (float.IsFinite(@float))
                    WriteNumberValue(@float);
                else
                    WriteStringValue(@float.ToString(CultureInfo.InvariantCulture)); //JSON has no NaN or Infinity
                break;
            case decimal @decimal:
                WriteNumberValue(@decimal);
                break;
            case ulong @ulong:
                WriteNumberValue(@ulong);
                break;
            case sbyte or byte or short or ushort or int or uint or long:
                WriteNumberValue(Convert.ToInt64(value));
                break;
            default:
                throw new ArgumentException($"'{value?.GetType()}' is not a number type", nameof(value));
        }
    }

    public void WriteRawValue(string json)
    {
        _writer.WriteRawValue(json);
        ApproximateStringLengthSoFar += json.Length;
    }

    public void WriteStartArray()
    {
        _writer.WriteStartArray();
        ApproximateStringLengthSoFar += 1; // '['
    }

    public void WriteEndArray()
    {
        _writer.WriteEndArray();
        ApproximateStringLengthSoFar += 1; // ']'
    }

    //We don't keep track of element count in each nested structure, so we can't tell
    //if a comma would be added to the json element or not. So we just always assume there is one :shrug:
    //E.g. { elem1: value, elem2: value } vs { elem1: value }
    private void IncrementForComma() => ApproximateStringLengthSoFar++;

    //Formats into a stack buffer just to measure the length, so writing numbers doesn't allocate a string each time
    private void IncrementForNumber<T>(T value) where T : ISpanFormattable
    {
        //No number should typically format longer than 32 chars (the longest is a decimal at 31)
        Span<char> buffer = stackalloc char[32];
        ApproximateStringLengthSoFar += value.TryFormat(buffer, out var charsWritten, default, CultureInfo.InvariantCulture)
            ? charsWritten
            : buffer.Length; //If formatting failed, assume the worst case
        IncrementForComma();
    }

    public void Dispose()
    {
        _writer.Dispose();
    }
}