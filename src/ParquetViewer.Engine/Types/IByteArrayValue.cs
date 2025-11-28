using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ParquetViewer.Engine.Types
{
    public interface IByteArrayValue : IComparable<IByteArrayValue>, IComparable
    {
        string Name { get; }
        byte[] Data { get; }
        DisplayFormat[] PossibleDisplayFormats { get; }

        public enum DisplayFormat
        {
            Hex = 0,    //Default hexadecimal format
            IPv6,       // 16 bytes
            IPv4,       // 4 bytes
            Guid,       // 16 bytes
            Short,      // 2 bytes
            Integer,    // 4 bytes
            Long,       // 8 bytes
            Float,      // 4 bytes
            Double,     // 8 bytes
            ASCII,      // ASCII text if printable (any size)
            Base64,     // Base64 encoded string (any size)
            Size        // Size information (any size)
        }

        string ToStringTruncated(int desiredLength);

        bool ToIPv6([NotNullWhen(true)] out IPAddress? ipAddress);
        bool ToIPv4([NotNullWhen(true)] out IPAddress? ipAddress);
        bool ToGuid([NotNullWhen(true)] out Guid? guid);
        bool ToASCII([NotNullWhen(true)] out string? ascii);
        bool ToShort([NotNullWhen(true)] out short? @short);
        bool ToInteger([NotNullWhen(true)] out int? @int);
        bool ToLong([NotNullWhen(true)] out long? @long);
        bool ToFloat([NotNullWhen(true)] out float? @float);
        bool ToDouble([NotNullWhen(true)] out double? @double);
        void ToBase64(out string base64);
    }
}
