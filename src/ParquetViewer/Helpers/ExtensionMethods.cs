using Microsoft.Win32;
using ParquetViewer.Engine.ParquetNET.Types;
using ParquetViewer.Engine.Types;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Windows.Forms;

namespace ParquetViewer.Helpers
{
    public static class ExtensionMethods
    {
        private const string DefaultDateTimeFormat = "g";
        private const string DefaultDateOnlyFormat = "d";
        public const string ISO8601DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.FFFFFFF";
        public const string ISO8601DateOnlyFormat = "yyyy-MM-dd";

        /// <summary>
        /// Returns a list of all column names within a given datatable
        /// </summary>
        /// <param name="datatable">The datatable to retrieve the column names from</param>
        /// <returns></returns>
        public static IList<string> GetColumnNames(this DataTable datatable)
        {
            List<string> columns = new List<string>(datatable.Columns.Count);
            foreach (System.Data.DataColumn column in datatable.Columns)
            {
                columns.Add(column.ColumnName);
            }
            return columns;
        }

        /// <summary>
        /// Gets the corresponding date format string for the provided <paramref name="dateFormat"/>
        /// </summary>
        /// <param name="dateFormat">Date format to get formatting string for</param>
        /// <returns>A formatting string such as: YYYY-MM-dd that is passible to DateTime.ToString()</returns>
        public static string GetDateFormat(this DateFormat dateFormat) => dateFormat switch
        {
            DateFormat.ISO8601 => ISO8601DateTimeFormat,
            DateFormat.Default => DefaultDateTimeFormat,
            DateFormat.Custom => AppSettings.CustomDateFormat ?? DefaultDateTimeFormat,
            _ => string.Empty
        };

        public static string GetDateOnlyFormat(this DateFormat dateFormat) => dateFormat switch
        {
            DateFormat.ISO8601 => ISO8601DateOnlyFormat,
            DateFormat.Default => DefaultDateOnlyFormat,
            DateFormat.Custom => AppSettings.CustomDateFormat is not null ?
                UtilityMethods.StripTimeComponentsFromDateFormat(AppSettings.CustomDateFormat) : DefaultDateOnlyFormat,
            _ => string.Empty
        };

        public static string GetExtension(this FileType fileType)
            => Enum.IsDefined(fileType)
            ? $".{fileType.ToString().ToLowerInvariant()}"
            : throw new ArgumentOutOfRangeException(nameof(fileType));

        public static long ToMillisecondsSinceEpoch(this DateTime dateTime) => new DateTimeOffset(dateTime).ToUnixTimeMilliseconds();

        public static Size RenderedSize(this PictureBox pictureBox)
        {
            var wfactor = (double)pictureBox.Image.Width / pictureBox.ClientSize.Width;
            var hfactor = (double)pictureBox.Image.Height / pictureBox.ClientSize.Height;

            var resizeFactor = Math.Max(wfactor, hfactor);
            return new Size((int)(pictureBox.Image.Width / resizeFactor), (int)(pictureBox.Image.Height / resizeFactor));
        }

        public static IEnumerable<DataColumn> AsEnumerable(this DataColumnCollection columns)
        {
            foreach (DataColumn column in columns)
            {
                yield return column;
            }
        }

        public static IEnumerable<DataGridViewCell> AsEnumerable(this DataGridViewSelectedCellCollection cells)
        {
            foreach (DataGridViewCell cell in cells)
            {
                yield return cell;
            }
        }

        /// <summary>
        /// Returns true if the type is a "simple" type. Basically anything that isn't a class, struct or array.
        /// </summary>
        /// <remarks>Source: https://stackoverflow.com/a/65079923/1458738</remarks>
        public static bool IsSimple(this Type type)
            => TypeDescriptor.GetConverter(type).CanConvertFrom(typeof(string));

        /// <summary>
        /// Returns true if the type is a number type.
        /// </summary>
        public static bool IsNumber(this Type type) =>
            System.Array.Exists(type.GetInterfaces(), i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(INumber<>));

        public static T ToEnum<T>(this int value, T @default) where T : struct, Enum
        {
            if (Enum.IsDefined(typeof(T), value))
            {
                return (T)Enum.ToObject(typeof(T), value);
            }
            return @default;
        }

        public static void DeleteSubKeyTreeIfExists(this RegistryKey key, string name)
        {
            if (key.OpenSubKey(name) is not null)
            {
                key.DeleteSubKeyTree(name);
            }
        }

