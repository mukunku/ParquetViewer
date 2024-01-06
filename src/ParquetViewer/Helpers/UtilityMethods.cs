using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;

namespace ParquetViewer.Helpers
{
    public static class UtilityMethods
    {
        public static string CleanCSVValue(string value, bool alwaysEncloseInQuotes = false)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                //In RFC 4180 we escape quotes with double quotes
                string formattedValue = value.Replace("\"", "\"\"");

                //Enclose value with quotes if it contains commas,line feeds or other quotes
                if (formattedValue.Contains(",") || formattedValue.Contains("\r") || formattedValue.Contains("\n") || formattedValue.Contains("\"\"") || alwaysEncloseInQuotes)
                    formattedValue = string.Concat("\"", formattedValue, "\"");

                return formattedValue;
            }
            else
                return string.Empty;
        }

        public static IEnumerable<IList<T>> Split<T>(ICollection<T> src, int splitIntoPieces)
        {
            int maxItems = src.Count / splitIntoPieces;
            int remainder = src.Count % splitIntoPieces;
            bool pieceDone = false;

            var list = new List<T>();
            foreach (var t in src)
            {
                list.Add(t);

                if (remainder > 0 && !pieceDone)
                {
                    remainder--;
                    pieceDone = true;
                    continue;
                }

                if (list.Count == maxItems || pieceDone)
                {
                    pieceDone = false;
                    yield return list;
                    list = new List<T>();
                }
            }

            if (list.Count > 0)
                yield return list;
        }
    }
}
