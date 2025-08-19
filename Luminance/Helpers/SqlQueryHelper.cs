using System.Windows.Controls.Primitives;

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
        public const string columnParam = "@column";
        public const string idParam = "@id";
        public const string nameParam = "@name";
        public const string typeParam = "@type";
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
        public const string currenciesTableIdcolumn = "currency_code";
        public const string currenciesTableSymbolcolumn = "currency_symbol";
        public const string currenciesTableNamecolumn = "currency_name";

        //Registration sequence Queries
        public const string createTableQueryString = $"SELECT {scriptTableScriptContentColumn} FROM {scriptDataTable} WHERE {scriptTableScriptTypeColumn} = 'create_table' ORDER BY {scriptTableScriptIdColumn} ASC";
        public const string defaultValuesQueryString = $"SELECT {scriptTableScriptContentColumn} FROM {scriptDataTable} WHERE {scriptTableScriptTypeColumn} = 'insert_default_values' ORDER BY {scriptTableScriptIdColumn} ASC";
        public const string insertDefaultCategoriesQueryString = $"INSERT INTO {categoriesDataTable} ({categoriesTableIdColumn},{categoriesTableENNameColumn},{categoriesTableTypeColumn}, {categoriesTableParentIdColumn}) VALUES ({idParam},{nameParam},{typeParam},{categoriesTableParentIdParam})";
        public const string insertDefaultCurrenciesQueryString = $"INSERT INTO {currenciesDataTable} ({currenciesTableIdcolumn}, {currenciesTableSymbolcolumn}, {currenciesTableNamecolumn}) VALUES ({idParam},{descriptionParam},{nameParam})";
        public const string createUserQueryString = $"INSERT INTO accounts (user_name,user_db,pw_salt,user_key) VALUES ({usernameParam},{userDbParam},{passwordSaltParam},{userKeyParam})";
        public const string insertFieldKeyQueryString = $"INSERT INTO fieldsec (field_key) VALUES({userFieldKeyParam})";
        

        //Login sequence Queries
        public const string userExistQueryString = $"SELECT 1 FROM accounts WHERE user_name = {usernameParam} LIMIT 1";
        public const string findFieldKeyQueryString = $"SELECT field_key FROM fieldsec";


        //String Builder Functions

        public static string UserDataSingleColumnReturnQueryBuilder(string column)
        {
            return $"SELECT {column} FROM {appAccountsDataTable} WHERE user_name = {usernameParam}";
        }


        //public string BuildCreateTableLookUpQuery()
        //{
        //    string queryString = "SELECT script_content FROM db_scripts WHERE type = 'create_table' ORDER BY script_id ASC";


        //    return queryString ;
        //}
    }
}
