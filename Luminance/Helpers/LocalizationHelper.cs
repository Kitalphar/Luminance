using System.Diagnostics;
using Luminance.Services;
using Microsoft.Data.Sqlite;

namespace Luminance.Helpers
{
    class LocalizationHelper
    {
        private string _localizationKey = string.Empty;
        private int _localizationId = 0;
        private string _localizationLanguage = "en"; // Default fallback
        private bool _isWithEscapeCharacter = false;

        private List<int> _localizationIdList = new();
        public LocalizationHelper(string key, int id, string language, bool isWithEscapeCharacter)
        {
            _localizationKey = key;
            _localizationId = id;
            _localizationLanguage = language;
            _isWithEscapeCharacter = isWithEscapeCharacter;
        }

        public enum LocalizationFilter
        {
            Key,
            KeyWithEscape,
            Id,
            Default
        }

        public string GetLocalizedString()
        {
            var language = AppSettings.Instance.Get("language");

            //Trying to get translated string by their string Key.
            string result = QueryLocalizedString(LocalizationFilter.Key);

            //Trying to get translated string by their ID.
            if (string.IsNullOrWhiteSpace(result))
                result = QueryLocalizedString(LocalizationFilter.Id);

            //Fallback to english language query
            if (string.IsNullOrWhiteSpace(result))
                result = QueryLocalizedString(LocalizationFilter.Default);

            return result ?? string.Empty;
        }

        public Dictionary<string, string> GetLocalizedStringsDictionary()
        {
            //Trying to get translated string by their string Key.
            var results = QueryLocalizedStrings(_isWithEscapeCharacter
                ? LocalizationFilter.KeyWithEscape
                : LocalizationFilter.Key);

            if (results.Count > 0)
                return results;

            //Trying to get translated string by their ID.
            results = QueryLocalizedStrings(LocalizationFilter.Id);

            if (results.Count > 0)
                return results;

            //Fallback to english language query
            results = QueryLocalizedStrings(LocalizationFilter.Default);

            return results;
        }

        private string QueryLocalizedString(LocalizationFilter filter)
        {
            string queryString = filter switch
            {
                LocalizationFilter.Key => SqlQueryHelper.SingleReturnLocalisationQueryStringBuilder(_localizationLanguage!),
                LocalizationFilter.Id => SqlQueryHelper.SingleReturnLocalisationByIdQueryStringBuilder(_localizationLanguage!),
                LocalizationFilter.Default => SqlQueryHelper.LocalizationFallbackByIdQueryString,
                _ => throw new ArgumentOutOfRangeException(nameof(filter), filter, null)
            };

            string returnString = String.Empty;

            AppDbQueryCoordinator.RunQuery(conn =>
            {
                using var command = new SqliteCommand(queryString, conn);

                switch (filter)
                {
                    case LocalizationFilter.Key:
                        command.Parameters.AddWithValue(SqlQueryHelper.keyParam, _localizationKey);
                        break;
                    case LocalizationFilter.Id:
                    case LocalizationFilter.Default:
                        command.Parameters.AddWithValue(SqlQueryHelper.idParam, _localizationId);
                        break;
                }

                using var reader = command.ExecuteReader();
                if (reader.Read())
                    returnString = reader.GetString(0);
            });

            return returnString;
        }

        private Dictionary<string, string> QueryLocalizedStrings(LocalizationFilter filter)
        {
            string queryString = filter switch
            {
                LocalizationFilter.Key => SqlQueryHelper.MultiReturnLocalisationQueryStringBuilder(_localizationLanguage!),
                LocalizationFilter.KeyWithEscape => SqlQueryHelper.MultiReturnLocalisationQueryStringBuilderWithEscape(_localizationLanguage!),
                LocalizationFilter.Id => SqlQueryHelper.MultiReturnLocalisationByIdQueryStringBuilder(_localizationLanguage!),
                LocalizationFilter.Default => SqlQueryHelper.LocalizationMultiReturnFallbackQueryString,
                _ => throw new ArgumentOutOfRangeException(nameof(filter), filter, null)
            };

            Dictionary<string, string> returnStrings = new Dictionary<string, string>();

            if (filter ==LocalizationFilter.Id)
            {
                if (!_isWithEscapeCharacter)
                    queryString = SqlQueryHelper.MultiIdReturnByStringKeyHelperQueryString;
                else
                    queryString = SqlQueryHelper.MultiIdReturnByStringKeyWithEscapeHelperQueryString;

                    AppDbQueryCoordinator.RunQuery(conn =>
                    {
                        using var command = new SqliteCommand(queryString, conn);


                        using var reader = command.ExecuteReader();

                        while (reader.Read())
                        {
                            _localizationIdList.Add(reader.GetInt32(0));
                        }
                    });

                string idList = string.Join(", ", _localizationIdList);
                queryString = SqlQueryHelper.MultiReturnLocalisationByIdQueryStringBuilder(_localizationLanguage);
                queryString = queryString.Replace(SqlQueryHelper.idParam, idList);
            }

            if (filter == LocalizationFilter.Default && _isWithEscapeCharacter)
            {
                queryString = SqlQueryHelper.LocalizationMultiReturnFallbackWithEscapeQueryString;
            }

            AppDbQueryCoordinator.RunQuery(conn =>
            {
                using var command = new SqliteCommand(queryString, conn);

                switch (filter)
                {
                    case LocalizationFilter.Key:
                    case LocalizationFilter.KeyWithEscape:
                        command.Parameters.AddWithValue(SqlQueryHelper.keyParam, _localizationKey);
                        Trace.WriteLine(queryString);
                        break;
                    case LocalizationFilter.Default:
                        command.Parameters.AddWithValue(SqlQueryHelper.idParam, _localizationId);
                        break;
                }

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    returnStrings.Add(reader.GetString(0), reader.GetString(1));
                }

            });

            return returnStrings;
        }
    }
}
