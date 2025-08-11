using ParquetViewer.Engine.Types;
using System;
using System.Drawing;
using System.IO;

namespace ParquetViewer.Helpers
{
    public static class ByteArrayValueExtensionMethods
    {
        private const string FORMATTING_ERROR_TEXT = "#ERR";

        /// <summary>
        /// Gets the string representation of the binary data in the desired format
        /// </summary>
        /// <param name="desiredFormat">How to interpret the binary data</param>
        /// <param name="desiredLength">An optional maximum string length target to try and achieve (NOT GUARANTEED)</param>
        /// <returns>String representation of the binary data in the desired format if possible.
        /// If conversion fails, <see cref="FORMATTING_ERROR_TEXT"/> is returned instead</returns>
        /// <remarks>Utilize <see cref="ByteArrayValue.PossibleDisplayFormats"/> to avoid calling incompatible conversions</remarks>
        public static string FormatString(this ByteArrayValue byteArrayValue, ByteArrayValue.DisplayFormat desiredFormat, int desiredLength = int.MaxValue)
        {
            ArgumentNullException.ThrowIfNull(byteArrayValue);
            ArgumentOutOfRangeException.ThrowIfLessThan(desiredLength, 1);

            if (desiredFormat == ByteArrayValue.DisplayFormat.IPv4)
            {
                if (byteArrayValue.ToIPv4(out var ipAddress))
                {
                    return ipAddress.ToString();
                }

                return FORMATTING_ERROR_TEXT;
            }
            else if (desiredFormat == ByteArrayValue.DisplayFormat.IPv6)
            {
                if (byteArrayValue.ToIPv6(out var ipAddress))
                {
                    return ipAddress.ToString();
                }

                return FORMATTING_ERROR_TEXT;
            }
            else if (desiredFormat == ByteArrayValue.DisplayFormat.Guid)
            {
                if (byteArrayValue.ToGuid(out var @guid))
                {
                    return @guid.Value.ToString();
                }

                return FORMATTING_ERROR_TEXT;
            }
            else if (desiredFormat == ByteArrayValue.DisplayFormat.Short)
            {
                if (byteArrayValue.ToShort(out var @short))
                {
                    return @short.Value.ToString();
                }

                return FORMATTING_ERROR_TEXT;
            }
            else if (desiredFormat == ByteArrayValue.DisplayFormat.Integer)
            {
                if (byteArrayValue.ToInteger(out var @int))
                {
                    return @int.Value.ToString();
                }

                return FORMATTING_ERROR_TEXT;
            }
            else if (desiredFormat == ByteArrayValue.DisplayFormat.Long)
            {
                if (byteArrayValue.ToLong(out var @long))
                {
                    return @long.Value.ToString();
                }

                return FORMATTING_ERROR_TEXT;
            }
            else if (desiredFormat == ByteArrayValue.DisplayFormat.Float)
            {
                if (byteArrayValue.ToFloat(out var @float))
                {
                    return @float.Value.ToString();
                }

                return FORMATTING_ERROR_TEXT;
            }
            else if (desiredFormat == ByteArrayValue.DisplayFormat.Double)
            {
                if (byteArrayValue.ToDouble(out var @double))
                {
                    return @double.Value.ToString();
                }

                return FORMATTING_ERROR_TEXT;
            }
            else if (desiredFormat == ByteArrayValue.DisplayFormat.ASCII)
            {
                if (byteArrayValue.ToASCII(out var ascii))
                {
                    if (ascii.Length <= desiredLength)
                        return ascii;

                    return ascii[..desiredLength] + "[...]";
                }

                return FORMATTING_ERROR_TEXT;
            }
            else if (desiredFormat == ByteArrayValue.DisplayFormat.Base64)
            {
                byteArrayValue.ToBase64(out var base64);
                if (base64.Length <= desiredLength)
                    return base64;

                return base64[..desiredLength] + "[...]";
            }
            else if (desiredFormat == ByteArrayValue.DisplayFormat.Size)
            {
                return byteArrayValue.Data.Length.ToString() + (byteArrayValue.Data.Length == 1 ? " byte" : " bytes");
            }
            else
            {
                return byteArrayValue.ToStringTruncated(desiredLength);
            }
        }

        /// <remarks>Can't put this into ByteArrayValue itself as that assembly doesn't reference System.Drawing</remarks>
        public static bool ToImage(this ByteArrayValue byteArrayValue, out Image? image)
        {
            ArgumentNullException.ThrowIfNull(byteArrayValue);

            try
            {
                using var ms = new MemoryStream(byteArrayValue.Data);
                image = Image.FromStream(ms);
                return true;
            }
            catch
            {
                image = null;
                return false;
            }
        }
    }
}
