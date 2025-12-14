using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Fed.Core.Common
{
    public class CsvParser
    {
        private static (IDictionary<string, int>, IList<string>) ReadCsv(string fileName)
        {
            var csvLines = File.ReadAllLines(fileName);

            var headers = new Dictionary<string, int>();

            var rawHeaders =
                csvLines[0]
                    .Split(new[] { ',' })
                    .Where(h => !string.IsNullOrEmpty(h))
                    .ToList();

            for (var i = 0; i < rawHeaders.Count; i++)
                headers.Add(rawHeaders[i].Trim(), i);

            var lines = csvLines.Skip(1).ToList();

            return (headers, lines);
        }

        private static string[] SplitIntoValues(string line, int length)
        {
            var values = new List<string>();
            var sb = new StringBuilder();
            var isDoubleQuoteOpen = false;

            var chars = line.ToCharArray();

            foreach (var c in chars)
            {
                if (c == ',' && !isDoubleQuoteOpen)
                {
                    var value = sb.ToString();
                    sb.Clear();
                    values.Add(value);
                }
                else if (c == '"')
                {
                    isDoubleQuoteOpen = !isDoubleQuoteOpen;
                }
                else
                {
                    sb.Append(c);
                }
            }

            values.Add(sb.ToString());

            return values.ToArray();
        }

        public static IList<T> ParseCsv<T>(string csvPath, Func<IDictionary<string, int>, string[], T> parseItem)
        {
            var items = new List<T>();
            var (headers, lines) = ReadCsv(csvPath);

            foreach (var line in lines)
            {
                try
                {
                    var values = SplitIntoValues(line, headers.Count());
                    var item = parseItem(headers, values);
                    items.Add(item);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR: An error occured parsing a row from the CSV file: {csvPath}");
                    Console.WriteLine(ex.Message);
                    Console.WriteLine($"Line: {line}");
                }
            }

            return items;
        }
    }
}
