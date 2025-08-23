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
        public const string languageParam = "@lang";
        public const string descriptionParam = "@description";

        //App.db accounts table parameters
        private const string appAccountsDataTable = "accounts";


        //App.db db_scripts table parameters
        private const string scriptDataTable = "db_scripts";
        private const string scriptTableScriptContentColumn = "script_content";
        private const string scriptTableScriptIdColumn = "script_id";
        private const string scriptTableScriptTypeColumn = "type";

        //User.db categories table parameters
        public const string categoriesDataTable = "categories";
        public const string categoriesTableIdColumn = "category_id";
        public const string categoriesTableENNameColumn = "category_name_en";
        public const string categoriesTableTypeColumn = "type";
        public const string categoriesTableParentIdColumn = "parent_category_id";
        public const string categoriesTableParentIdParam = "@parentid";

        //User.db currencies table parameters.
        public const string currenciesDataTable = "currencies";
        public const string currenciesTableIdColumn = "currency_code";
        public const string currenciesTableSymbolColumn = "currency_symbol";
        public const string currenciesTableNameColumn = "currency_name";

        //User.db error_message table parameters.
        public const string errorMessagesDataTable = "error_messages";
        public const string errorTableIdColumn = "id";
        public const string errorTableErrorCodeColumn = "error_code";

        //Registration sequence Queries
        public const string createTableQueryString = $"SELECT {scriptTableScriptContentColumn} FROM {scriptDataTable} WHERE {scriptTableScriptTypeColumn} = 'create_table' ORDER BY {scriptTableScriptIdColumn} ASC";
        public const string defaultValuesQueryString = $"SELECT {scriptTableScriptContentColumn} FROM {scriptDataTable} WHERE {scriptTableScriptTypeColumn} = 'insert_default_values' ORDER BY {scriptTableScriptIdColumn} ASC";
        public const string insertDefaultCategoriesQueryString = $"INSERT INTO {categoriesDataTable} ({categoriesTableIdColumn},{categoriesTableENNameColumn},{categoriesTableTypeColumn}, {categoriesTableParentIdColumn}) VALUES ({idParam},{nameParam},{typeParam},{categoriesTableParentIdParam})";
        public const string insertDefaultCurrenciesQueryString = $"INSERT INTO {currenciesDataTable} ({currenciesTableIdColumn}, {currenciesTableSymbolColumn}, {currenciesTableNameColumn}) VALUES ({idParam},{descriptionParam},{nameParam})";
        public const string createUserQueryString = $"INSERT INTO accounts (user_name,user_db,pw_salt,user_key) VALUES ({usernameParam},{userDbParam},{passwordSaltParam},{userKeyParam})";
        public const string insertFieldKeyQueryString = $"INSERT INTO fieldsec (field_key) VALUES({userFieldKeyParam})";
        
        //Login sequence Queries
        public const string userExistQueryString = $"SELECT 1 FROM accounts WHERE user_name = {usernameParam} LIMIT 1";
        public const string findFieldKeyQueryString = $"SELECT field_key FROM fieldsec";

        //Error Message Query

        //String Builder Functions
        public static string ErrorMessageQueryStringBuilder(string language, string idOrKeyColumn)
        {
            return $"SELECT {language} FROM {errorMessagesDataTable} WHERE {idOrKeyColumn} = {valueParam}";
        }

        public static string SingleReturnLocalisationQueryStringBuilder(string language)
        {
            return $"SELECT {language} FROM localized_strings WHERE key = {keyParam} LIMIT 1";
        }

        public static string UserDataSingleColumnReturnQueryBuilder(string column)
        {
            return $"SELECT {column} FROM {appAccountsDataTable} WHERE user_name = {usernameParam}";
        }

        object NormalizeDbValue(object value)
        {
            if (value == null) return DBNull.Value;
            if (value is string s && string.IsNullOrWhiteSpace(s)) return DBNull.Value;
            return value;
        }
    }
}
