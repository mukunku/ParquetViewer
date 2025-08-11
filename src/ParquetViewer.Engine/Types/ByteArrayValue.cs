﻿using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text;

namespace ParquetViewer.Engine.Types
{
    public class ByteArrayValue : IComparable<ByteArrayValue>, IComparable
    {
        public string Name { get; }
        public byte[] Data { get; }

        private DisplayFormat[]? _possibleDisplayFormats;
        public DisplayFormat[] PossibleDisplayFormats => 
            _possibleDisplayFormats ??= this.CalculatePossibleDisplayFormats();

        public ByteArrayValue(string name, byte[] data)
        {
            this.Name = name;
            this.Data = data;
        }

        public override string ToString() => BitConverter.ToString(this.Data);

        public int CompareTo(ByteArrayValue? other)
        {
            if (other?.Data is null)
                return 1;
            else if (this.Data is null)
                return -1;
            else
                return ByteArraysEqual(this.Data, other.Data);
        }

        private static int ByteArraysEqual(ReadOnlySpan<byte> a1, ReadOnlySpan<byte> a2) => a1.SequenceCompareTo(a2);

        public int CompareTo(object? obj)
        {
            if (obj is ByteArrayValue byteArray)
                return CompareTo(byteArray);
            else
                return 1;
        }

        /// <summary>
        /// Returns possible display formats for the binary data in <see cref="Data"/>
        /// </summary>
        /// <returns></returns>
        private DisplayFormat[] CalculatePossibleDisplayFormats()
        {
            var possibleDisplayFormats = new List<DisplayFormat>();

            if (ToIPv4(out var _))
                possibleDisplayFormats.Add(DisplayFormat.IPv4);

            if (ToIPv6(out var _))
                possibleDisplayFormats.Add(DisplayFormat.IPv6);

            if (ToGuid(out var _))
                possibleDisplayFormats.Add(DisplayFormat.Guid);

            if (ToShort(out var _))
                possibleDisplayFormats.Add(DisplayFormat.Short);

            if (ToInteger(out var _))
                possibleDisplayFormats.Add(DisplayFormat.Integer);

            if (ToLong(out var _))
                possibleDisplayFormats.Add(DisplayFormat.Long);

            if (ToFloat(out var _))
                possibleDisplayFormats.Add(DisplayFormat.Float);

            if (ToDouble(out var _))
                possibleDisplayFormats.Add(DisplayFormat.Double);

            if (ToASCII(out var _))
                possibleDisplayFormats.Add(DisplayFormat.ASCII);

            //Always supported formats
            possibleDisplayFormats.Add(DisplayFormat.Hex);
            possibleDisplayFormats.Add(DisplayFormat.Base64);
            possibleDisplayFormats.Add(DisplayFormat.Size);

            return possibleDisplayFormats.ToArray();
        }

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

        #region Type Conversions
        public bool ToIPv6([NotNullWhen(true)] out IPAddress? ipAddress)
        {
            ipAddress = null;

            if (Data.Length != 16)
                return false;

            try
            {
                // Try to create an IPAddress - if it succeeds, it's at least valid binary for IPv6
                ipAddress = new IPAddress(Data);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool ToIPv4([NotNullWhen(true)] out IPAddress? ipAddress)
        {
            ipAddress = null;

            if (Data.Length != 4)
                return false;

            try
            {
                // Try to create an IPAddress - if it succeeds, it's at least valid binary for IPv4
                ipAddress = new IPAddress(Data);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool ToGuid([NotNullWhen(true)] out Guid? guid)
        {
            guid = null;

            if (Data.Length != 16)
                return false;

            try
            {
                // Try to create a Guid - if it succeeds, it's at least valid binary for a GUID
                guid = new Guid(Data);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool ToASCII([NotNullWhen(true)] out string? ascii)
        {
            ascii = null;

            if (Data.Length == 0)
                return false;

            var printableCount = this.Data.Sum(@byte =>
                @byte >= ' ' /*32*/ && @byte <= '~' /*126*/ //Printable ASCII range
                ? 1 : 0);

            //If the length is longer than 8 make sure at least 75% of bytes are printable as ASCII
            if (Data.Length < 8 || (double)printableCount / Data.Length >= 0.75)
            {
                try
                {
                    ascii = Encoding.ASCII.GetString(Data);
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            return false;
        }

        public bool ToShort([NotNullWhen(true)] out short? @short)
        {
            @short = null;

            if (Data.Length != 2)
                return false;

            try
            {
                @short = BitConverter.ToInt16(Data);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool ToInteger([NotNullWhen(true)] out int? @int)
        {
            @int = null;

            if (Data.Length != 4)
                return false;

            try
            {
                @int = BitConverter.ToInt32(Data);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool ToLong([NotNullWhen(true)] out long? @long)
        {
            @long = null;

            if (Data.Length != 8)
                return false;

            try
            {
                @long = BitConverter.ToInt64(Data);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool ToFloat([NotNullWhen(true)] out float? @float)
        {
            @float = null;

            if (Data.Length != 4)
                return false;

            try
            {
                @float = BitConverter.ToSingle(Data);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool ToDouble([NotNullWhen(true)] out double? @double)
        {
            @double = null;

            if (Data.Length != 8)
                return false;

            try
            {
                @double = BitConverter.ToDouble(Data);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void ToBase64(out string base64)
        {
            base64 = Convert.ToBase64String(Data);
        }
        #endregion

        /// <summary>
        /// Calculates how many bytes are needed to generate a string of the given length in hexadecimal format.
        /// </summary>
        private static int HexStringLengthToByteCount(int stringLength)
            => (stringLength + 1) / 3; //One byte = 3 chars. E.g. AA- (-1 for the last byte which won't have a dash)

        /// <summary>
        /// Returns the binary data in hex with an ellipsis in the middle if required to reach <paramref name="desiredLength"/> 
        /// </summary>
        /// <param name="desiredLength">Positive integer length to target (NOT GUARANTEED)</param>
        /// <returns>Binary data in hexadecimal representation</returns>
        public string ToStringTruncated(int desiredLength)
        {
            if (Data.Length < HexStringLengthToByteCount(desiredLength))
                return BitConverter.ToString(Data);

            return BitConverter.ToString(Data, 0, desiredLength / 2) + "[...]"
            + BitConverter.ToString(Data, Data.Length - (desiredLength / 2));
        }
    }
}
