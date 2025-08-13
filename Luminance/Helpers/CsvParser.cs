using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using CsvHelper;
using CsvHelper.Configuration;

namespace Luminance.Helpers
{
    internal class CsvParser
    {
        public static List<dynamic> ParseCsv<T>(string csvPath)
        {
            using var reader = new StreamReader(csvPath);

            var firstLine = reader.ReadLine();
            var secondLine = reader.ReadLine();

            if (firstLine == null)
                return new List<dynamic>();

            string[] firtRow = firstLine.Split(",") ?? Array.Empty<string>();
            string[] secondRow = secondLine?.Split(",") ?? Array.Empty<string>();

            bool hasHeader = IsProbablyHeader(firtRow, secondRow);

            reader.BaseStream.Seek(0, SeekOrigin.Begin);
            reader.DiscardBufferedData();

            return hasHeader
                ? ParseCsvWithHeader(reader)
                : ParseCsvWithoutHeader(reader);
        }

        private static bool IsProbablyHeader(string[] firstRow, string[] secondRow)
        {
            int score = 0;
            int totalChecks = 0;
            double confidenceScore = 0;

            //Check 0: First & Second row must exist and contain at least one non-empty field.
            if (firstRow.Length == 0 || firstRow.All(string.IsNullOrWhiteSpace))
                throw new InvalidOperationException("ERR_CSV_MISSING_ROW(404)");

            if (secondRow.Length == 0 || secondRow.All(string.IsNullOrWhiteSpace))
                throw new InvalidOperationException("ERR_CSV_MISSING_ROW(404)");

            //Check 1: Type mismatch between rows
            totalChecks++;
            int typeMismatchCount = 0;

            for (int i = 0; i < Math.Min(firstRow.Length, secondRow.Length); i++)
            {
                bool firstIsString = !double.TryParse(firstRow[i], out _) && !DateTime.TryParse(firstRow[i], out _);
                bool secondIsNumberOrDate = double.TryParse(secondRow[i], out _) || DateTime.TryParse(secondRow[i], out _);


                if (firstIsString && secondIsNumberOrDate)
                    typeMismatchCount++;
            }

            //If first row has string and second row has type mismatch, it is likely a header.
            if (typeMismatchCount > 0)
                score++;

            //Check 2: Header-like formatting. (underscores, spaces, camelCase, etc...)
            totalChecks++;
            int headerLikeFormatCount = firstRow.Count(s => Regex.IsMatch(s, @"[\s_]|[a-z][A-Z]"));

            if (headerLikeFormatCount > 0)
                score++;

            //Check 3: Common header terms
            totalChecks++;
            string[] commonTerms = { "id", "name", "date", "code", "type", "amount", "value", "desc" };
            int headerKeywordCount = firstRow.Count(cell => commonTerms.Any(term => cell.Trim().ToLower().Contains(term)));

            if (headerKeywordCount > 0)
                score++;

            //Check 4: Duplicates in first row.
            totalChecks++ ;
            if (firstRow.Distinct().Count() != firstRow.Length)
                score -= 2; //if true, it's invalid header.

            //Normalize score
            confidenceScore = Math.Clamp((double)score / totalChecks, 0, 1);

            //With 0.5 tresshold we need at least 2 valid checks and no duplicates in first row to pass.
            return confidenceScore >= 0.5;
        }

        private static List<dynamic> ParseCsvWithHeader(TextReader reader)
        {
            
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                IgnoreBlankLines = true,
                MissingFieldFound = null
            };
            using var csv = new CsvReader(reader, config);

            var records = new List<dynamic>();

            while (csv.Read())
            {
                dynamic row = new ExpandoObject();
                var dict = (IDictionary<string, object>)row;

                foreach (var header in csv.HeaderRecord)
                {
                    dict[header] = csv.GetField(header);
                }

                records.Add(row);
            }

            return records;

            //return csv.GetRecords<dynamic>().ToList();
        }

        private static List<dynamic> ParseCsvWithoutHeader(TextReader reader)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false,
                IgnoreBlankLines = true,
                MissingFieldFound = null
            };

            using var csv = new CsvReader(reader, config);

            var results = new List<dynamic>();

            while (csv.Read())
            {
                dynamic row = new ExpandoObject();
                var dict = (IDictionary<string, object>)row;

                for (int i = 0; csv.TryGetField(i, out string? value); i++)
                {
                    dict[$"Column{i + 1}"] = value;
                }

                results.Add(row);
            }

            return results;
        }

        //This grabs the keys(column names) from the first row:
        public static List<string> GetColumnNames(List<dynamic> records)
        {
            if (records == null || records.Count == 0)
                return new List<string>();

            var firstRow = (IDictionary<string, object>)records[0];
            return firstRow.Keys.ToList();
        }

        //Get All Values for a Given Column
        public static List<string> GetColumnValues(List<dynamic> records, string columnName)
        {
            var values = new List<string>();

            foreach (var row in records)
            {
                var dict = (IDictionary<string, object>)row;
                if (dict.TryGetValue(columnName, out var value))
                {
                    values.Add(value?.ToString() ?? "");
                }
                else
                {
                    values.Add(""); // or throw if strict
                }
            }

            return values;
        }



        // THIS DOES NOT BELONG HERE
        //public void SeedCategories(string dbPath)
        //{
        //    using var connection = new SqliteConnection($"Data Source={dbPath}");
        //    connection.Open();

        //    using var reader = new StreamReader("Data/categories.csv");
        //    using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        //    var records = csv.GetRecords<Category>();

        //    foreach (var category in records)
        //    {
        //        using var command = connection.CreateCommand();
        //        command.CommandText = "INSERT INTO Categories (Id, Name) VALUES (@Id, @Name)";
        //        command.Parameters.AddWithValue("@Id", category.Id);
        //        command.Parameters.AddWithValue("@Name", category.Name);
        //        command.ExecuteNonQuery();
        //    }
        //}

    }
}
