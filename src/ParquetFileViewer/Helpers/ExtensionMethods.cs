using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace ParquetFileViewer.Helpers
{
    public static class ExtensionMethods
    {
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

        public static string FormatJSON(this string input)
        {
            if (input == null)
                return null;

            try
            {
                return JValue.Parse(input).ToString(Formatting.Indented);
            }
            catch (Exception)
            {
                //malformed json detected
                return input;
            }
        }

        public static string Base64Encode(this string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        public static string Base64Decode(this string base64EncodedData)
        {
            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }
    }
}