        public static Array GetColumnValues(this DataTable dataTable, Type type, string columnName, int skipCount, int fetchCount)
        {
            ArgumentNullException.ThrowIfNull(dataTable);
            ArgumentNullException.ThrowIfNull(type);
            ArgumentOutOfRangeException.ThrowIfLessThan(skipCount, 0);
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(fetchCount, 0);

            if (!dataTable.Columns.Contains(columnName))
                throw new ArgumentException($"Column `{columnName}` does not exist in the datatable");

            var recordCountAfterSkip = dataTable.Rows.Count - skipCount;
            var recordCountToRead = fetchCount > recordCountAfterSkip ? recordCountAfterSkip : fetchCount;
            var values = Array.CreateInstance(type, recordCountToRead);
            var index = 0;
            foreach(DataRow row in dataTable.Rows)
            {
                if (skipCount-- > 0)
                {
                    continue;
                }

                var value = row[columnName];
                if (value == DBNull.Value)
                    value = null;
                else if (value is IByteArrayValue byteArray)
                    value = byteArray.Data;
                else if (value is IListValue || value is IMapValue || value is IStructValue)
                    throw new NotSupportedException("List, Map, and Struct types are currently not supported.");

                values.SetValue(value, index++);

                if (--fetchCount <= 0)
                {
                    break;
                }
            }

            return values;
        }

        public static Type GetNullableVersion(this Type sourceType) => sourceType == null
                ? throw new ArgumentNullException(nameof(sourceType))
                : !sourceType.IsValueType
                    || (sourceType.IsGenericType
                        && sourceType.GetGenericTypeDefinition() == typeof(Nullable<>))
                ? sourceType
                : typeof(Nullable<>).MakeGenericType(sourceType);

        /// <summary>
        /// Converts a float to a string without using the scientific notation, if possible
        /// </summary>
        /// <returns><paramref name="floatValue"/> in its decimal representation. `null` if the decimal conversion fails.</returns>
        public static string? ToDecimalString(this float floatValue)
            => floatValue >= (float)decimal.MinValue && floatValue <= (float)decimal.MaxValue ? ToDecimalStringImpl(floatValue) : null;

        /// <summary>
        /// Converts a double to a string without using the scientific notation, if possible
        /// </summary>
        /// <returns><paramref name="doubleValue"/> in its decimal representation. `null` if the decimal conversion fails.</returns>
        public static string? ToDecimalString(this double doubleValue)
            => doubleValue >= (double)decimal.MinValue && doubleValue <= (double)decimal.MaxValue ? ToDecimalStringImpl(doubleValue) : null;

        private static string? ToDecimalStringImpl(object value)
        {
            try
            {
                return Convert.ToDecimal(value).ToString();
            }
            catch
            {
                return null;
            }
        }

        //Source: https://stackoverflow.com/a/7574615/1458738
        public static string Left(this string value, int maxLength, string? truncateSuffix = null)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            maxLength = Math.Abs(maxLength);
            return value.Length <= maxLength ? value : (value.Substring(0, maxLength) + truncateSuffix);
        }

        public static IEnumerable<ToolStripItem> Children(this MenuStrip menuStrip)
        {
            foreach (ToolStripItem toolStrip in menuStrip.Items)
            {
                yield return toolStrip;

                if (toolStrip is ToolStripMenuItem childStrip)
                {
                    foreach (var child in childStrip.Children())
                    {
                        yield return child;
                    }
                }
            }
        }

        private static IEnumerable<ToolStripItem> Children(this ToolStripItem toolStrip)
        {
            if (toolStrip is ToolStripMenuItem menuItem)
            {
                foreach (ToolStripItem childStrip in menuItem.DropDownItems)
                {
                    yield return childStrip;
                    foreach (ToolStripItem cc in childStrip.Children())
                    {
                        yield return cc;
                    }
                }
            }
        }

        public static IEnumerable<T> AppendIf<T>(this IEnumerable<T> enumerable, bool append, T value)
        {
            if (append)
                return enumerable.Append(value);
            else
                return enumerable;
        }

        /// <remarks>Can't put this into IByteArrayValue itself as that assembly doesn't reference System.Drawing</remarks>
        public static bool ToImage(this IByteArrayValue byteArrayValue, [NotNullWhen(true)] out Image? image)
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

        public static void DisposeSafely(this IDisposable? disposable)
        {
            try
            {
                disposable?.Dispose();
            }
            catch { /*swallow*/ }
        }

        public static bool ImplementsInterface<T>(this Type type)
        {
            if (type is null)
                return false;
            else
                return typeof(T).IsAssignableFrom(type);
        }

        public static string Format(this string formatString, params object?[] args) 
            => string.Format(formatString, args);

        /// <summary>
        /// https://huggingface.co/docs/hub/en/datasets-image#parquet-format
        /// </summary>
        /// <returns>True if this is a struct named "image" with "bytes" and "path" fields</returns>
        public static bool IsHuggingFaceImageFormat(this IStructValue structValue, [NotNullWhen(true)] out byte[]? data)
        {
            if (structValue.Name == "image" //Should we allow other names?
                && structValue.Data.Columns.Keys.Count == 2
                && structValue.Data.Columns.Keys.Contains("bytes")
                && structValue.Data.Columns.Keys.Contains("path")
                && structValue.Data.GetValue("bytes") is ByteArrayValue byteArrayValue)
            {
                data = byteArrayValue.Data;
                return true;
            }
            data = null;
            return false;
        }
    }
}