using ParquetViewer.Engine.Types;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Drawing;
using System.IO;
using System.Numerics;
using System.Windows.Forms;

namespace ParquetViewer.Helpers
{
    public static class ExtensionMethods
    {
        private const string DefaultDateTimeFormat = "g";
        private const string DateOnlyDateTimeFormat = "d";
        private const string ISO8601DateTimeFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";
        private const string ISO8601DateOnlyFormat = "yyyy-MM-dd";
        private const string ISO8601Alt1DateTimeFormat = "yyyy-MM-dd HH:mm:ss.fff";
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
            foreach (DataColumn column in datatable.Columns)
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
            DateFormat.ISO8601_DateOnly => ISO8601DateOnlyFormat, // obsolete
            DateFormat.Default_DateOnly => DateOnlyDateTimeFormat, // obsolete
            DateFormat.ISO8601_Alt1 => ISO8601Alt1DateTimeFormat,
            DateFormat.ISO8601_Alt2 => ISO8601Alt2DateTimeFormat,
            DateFormat.Default => DefaultDateTimeFormat,
            _ => string.Empty
        };

        public static string GetExtension(this FileType fileType) => fileType switch
        {
            FileType.CSV => ".csv",
            FileType.XLS => ".xls",
            _ => throw new ArgumentOutOfRangeException(nameof(fileType))
        };

        public static long ToMillisecondsSinceEpoch(this DateTime dateTime) => new DateTimeOffset(dateTime).ToUnixTimeMilliseconds();

        public static Image ToImage(this ByteArrayValue byteArrayValue)
        {
            if (byteArrayValue is null)
                throw new ArgumentNullException(nameof(byteArrayValue));

            using var ms = new MemoryStream(byteArrayValue.Data);
            return Image.FromStream(ms);
        }

        public static bool IsImage(this ByteArrayValue byteArrayValue)
        {
            if (byteArrayValue is null)
                throw new ArgumentNullException(nameof(byteArrayValue));

            try
            {
                using var ms = new MemoryStream(byteArrayValue.Data);
                Image.FromStream(ms);
                return true;
            }
            catch
            {
                return false;
            }
        }

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

        /// <summary>
        /// Returns true if the type is a "simple" type. Basically anything that isn't a class or array.
        /// </summary>
        /// <remarks>Source: https://stackoverflow.com/a/65079923/1458738</remarks>
        public static bool IsSimple(this Type type) 
            => TypeDescriptor.GetConverter(type).CanConvertFrom(typeof(string));

        /// <summary>
        /// Returns true if the type is a number type.
        /// </summary>
        public static bool IsNumber(this Type type) => 
            Array.Exists(type.GetInterfaces(), i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(INumber<>));
    }
}
