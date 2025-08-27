using Luminance.Services;
using Microsoft.Data.Sqlite;

namespace Luminance.Helpers
{
    class LocalizationHelper
    {
        private string _localizationKey = string.Empty;
        private int _localizationId = 0;
        private string _localizationLanguage = "en"; // Default fallback

        public LocalizationHelper(string key, int id, string language)
        {
            _localizationKey = key;
            _localizationId = id;
            _localizationLanguage = language;
        }

        public enum LocalizationFilter
        {
            Key,
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

        private string QueryLocalizedString(LocalizationFilter filter)
        {
            string queryString = filter switch
            {
                LocalizationFilter.Key => SqlQueryHelper.SingleReturnLocalisationQueryStringBuilder(_localizationLanguage!),
                LocalizationFilter.Id => SqlQueryHelper.SingleReturnLocalisationIdQueryStringBuilder(_localizationLanguage!),
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
    }
}
