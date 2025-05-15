using System.Net;
using System.Text;

namespace ParquetViewer.Engine.Types
{
    public class ByteArrayValue : IComparable<ByteArrayValue>, IComparable
    {
        public string Name { get; }
        public byte[] Data { get; }
        public bool IsGenericFixed { get; }
        public int FixedLength { get; }

        // Enum to represent different display formats for binary data
        public enum DisplayFormat
        {
            Hex,        // Default hexadecimal format
            IPv6,       // IPv6 address format (16 bytes)
            IPv4,       // IPv4 address format (4 bytes)
            UUID,       // UUID/GUID format (16 bytes)
            Integer,    // Integer representation (2, 4, or 8 bytes)
            Float,      // Floating point representation (4 or 8 bytes)
            Base64,     // Base64 encoded string (any length)
            ASCII,      // ASCII text if printable (any length)
            Size,       // Size information (any length)
            Binary      // Raw binary representation (any length)
        }

        public ByteArrayValue(string name, byte[] data, bool isGenericFixed = false, int fixedLength = 0)
        {
            this.Name = name;
            this.Data = data;
            this.IsGenericFixed = isGenericFixed;
            this.FixedLength = fixedLength;
        }

        public override string ToString()
        {
            // Try to intelligently detect the most appropriate format based on data length and content
            try
            {
                if (IsGenericFixed)
                {
                    // Handle fixed-length binary data based on length
                    if (Data.Length == 16)
                    {
                        // 16-byte data: Try IPv6 or UUID
                        if (IsLikelyIPv6())
                            return ToIPv6String();
                        else if (IsLikelyUUID())
                            return ToUUIDString();
                    }
                    else if (Data.Length == 4)
                    {
                        // 4-byte data: Try IPv4
                        try
                        {
                            return new IPAddress(Data).ToString();
                        }
                        catch { /* Fall through to default */ }
                    }
                    else if (Data.Length <= 8)
                    {
                        // For small binary data, check if it's mostly ASCII printable
                        string asciiStr = ToASCIIString();
                        if (asciiStr != ToHexString()) // If ASCII conversion was successful
                            return asciiStr;
                    }
                }
                else
                {
                    // For variable-length binary data, check if it's mostly ASCII printable
                    string asciiStr = ToASCIIString();
                    if (asciiStr != ToHexString()) // If ASCII conversion was successful
                        return asciiStr;
                }
            }
            catch
            {
                // If any detection fails, fall back to hex
            }

            // Default to hex representation for all other cases
            return ToHexString();
        }

        /// <summary>
        /// Converts the binary data to a string using the specified format
        /// </summary>
        public string ToString(DisplayFormat format)
        {
            try
            {
                return format switch
                {
                    DisplayFormat.IPv6 => ToIPv6String(),
                    DisplayFormat.IPv4 => Data.Length == 4 ? new IPAddress(Data).ToString() : ToHexString(),
                    DisplayFormat.UUID => ToUUIDString(),
                    DisplayFormat.Base64 => ToBase64String(),
                    DisplayFormat.ASCII => ToASCIIString(),
                    DisplayFormat.Size => $"{Data.Length} bytes",
                    DisplayFormat.Integer => GetIntegerRepresentation(),
                    DisplayFormat.Float => GetFloatRepresentation(),
                    DisplayFormat.Binary => GetBinaryRepresentation(),
                    _ => ToHexString() // Default to hex
                };
            }
            catch
            {
                // If conversion fails, fall back to hex
                return ToHexString();
            }
        }

        /// <summary>
        /// Gets an integer representation of the binary data if possible
        /// </summary>
        private string GetIntegerRepresentation()
        {
            if (Data.Length == 2)
            {
                return BitConverter.ToInt16(Data, 0).ToString();
            }
            else if (Data.Length == 4)
            {
                return BitConverter.ToInt32(Data, 0).ToString();
            }
            else if (Data.Length == 8)
            {
                return BitConverter.ToInt64(Data, 0).ToString();
            }

            return ToHexString();
        }

        /// <summary>
        /// Gets a floating-point representation of the binary data if possible
        /// </summary>
        private string GetFloatRepresentation()
        {
            if (Data.Length == 4)
            {
                return BitConverter.ToSingle(Data, 0).ToString();
            }
            else if (Data.Length == 8)
            {
                return BitConverter.ToDouble(Data, 0).ToString();
            }

            return ToHexString();
        }

        /// <summary>
        /// Gets a binary (0s and 1s) representation of the data
        /// </summary>
        private string GetBinaryRepresentation()
        {
            var result = new StringBuilder();
            foreach (byte b in Data)
            {
                result.Append(Convert.ToString(b, 2).PadLeft(8, '0'));
                result.Append(' ');
            }
            return result.ToString().Trim();
        }

