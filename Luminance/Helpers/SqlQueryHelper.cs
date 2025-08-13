using System.Windows.Controls.Primitives;

namespace Luminance.Helpers
{
    internal class SqlQueryHelper
    {
        //Parameters
        public const string usernameParam = "@username";
        public const string passwordParam = "@password";
        public const string passwordSaltParam = "@pwsalt";
        public const string userKeyParam = "@userkey";
        public const string userDbParam = "@userdbname";
        public const string userFieldKeyParam = "@fieldkey";
        public const string columnParam = "@column";

        public const string scriptDataTable = "db_scripts";
        public const string scriptTableScriptContentColumn = "script_content";
        public const string scriptTableScriptIdColumn = "script_id";
        public const string scriptTableScriptTypeColumn = "script_id";

        //Registration sequence Queries
        public const string createTableQueryString = "SELECT script_content FROM db_scripts WHERE type = 'create_table' ORDER BY script_id ASC";
        public const string InsertDefaultValuesQueryString = "SELECT script_content FROM db_scripts WHERE type = 'insert_default_values' ORDER BY script_id ASC";
        public const string createUserQueryString = $"INSERT INTO accounts (user_name,user_db,pw_salt,user_key) VALUES ({usernameParam},{userDbParam},{passwordSaltParam},{userKeyParam})";
        public const string insertFieldKeyQueryString = $"INSERT INTO fieldsec (field_key) VALUES({userFieldKeyParam})";
        

        //Login sequence Queries
        
        public const string userExistQueryString = $"SELECT 1 FROM accounts WHERE user_name = {usernameParam} LIMIT 1";
        public const string findUserDataQueryString = $"SELECT {columnParam} FROM accounts WHERE user_name = {usernameParam}";






        //public string BuildCreateTableLookUpQuery()
        //{
        //    string queryString = "SELECT script_content FROM db_scripts WHERE type = 'create_table' ORDER BY script_id ASC";


        //    return queryString ;
        //}
    }
}
