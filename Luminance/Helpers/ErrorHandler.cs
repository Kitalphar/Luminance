using System.Data;
using Microsoft.Data.Sqlite;
using System.Windows;
using Luminance.Services;

namespace Luminance.Helpers
{
    internal class ErrorHandler
    {
        private const string TranslationMissing = "TRANSLATION_MISSING";
        private const string UnknownError = "UNKNOWN_ERROR";
        private const string FatalError = "FATAL_ERROR";
        private const string EnglishLangColumn = "en";

        //Not sure how to do this yet...
        public enum ErrorSeverity
        {
            Recoverable,
            Fatal
        }
        public static string FindErrorMessage(string errorCodeAndId)
        {
            int parenthesisStart = errorCodeAndId.IndexOf('(');
            int parenthesisEnd = errorCodeAndId.IndexOf(')');

            //if (errorCodeAndId.StartsWith("ERR_CODE_"))
            //    return UnknownError;

            if (parenthesisStart == -1 || parenthesisEnd == -1 || parenthesisEnd <= parenthesisStart)
                throw new InvalidOperationException("ERR_CODE_FORMAT_INVALID(401)");

            string errorCode = errorCodeAndId.Substring(0, parenthesisStart);
            string errorId = errorCodeAndId.Substring(parenthesisStart + 1, (parenthesisEnd - parenthesisStart - 1));
            string errorCodeColumn = SqlQueryHelper.errorTableErrorCodeColumn;
            string errorIdColumn = SqlQueryHelper.errorTableIdColumn;

            string? errorMessage = null;

            AppDbQueryCoordinator.RunQuery(conn =>
            {
                string languageColumn = AppSettings.Instance.Get("language");

                //Attempt to find error message with error_code
                errorMessage = ErrorMessageQuery(conn, languageColumn, errorCodeColumn, errorCode, true);

                //if keyphrase query returned translation missing, retry with english language
                //in case it returns null, ID fallback will pick it up.
                if (errorMessage == TranslationMissing)
                    errorMessage = ErrorMessageQuery(conn, EnglishLangColumn, errorCodeColumn, errorCode, false);

                //Fallback to ID
                if (string.IsNullOrWhiteSpace(errorMessage))
                {
                    if (!int.TryParse(errorId, out int id))
                        throw new InvalidOperationException("ERR_CODE_ID_INVALID(402)");

                    //Attempt to find error message with id number
                    errorMessage = ErrorMessageQuery(conn, languageColumn, errorIdColumn, errorId, true);

                    //if id query returned translation missing, retry with english language
                    if (errorMessage == TranslationMissing)
                        errorMessage = ErrorMessageQuery(conn, EnglishLangColumn, errorIdColumn, errorId, false);

                    //if there is no value, it is likely this error does not exist in the database.
                    if (errorMessage == null)
                        throw new DataException("ERR_CODE_ID_NOT_EXIST(403)");
                }
            });

            return errorMessage ?? UnknownError;
        }

        private static string? ErrorMessageQuery(SqliteConnection conn, string langColumn, string column, string value, bool tryFallBack)
        {
            string queryString = SqlQueryHelper.ErrorMessageQueryStringBuilder(langColumn, column);
            string? returnString = null;

            AppDbQueryCoordinator.RunQuery(conn =>
            {
                using var command = new SqliteCommand(queryString, conn);
                command.Parameters.AddWithValue(SqlQueryHelper.valueParam, value);

                using var reader = command.ExecuteReader();
                //If row has value and row value is NOT NULL.
                if (reader.Read() && !reader.IsDBNull(0))
                    returnString = reader.GetString(0); //Value could still be Empty string.
            });

            //Assuming translation is missing, we retry with the english error message.
            if (string.IsNullOrWhiteSpace(returnString) && tryFallBack && langColumn != EnglishLangColumn)
                return TranslationMissing;

            //If fallback is not enabled, returnstring remains NullorWhiteSpace for ID query to kick in.
            return returnString;
        }

        public static void ShowErrorMessage(string errorMessage)
        {
            MessageBoxResult errorMessageBox = MessageBox.Show(
                errorMessage,
                "An Error has occurred.",
                MessageBoxButton.OK,
                MessageBoxImage.Warning
            );
        }
    }
}
