using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text;
using static ParquetViewer.Engine.Types.IByteArrayValue;

namespace ParquetViewer.Engine.Types
{
    public class ByteArrayValueBase : IByteArrayValue
    {
        public byte[] Data { get; }


        private DisplayFormat[]? _possibleDisplayFormats;
        public DisplayFormat[] PossibleDisplayFormats =>
            _possibleDisplayFormats ??= CalculatePossibleDisplayFormats();

        public ByteArrayValueBase(byte[] data)
        {
            Data = data;
        }

        public override string ToString() => BitConverter.ToString(Data);

        public int CompareTo(IByteArrayValue? other)
        {
            if (other?.Data is null)
                return 1;
            else if (Data is null)
                return -1;
            else
                return Helpers.ByteArraysEqual(Data, other.Data);
        }

        public int CompareTo(object? obj)
        {
            if (obj is IByteArrayValue byteArray)
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

            var printableCount = Data.Sum(@byte =>
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
            => stringLength != int.MaxValue
            ? (stringLength + 1) / 3 //One byte = 3 chars. E.g. AA- (-1 for the last byte which won't have a dash)
            : int.MaxValue;

        /// <summary>
        /// Returns the binary data in hex with an ellipsis in the middle if required to reach <paramref name="desiredLength"/> 
        /// </summary>
        /// <param name="desiredLength">Positive integer length to target (NOT GUARANTEED)</param>
        /// <returns>Binary data in hexadecimal representation</returns>
        public string ToStringTruncated(int desiredLength)
        {
            var maxBytesToRender = Math.Max(2, HexStringLengthToByteCount(desiredLength));
            if (Data.Length <= maxBytesToRender)
                return BitConverter.ToString(Data);

            return BitConverter.ToString(Data, 0, maxBytesToRender / 2) + "[...]"
            + BitConverter.ToString(Data, Data.Length - maxBytesToRender / 2);
        }
    }
}