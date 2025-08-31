using System.Security.RightsManagement;

namespace Luminance.Helpers
{
    internal class SqlQueryHelper
    {
        //NOTE: PARAMETERS CAN NOT BE USED FOR COLUMNS!!!!!

        //Public Parameters
        public const string usernameParam = "@username";
        public const string passwordParam = "@password";
        public const string passwordSaltParam = "@pwsalt";
        public const string userKeyParam = "@userkey";
        public const string userDbParam = "@userdbname";
        public const string userFieldKeyParam = "@fieldkey";
        public const string idParam = "@id";
        public const string keyParam = "@key";
        public const string nameParam = "@name";
        public const string typeParam = "@type";
        public const string valueParam = "@value";
        public const string filterParam = "@filter";
        public const string languageParam = "@lang";
        public const string descriptionParam = "@description";
        public const string categoryParam = "@category";
        public const string currencyParam = "@currency";
        public const string dateParam = "@date";

        //App.db accounts table parameters
        private const string appAccountsDataTable = "accounts";
        private const string appAccountUserNameColumn = "user_name";
        public const string appAccountsTableUserDbColumn = "user_db";
        public const string appAccountsTableUserKeyColumn = "user_key";
        public const string appAccountsTablePasswordSaltColumn = "pw_salt";

        //App.db settings table parameters
        private const string appSettingsDataTable = "settings";
        private const string appSettingsTableKeyColumn = "key";
        private const string appSettingsTableValueColumn = "value";

        //App.db db_scripts table parameters
        private const string scriptDataTable = "db_scripts";
        private const string scriptTableScriptContentColumn = "script_content";
        private const string scriptTableScriptIdColumn = "script_id";
        private const string scriptTableScriptTypeColumn = "type";

        //App.db localized_strings table parameters
        private const string localizedStringsTable = "localized_strings";
        private const string localizationTableIdColumn = "id";
        private const string localizationTableKeyColumn = "key";
        private const string localizationDefaultLanguage = "en";

        //App.db error_message table parameters.
        public const string errorMessagesDataTable = "error_messages";
        public const string errorTableIdColumn = "id";
        public const string errorTableErrorCodeColumn = "error_code";

        //User.db accounts table parameters
        private const string userAccountsDataTable = "accounts";
        private const string userAccountsAccountIdColumn = "account_id";
        private const string userAccountsAccountNameColumn = "account_name";
        private const string userAccountsCurrencyCodeColumn = "currency_code";
        private const string userAccountsBalanceColumn = "balance";

        //User.db categories table parameters
        public const string categoriesDataTable = "categories";
        public const string categoriesTableIdColumn = "category_id";
        public const string categoriesTableNameColumn = "category_name_en"; //EN burned in for now, as no other language is available yet.
        public const string categoriesTableTypeColumn = "type";
        public const string categoriesTableParentIdColumn = "parent_category_id";
        public const string categoriesTableParentIdParam = "@parentid";

        //User.db currencies table parameters.
        public const string currenciesDataTable = "currencies";
        public const string currenciesTableIdColumn = "currency_code";
        public const string currenciesTableSymbolColumn = "currency_symbol";
        public const string currenciesTableNameColumn = "currency_name";

        //User.db Transactions table parameters.
        public const string TransactionsDataTable = "transactions";
        public const string TransactionsTableTransactionIdColumn= "transaction_id";
        public const string TransactionsTableAccountIdColumn = "account_id";
        public const string TransactionsTableDescriptionColumn = "description";
        public const string TransactionsTableTransAmountColumn = "trans_amount";
        public const string TransactionsTableCurrencyCodeColumn = "currency_code";
        public const string TransactionsTableCategoryIdColumn = "category_id";
        public const string TransactionsTableDateColumn = "trans_date";


        //Localisation Queries
        public const string LocalizationFallbackByIdQueryString = $"SELECT {localizationDefaultLanguage} FROM {localizedStringsTable} WHERE {localizationTableIdColumn} = {idParam} LIMIT 1";
        public const string LocalizationMultiReturnFallbackQueryString = $"SELECT {localizationTableKeyColumn}, {localizationDefaultLanguage} FROM {localizedStringsTable} WHERE {localizationTableKeyColumn} LIKE {keyParam}";
        public const string LocalizationMultiReturnFallbackWithEscapeQueryString = $"SELECT  {localizationTableKeyColumn}, {localizationDefaultLanguage} FROM {localizedStringsTable} WHERE {localizationTableKeyColumn} LIKE {keyParam} ESCAPE '\'";

