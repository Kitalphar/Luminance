using System.Dynamic;
using System.Globalization;
using System.IO;
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

            string[] firtRow = firstLine.Split(",");
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
            int headerScore = 0;

            for (int i = 0; i < Math.Min(firstRow.Length, secondRow.Length); i++)
            {
                bool firstIsString = !double.TryParse(firstRow[i], out _);
                bool secondIsNumber = double.TryParse(secondRow[i], out _);

                if (firstIsString && secondIsNumber)
                    headerScore++;
            }

            return headerScore > (firstRow.Length / 2); // Simple majority rule
        }


        private static List<dynamic> ParseCsvWithHeader(TextReader reader)
        {
            //FINISH THIS
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true
            };
            using var csv = new CsvReader(reader, config);
           
            return csv.GetRecords<dynamic>().ToList();
        }

        private static List<dynamic> ParseCsvWithoutHeader(TextReader reader)
        {
            //FINISH THIS
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false
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
