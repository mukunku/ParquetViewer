using System.Text.Encodings.Web;
using System.Text.Json;

namespace ParquetViewer.Engine.ParquetNET
{
    public class Utf8JsonWriterWithRunningLength : IDisposable
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
            ApproximateStringLengthSoFar += name.Length + 1; // +1 for the colon ':'
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
            ApproximateStringLengthSoFar += value.Length;
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

        public void WriteNumberValue(decimal value)
        {
            _writer.WriteNumberValue(value);
            ApproximateStringLengthSoFar += value.ToString().Length;
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

        public void Dispose()
        {
            _writer.Dispose();
        }
    }
}