        //Registration sequence Queries
        public const string createTableQueryString = $"SELECT {scriptTableScriptContentColumn} FROM {scriptDataTable} WHERE {scriptTableScriptTypeColumn} = 'create_table' ORDER BY {scriptTableScriptIdColumn} ASC";
        public const string defaultValuesQueryString = $"SELECT {scriptTableScriptContentColumn} FROM {scriptDataTable} WHERE {scriptTableScriptTypeColumn} = 'insert_default_values' ORDER BY {scriptTableScriptIdColumn} ASC";
        public const string insertDefaultCategoriesQueryString = $"INSERT INTO {categoriesDataTable} ({categoriesTableIdColumn},{categoriesTableNameColumn},{categoriesTableTypeColumn}, {categoriesTableParentIdColumn}) VALUES ({idParam},{nameParam},{typeParam},{categoriesTableParentIdParam})";
        public const string insertDefaultCurrenciesQueryString = $"INSERT INTO {currenciesDataTable} ({currenciesTableIdColumn}, {currenciesTableSymbolColumn}, {currenciesTableNameColumn}) VALUES ({idParam},{descriptionParam},{nameParam})";
        public const string createUserQueryString = $"INSERT INTO {appAccountsDataTable} ({appAccountUserNameColumn},{appAccountsTableUserDbColumn},{appAccountsTablePasswordSaltColumn},{appAccountsTableUserKeyColumn}) VALUES ({usernameParam},{userDbParam},{passwordSaltParam},{userKeyParam})";
        public const string insertFieldKeyQueryString = $"INSERT INTO fieldsec (field_key) VALUES({userFieldKeyParam})";
        
        //Login sequence Queries
        public const string userExistQueryString = $"SELECT 1 FROM {appAccountsDataTable} WHERE {appAccountUserNameColumn} = {usernameParam} LIMIT 1";
        public const string findFieldKeyQueryString = $"SELECT field_key FROM fieldsec";

        //First Time Setup Queries.
        public const string GetAvailableLanguagesQueryString = "SELECT iso_code, lang_name FROM languages";
        public const string GetDefaultCurrencyQueryString = $"SELECT {currenciesTableIdColumn}, {currenciesTableNameColumn} FROM {currenciesDataTable} WHERE {currenciesTableIdColumn} = 'USD'";
        public const string GetCurrenciesQueryString = $"SELECT {currenciesTableIdColumn}, {currenciesTableNameColumn} FROM {currenciesDataTable} WHERE {currenciesTableIdColumn} <> 'USD'";
        public const string UpdateUserAccountsCurrencyQueryString = $"UPDATE {userAccountsDataTable} SET {userAccountsCurrencyCodeColumn} = {valueParam}";
        public const string UpdateUserAccountsMainAccountBalanceQueryString = $"UPDATE {userAccountsDataTable} SET {userAccountsBalanceColumn} = {valueParam} WHERE {userAccountsAccountIdColumn} = 1"; //ID 1 should be Main account on first setup

        //Settings Queries
        public const string UpdateSettingQueryString = $"UPDATE {appSettingsDataTable} SET {appSettingsTableValueColumn} = {valueParam} WHERE {appSettingsTableKeyColumn} = {keyParam}";

        //TransactionView Queries
        public const string GetTransactionsFromDbQueryString = $@"SELECT t.{TransactionsTableTransactionIdColumn}, a.{userAccountsAccountNameColumn} AS {userAccountsAccountNameColumn}, t.{TransactionsTableDescriptionColumn}, t.{TransactionsTableTransAmountColumn}, 
                                cur.{currenciesTableIdColumn}, cat.{categoriesTableNameColumn}, t.{TransactionsTableDateColumn}, cat.{categoriesTableIdColumn}, t.{TransactionsTableAccountIdColumn} 
                            FROM {TransactionsDataTable} t JOIN {userAccountsDataTable} a ON t.{TransactionsTableAccountIdColumn} = a.{userAccountsAccountIdColumn} 
                            JOIN {currenciesDataTable} cur ON t.{TransactionsTableCurrencyCodeColumn} = cur.{currenciesTableIdColumn} 
                            JOIN {categoriesDataTable} cat ON t.{TransactionsTableCategoryIdColumn} = cat.{categoriesTableIdColumn} 
                            ORDER BY t.{TransactionsTableTransactionIdColumn} DESC 
                            LIMIT 50";
        public const string GetAccountsFromDbQueryString = $"SELECT {userAccountsAccountIdColumn}, {userAccountsAccountNameColumn}, {userAccountsCurrencyCodeColumn} FROM {userAccountsDataTable}";
        public const string GetCategoriesFromDbQueryStrin = $"SELECT {categoriesTableIdColumn}, {categoriesTableNameColumn}, {categoriesTableTypeColumn}, {categoriesTableParentIdColumn} FROM {categoriesDataTable} WHERE {categoriesTableIdColumn} > 14";
        public const string DeleteTransactionRowFromDbQueryString = $"DELETE FROM {TransactionsDataTable} WHERE {TransactionsTableTransactionIdColumn} = {idParam}";
        public const string UpdateTransactionRowQueryString = $@"UPDATE {TransactionsDataTable} SET {TransactionsTableDescriptionColumn} = {descriptionParam}, {TransactionsTableTransAmountColumn} = {valueParam}, {TransactionsTableCategoryIdColumn} = {categoryParam} 
                            WHERE {TransactionsTableTransactionIdColumn} = {idParam}";
        public const string InsertNewTransactionRowQueryString = $@"INSERT INTO {TransactionsDataTable} ({TransactionsTableAccountIdColumn}, {TransactionsTableDescriptionColumn}, {TransactionsTableTransAmountColumn}, {TransactionsTableCurrencyCodeColumn}, {TransactionsTableCategoryIdColumn}, {TransactionsTableDateColumn}) 
                            VALUES ({idParam},{descriptionParam},{valueParam},{currencyParam},{categoryParam},{dateParam})";
        public const string LookUpOriginalTransactionValueQueryString = $"SELECT {TransactionsTableTransAmountColumn} FROM {TransactionsDataTable} WHERE {TransactionsTableTransactionIdColumn} = {idParam}";
        public const string UpdateAccountBalanceQueryString = $"UPDATE {userAccountsDataTable} SET {userAccountsBalanceColumn} = {userAccountsBalanceColumn} + {valueParam} WHERE {userAccountsAccountIdColumn} = {idParam}";

