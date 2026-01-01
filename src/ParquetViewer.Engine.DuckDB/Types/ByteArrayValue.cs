using ParquetViewer.Engine.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ParquetViewer.Engine.DuckDB.Types
{
    internal class ByteArrayValue : IByteArrayValue
    {
        public string Name => throw new NotImplementedException();

        public byte[] Data => throw new NotImplementedException();

        public IByteArrayValue.DisplayFormat[] PossibleDisplayFormats => throw new NotImplementedException();

        public int CompareTo(IByteArrayValue? other)
        {
            throw new NotImplementedException();
        }

        public int CompareTo(object? obj)
        {
            throw new NotImplementedException();
        }

        public bool ToASCII([NotNullWhen(true)] out string? ascii)
        {
            throw new NotImplementedException();
        }

        public void ToBase64(out string base64)
        {
            throw new NotImplementedException();
        }

        public bool ToDouble([NotNullWhen(true)] out double? @double)
        {
            throw new NotImplementedException();
        }

        public bool ToFloat([NotNullWhen(true)] out float? @float)
        {
            throw new NotImplementedException();
        }

        public bool ToGuid([NotNullWhen(true)] out Guid? guid)
        {
            throw new NotImplementedException();
        }

        public bool ToInteger([NotNullWhen(true)] out int? @int)
        {
            throw new NotImplementedException();
        }

        public bool ToIPv4([NotNullWhen(true)] out IPAddress? ipAddress)
        {
            throw new NotImplementedException();
        }

        public bool ToIPv6([NotNullWhen(true)] out IPAddress? ipAddress)
        {
            throw new NotImplementedException();
        }

        public bool ToLong([NotNullWhen(true)] out long? @long)
        {
            throw new NotImplementedException();
        }

        public bool ToShort([NotNullWhen(true)] out short? @short)
        {
            throw new NotImplementedException();
        }

        public string ToStringTruncated(int desiredLength)
        {
            throw new NotImplementedException();
        }
    }
}
