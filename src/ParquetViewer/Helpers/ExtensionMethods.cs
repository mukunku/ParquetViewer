using Microsoft.Win32;
using ParquetViewer.Engine.Types;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Windows.Forms;

namespace ParquetViewer.Helpers
{
    public static class ExtensionMethods
    {
        private const string DefaultDateTimeFormat = "g";
        public const string ISO8601DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.FFFFFFF";

        [Obsolete]
        private const string ISO8601Alt1DateTimeFormat = "yyyy-MM-dd HH:mm:ss.fff";
        [Obsolete]
        private const string ISO8601Alt2DateTimeFormat = "yyyy-MM-dd HH:mm:ss";

        public static DataGridViewAutoSizeColumnsMode ToDGVMode(this AutoSizeColumnsMode mode) => mode switch
        {
            AutoSizeColumnsMode.ColumnHeader => DataGridViewAutoSizeColumnsMode.ColumnHeader,
            AutoSizeColumnsMode.AllCells => DataGridViewAutoSizeColumnsMode.AllCells,
            _ => DataGridViewAutoSizeColumnsMode.None
        };

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
#pragma warning disable CS0612 // Type or member is obsolete
        public static string GetDateFormat(this DateFormat dateFormat) => dateFormat switch
        {
            DateFormat.ISO8601 => ISO8601DateTimeFormat,
            //TODO: Get rid of this code that handles obsolete date formats after a few releases
            DateFormat.ISO8601_Alt1 => ISO8601Alt1DateTimeFormat,
            DateFormat.ISO8601_Alt2 => ISO8601Alt2DateTimeFormat,
            DateFormat.Default => DefaultDateTimeFormat,
            DateFormat.Custom => AppSettings.CustomDateFormat ?? DefaultDateTimeFormat,
            _ => string.Empty
        };
#pragma warning restore CS0612 // Type or member is obsolete

        public static string GetExtension(this FileType fileType) => fileType switch
        {
            FileType.CSV => ".csv",
            FileType.XLS => ".xls",
            FileType.JSON => ".json",
            FileType.PARQUET => ".parquet",
            _ => throw new ArgumentOutOfRangeException(nameof(fileType))
        };

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

        public static System.Array GetColumnValues(this DataTable dataTable, Type type, string columnName)
        {
            ArgumentNullException.ThrowIfNull(dataTable);

            if (!dataTable.Columns.Contains(columnName))
                throw new ArgumentException($"Column '{columnName}' does not exist in the datatable");

            var values = System.Array.CreateInstance(type, dataTable.Rows.Count);
            for (var i = 0; i < dataTable.Rows.Count; i++)
            {
                var value = dataTable.Rows[i][columnName];
                if (value == DBNull.Value)
                    value = null;
                else if (value is ByteArrayValue byteArray)
                    value = byteArray.Data;
                else if (value is ListValue || value is MapValue || value is StructValue)
                    throw new NotSupportedException("List, Map, and Struct types are currently not supported.");

                values.SetValue(value, i);
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

        private const float DECIMAL_MIN_FLOAT = (float)decimal.MinValue;
        private const float DECIMAL_MAX_FLOAT = (float)decimal.MaxValue;
        private const double DECIMAL_MIN_DOUBLE = (double)decimal.MinValue;
        private const double DECIMAL_MAX_DOUBLE = (double)decimal.MaxValue;

        /// <summary>
        /// Converts a float to a string without using the scientific notation, if possible
        /// </summary>
        public static string ToDecimalString(this float floatValue, string? valueOverrideIfFormattingFails = null)
            => ToDecimalStringImpl(floatValue, floatValue >= DECIMAL_MIN_FLOAT && floatValue <= DECIMAL_MAX_FLOAT, valueOverrideIfFormattingFails);

        /// <summary>
        /// Converts a double to a string without using the scientific notation, if possible
        /// </summary>
        public static string ToDecimalString(this double doubleValue, string? valueOverrideIfFormattingFails = null)
            => ToDecimalStringImpl(doubleValue, doubleValue >= DECIMAL_MIN_DOUBLE && doubleValue <= DECIMAL_MAX_DOUBLE, valueOverrideIfFormattingFails);

        private static string ToDecimalStringImpl(object value, bool isInRange, string? valueOverrideIfFormattingFails)
        {
            var formattedValue = value?.ToString() ?? string.Empty;

            if (!isInRange)
            {
                if (!string.IsNullOrWhiteSpace(valueOverrideIfFormattingFails))
                    return valueOverrideIfFormattingFails;
                else
                    return formattedValue;
            }

            var isUsingScientificNotation = formattedValue.Contains('E', StringComparison.InvariantCultureIgnoreCase);
            if (isUsingScientificNotation)
            {
                try
                {
                    //Convert the float/double to a decimal which is formatted much nicer as string
                    formattedValue = Convert.ToDecimal(value).ToString();
                }
                catch
                {
                    if (!string.IsNullOrWhiteSpace(valueOverrideIfFormattingFails))
                    {
                        formattedValue = valueOverrideIfFormattingFails;
                    }
                }
            }
            return formattedValue;
        }

        //Source: https://stackoverflow.com/a/7574615/1458738
        public static string Left(this string value, int maxLength, string? truncateSuffix = null)
        {
            if (string.IsNullOrEmpty(value)) return value;
            maxLength = Math.Abs(maxLength);

            return (value.Length <= maxLength
                   ? value
                   : (value.Substring(0, maxLength) + truncateSuffix)
                   );
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