        //DashboardView Module's Queries
        public const string GetUserAccountDetailsFromDbQueryString =$"SELECT a.account_id, a.account_name, a.balance, c.currency_symbol FROM {userAccountsDataTable} a JOIN {currenciesDataTable} c ON a.currency_code = c.currency_code;";
        public const string GetTopFiveCategoryQueryString = $@"
                            SELECT COALESCE(p.{categoriesTableNameColumn}, c.{categoriesTableNameColumn}) AS category_name,
                                   SUM(t.{TransactionsTableTransAmountColumn}) AS TotalSpent
                            FROM {TransactionsDataTable} t
                            JOIN {categoriesDataTable} c ON t.{categoriesTableIdColumn} = c.{categoriesTableIdColumn}
                            LEFT JOIN {categoriesDataTable} p ON c.{categoriesTableParentIdColumn} = p.{categoriesTableIdColumn}
                            GROUP BY COALESCE(p.{categoriesTableIdColumn}, c.{categoriesTableIdColumn}), COALESCE(p.{categoriesTableNameColumn}, c.{categoriesTableNameColumn})
                            ORDER BY TotalSpent ASC
                            LIMIT 5";


        //Internal SqlHelper queries to support QueryBuilders
        public const string MultiIdReturnByStringKeyHelperQueryString = $"SELECT {localizationTableIdColumn} FROM {localizedStringsTable} WHERE {localizationTableKeyColumn} LIKE {keyParam} ESCAPE '\\'";
        public const string MultiIdReturnByStringKeyWithEscapeHelperQueryString = $"SELECT {localizationTableIdColumn} FROM {localizedStringsTable} WHERE {localizationTableKeyColumn} LIKE {keyParam} ESCAPE '\\'";

        //String Builder Functions
        public static string ErrorMessageQueryStringBuilder(string language, string idOrKeyColumn)
        {
            return $"SELECT {language} FROM {errorMessagesDataTable} WHERE {idOrKeyColumn} = {valueParam}";
        }

        //Localization Querry String builders
        public static string SingleReturnLocalisationQueryStringBuilder(string language)
        {
            return $"SELECT {language} FROM {localizedStringsTable} WHERE {localizationTableKeyColumn} = {keyParam} LIMIT 1";
        }

        public static string MultiReturnLocalisationQueryStringBuilder(string language)
        {
            return $"SELECT {localizationTableKeyColumn}, {language} FROM {localizedStringsTable} WHERE {localizationTableKeyColumn} LIKE {keyParam}";
        }

        public static string MultiReturnLocalisationQueryStringBuilderWithEscape(string language)
        {
            return $"SELECT  {localizationTableKeyColumn}, {language} FROM {localizedStringsTable} WHERE {localizationTableKeyColumn} LIKE {keyParam} ESCAPE '\\'";
        }

        public static string SingleReturnLocalisationByIdQueryStringBuilder(string language)
        {
            return $"SELECT {language} FROM {localizedStringsTable} WHERE {localizationTableIdColumn} = {idParam} LIMIT 1";
        }

        public static string MultiReturnLocalisationByIdQueryStringBuilder(string language)
        {
            return $"SELECT {localizationTableKeyColumn}, {language} FROM {localizedStringsTable} WHERE {localizationTableIdColumn} IN ({idParam})";
        }

        //User data Query String builders
        public static string UserDataSingleColumnReturnQueryBuilder(string column)
        {
            return $"SELECT {column} FROM {appAccountsDataTable} WHERE {appAccountUserNameColumn} = {usernameParam}";
        }


        object NormalizeDbValue(object value)
        {
            if (value == null) return DBNull.Value;
            if (value is string s && string.IsNullOrWhiteSpace(s)) return DBNull.Value;
            return value;
        }
    }
}