        /// <summary>
        /// Attempts to determine if the data is likely an IPv6 address
        /// </summary>
        public bool IsLikelyIPv6()
        {
            // This is a simple heuristic and may need refinement
            if (Data.Length != 16)
                return false;

            try
            {
                // Try to create an IPAddress - if it succeeds, it's at least valid binary for IPv6
                var _ = new IPAddress(Data);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Attempts to determine if the data is likely a UUID/GUID
        /// </summary>
        public bool IsLikelyUUID()
        {
            if (Data.Length != 16)
                return false;

            try
            {
                // Try to create a Guid - if it succeeds, it's at least valid binary for a GUID
                var _ = new Guid(Data);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Converts the 16-byte binary data to an IPv6 address string
        /// </summary>
        public string ToIPv6String()
        {
            if (Data.Length != 16)
                return ToHexString();

            try
            {
                return new IPAddress(Data).ToString();
            }
            catch
            {
                return ToHexString();
            }
        }

        /// <summary>
        /// Converts the 16-byte binary data to a UUID/GUID string
        /// </summary>
        public string ToUUIDString()
        {
            if (Data.Length != 16)
                return ToHexString();

            try
            {
                return new Guid(Data).ToString();
            }
            catch
            {
                return ToHexString();
            }
        }

        /// <summary>
        /// Converts the binary data to a Base64 encoded string
        /// </summary>
        public string ToBase64String()
        {
            return Convert.ToBase64String(Data);
        }

        /// <summary>
        /// Attempts to convert the binary data to an ASCII string if it contains printable characters
        /// </summary>
        public string ToASCIIString()
        {
            // Check if the data contains mostly printable ASCII characters
            if (Data.Length == 0)
                return string.Empty;

            int printableCount = 0;
            foreach (byte b in Data)
            {
                if (b >= 32 && b <= 126) // Printable ASCII range
                    printableCount++;
            }

            // If at least 75% of bytes are printable ASCII, return as ASCII string
            if ((double)printableCount / Data.Length >= 0.75)
            {
                return Encoding.ASCII.GetString(Data);
            }

            // Otherwise, fall back to hex representation
            return ToHexString();
        }

        /// <summary>
        /// Returns the binary data as a hexadecimal string regardless of type
        /// </summary>
        public string ToHexString() => BitConverter.ToString(this.Data);

        /// <summary>
        /// Gets all possible string representations of this binary data
        /// </summary>
        public Dictionary<DisplayFormat, string> GetAllRepresentations()
        {
            var result = new Dictionary<DisplayFormat, string>
            {
                { DisplayFormat.Hex, ToHexString() }
            };

            // Add specialized formats based on data length
            if (Data.Length == 16)
            {
                // 16-byte formats
                result.Add(DisplayFormat.IPv6, ToIPv6String());
                result.Add(DisplayFormat.UUID, ToUUIDString());
            }
            else if (Data.Length == 4)
            {
                // 4-byte formats (IPv4, Int32)
                try
                {
                    result.Add(DisplayFormat.IPv4, new IPAddress(Data).ToString());
                    if (BitConverter.IsLittleEndian)
                    {
                        var dataArray = Data.ToArray();
                        Array.Reverse(dataArray);
                        result.Add(DisplayFormat.Integer, BitConverter.ToInt32(dataArray, 0).ToString());
                    }
                    else
                    {
                        result.Add(DisplayFormat.Integer, BitConverter.ToInt32(Data, 0).ToString());
                    }
                }
                catch { /* Ignore conversion errors */ }
            }
            else if (Data.Length == 8)
            {
                // 8-byte formats (Long, Double)
                try
                {
                    if (BitConverter.IsLittleEndian)
                    {
                        var dataArray = Data.ToArray();
                        Array.Reverse(dataArray);
                        result.Add(DisplayFormat.Integer, BitConverter.ToInt64(dataArray, 0).ToString());
                        result.Add(DisplayFormat.Float, BitConverter.ToDouble(dataArray, 0).ToString());
                    }
                    else
                    {
                        result.Add(DisplayFormat.Integer, BitConverter.ToInt64(Data, 0).ToString());
                        result.Add(DisplayFormat.Float, BitConverter.ToDouble(Data, 0).ToString());
                    }
                }
                catch { /* Ignore conversion errors */ }
            }
            else if (Data.Length == 2)
            {
                // 2-byte formats (Short)
                try
                {
                    if (BitConverter.IsLittleEndian)
                    {
                        var dataArray = Data.ToArray();
                        Array.Reverse(dataArray);
                        result.Add(DisplayFormat.Integer, BitConverter.ToInt16(dataArray, 0).ToString());
                    }
                    else
                    {
                        result.Add(DisplayFormat.Integer, BitConverter.ToInt16(Data, 0).ToString());
                    }
                }
                catch { /* Ignore conversion errors */ }
            }

            // These formats work for any length
            result.Add(DisplayFormat.Base64, ToBase64String());
            result.Add(DisplayFormat.ASCII, ToASCIIString());

            // Add binary size information
            result.Add(DisplayFormat.Size, $"{Data.Length} bytes");

            return result;
        }

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
    }
}
