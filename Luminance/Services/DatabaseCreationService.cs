using System.IO;
using Luminance.Helpers;
using Microsoft.Data.Sqlite;

namespace Luminance.Services
{
    public class DatabaseCreationService
    {
        public static void CreateNewDatabase(string userName, string password)
        {

            try
            {
                ICryptoService cryptoService = new CryptoService();

                string userNameHash = cryptoService.HashUserName(userName);

                if (UserExists(userNameHash))
                    throw new InvalidOperationException("ERR_USERNAME_TAKEN(204)");

                string passwordSalt = cryptoService.GeneratePasswordSalt();
                string recoveryKey = cryptoService.GenerateRecoveryKey();

                /*Store recoveryKey as a flag for new user creation and to show it
                  to user later (may be changed later. */
                AppSettings.Instance.Set("recoveryKey", recoveryKey);

                //Derive user Encryption Key from Password.
                string userKey = cryptoService.GenerateUserKey(password, passwordSalt);

                //Encrypt userKey with the RecoveryKey to allow for password recovery.
                string encryptedUserKey = cryptoService.GenerateEncryptedUserKey(userKey, recoveryKey);

                //Storing userKey early for SqlCipher
                AppSettings.Instance.Set("userKey", userKey);

                //Create a per-user Database.
                string dbName = String.Concat(cryptoService.ObfuscateDatabaseName(userName), ".db");
                GenerateDatabase(dbName);

                //Store encrypted user data in App.db
                AppDbQueryCoordinator.RunQuery(conn =>
                {
                    //const string createUser = "INSERT INTO accounts (user_name,user_db,pw_salt,user_key) VALUES (@username,@dbname,@pwsalt,@userkey)";

                    using var command1 = new SqliteCommand();

                    using var command = new SqliteCommand(SqlQueryHelper.createUserQueryString, conn);
                    command.Parameters.AddWithValue(SqlQueryHelper.usernameParam, userNameHash);
                    command.Parameters.AddWithValue(SqlQueryHelper.userDbParam, dbName);
                    command.Parameters.AddWithValue(SqlQueryHelper.passwordSaltParam, passwordSalt);
                    command.Parameters.AddWithValue(SqlQueryHelper.userKeyParam, encryptedUserKey);

                    command.ExecuteNonQuery();
                });

                string fieldKey = cryptoService.GenerateFieldKey();
                string encryptedFieldKey = cryptoService.EncryptFieldKey(fieldKey, userKey);

                //Store encrypted fieldKey
                SecureUserDbQueryCoordinator.RunQuery(conn =>
                {

                    using var command = new SqliteCommand(SqlQueryHelper.insertFieldKeyQueryString, conn);
                    command.Parameters.AddWithValue(SqlQueryHelper.userFieldKeyParam, encryptedFieldKey);

                    command.ExecuteNonQuery();
                });
                
                AppSettings.Instance.Set("fieldKey", fieldKey);
            }
            catch (Exception ex)
            {
                string errorMessage = ErrorHandler.FindErrorMessage(ex.Message);

                ErrorHandler.ShowErrorMessage(errorMessage);
            }
        }

        private static bool UserExists(string userName)
        {
            return AppDbQueryCoordinator.RunQuery(conn =>
            {
                using var command = new SqliteCommand(SqlQueryHelper.userExistQueryString, conn);
                command.Parameters.AddWithValue(SqlQueryHelper.usernameParam, userName);

                using var reader = command.ExecuteReader();
                return reader.Read();
            });
        }

        private static void GenerateDatabase(string dbName)
        {
            try
            {
                string dataFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
                string dbFile = Path.Combine(dataFolder, dbName);

                if (!Directory.Exists(dataFolder))
                    Directory.CreateDirectory(dataFolder);

                if (File.Exists(dbFile))
                    throw new IOException("ERR_IO_DB_FILE_EXISTS(701)");

                //SQLiteConnection.CreateFile(dbFile);

                string userKey = AppSettings.Instance.Get("userKey");
                if (string.IsNullOrEmpty(userKey))
                    throw new InvalidOperationException("ERR_USERKEY_NOT_AVAILABLE(221)");

                //Connect to User's database file & initialize SqlCipher
                //string connectionString = $"Data Source={dbFile};Version=3;";
                string connectionString = $"Data Source={dbFile};";
                UserDatabaseService.Initialize(connectionString);

                CreateDbFileWithSqlCipher(userKey, dbFile);

                //Get CREATE TABLE scripts from App.db
                var createTableQueries = GetDbScriptsFromAppDb(SqlQueryHelper.createTableQueryString);

                //Create default tables in the user's database file.
                ExecuteUserDbScripts(createTableQueries);

                //Create default categories.
                string categoriesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources/csv/categories.csv");
                InsertDeafultValuesFromCsv(categoriesPath, "categories");

                //Create currencies (This has to be before accounts because of foreign key constraints.)
                string currenciesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources/csv/currencies.csv");
                InsertDeafultValuesFromCsv(currenciesPath, "currencies");

                //Look up querystrings and create default accounts
                var createAccountsQueries = GetDbScriptsFromAppDb(SqlQueryHelper.defaultValuesQueryString);

                ExecuteUserDbScripts(createAccountsQueries);
            }
            catch 
            {
                throw;
            }
        }

        private static void CreateDbFileWithSqlCipher(string userKey, string dbFile)
        {

            using var conn = new SqliteConnection($"Data Source={dbFile};Mode=ReadWriteCreate;");
            conn.Open();


            using (var cmd = conn.CreateCommand())
            {
                // Set the key BEFORE any other SQL
                cmd.CommandText = $"PRAGMA key = '{userKey}';";
                cmd.ExecuteNonQuery();
            }

            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "PRAGMA journal_mode=WAL;";
                cmd.ExecuteNonQuery();
            }
        }


        private static List<string> GetDbScriptsFromAppDb(string queryString)
        {
            var queryList = new List<string>();

            AppDbQueryCoordinator.RunQuery(appConn =>
            {
                using var command = new SqliteCommand(queryString, appConn);
                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    queryList.Add(reader.GetString(0));
                }
            });

            return queryList;
        }

        private static void ExecuteUserDbScripts(List<string> queryScripts)
        {
            SecureUserDbQueryCoordinator.RunQuery(userConn =>
            {
                foreach (var queryScript in queryScripts)
                {
                    using var createCmd = new SqliteCommand(queryScript, userConn);
                    createCmd.ExecuteNonQuery();
                }
            });
        }

        private static void InsertDeafultValuesFromCsv(string csvPath, string filter)
        {
            var records = CsvParser.ParseCsv<dynamic>(csvPath);

            SecureUserDbQueryCoordinator.RunTransaction(userConn =>
            {
                using var transaction = userConn.BeginTransaction();

                try
                {
                    foreach (var row in records)
                    {
                        var dict = (IDictionary<string, object>)row;

                        using var cmd = userConn.CreateCommand();

                        switch (filter)
                        {
                            case "categories":
                                cmd.CommandText = SqlQueryHelper.insertDefaultCategoriesQueryString;
                                cmd.Parameters.AddWithValue(SqlQueryHelper.idParam, dict[SqlQueryHelper.categoriesTableIdColumn]);
                                cmd.Parameters.AddWithValue(SqlQueryHelper.nameParam, dict[SqlQueryHelper.categoriesTableENNameColumn]);
                                cmd.Parameters.AddWithValue(SqlQueryHelper.typeParam, dict[SqlQueryHelper.categoriesTableTypeColumn]);

                                //This is because i had Microsoft.Data.Sql is strict about datatypes.
                                var parentIdRaw = dict[SqlQueryHelper.categoriesTableParentIdColumn]?.ToString();
                                cmd.Parameters.AddWithValue(SqlQueryHelper.categoriesTableParentIdParam,string.IsNullOrEmpty(parentIdRaw) ? DBNull.Value : (object)parentIdRaw);
                                //cmd.Parameters.AddWithValue(SqlQueryHelper.categoriesTableParentIdParam, dict[SqlQueryHelper.categoriesTableParentIdColumn]);
                                break;
                            case "currencies":
                                cmd.CommandText = SqlQueryHelper.insertDefaultCurrenciesQueryString;
                                cmd.Parameters.AddWithValue(SqlQueryHelper.idParam, dict[SqlQueryHelper.currenciesTableIdColumn]);
                                cmd.Parameters.AddWithValue(SqlQueryHelper.descriptionParam, dict[SqlQueryHelper.currenciesTableSymbolColumn]);
                                cmd.Parameters.AddWithValue(SqlQueryHelper.nameParam, dict[SqlQueryHelper.currenciesTableNameColumn]);
                                break;
                        }

                        cmd.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
                catch 
                {
                    transaction.Rollback();
                    throw;
                }
            });
        }

        //Use this in the future or put it somewhere else later?
        public static void InsertGenericCsv(string csvPath, string tableName)
        {
            var records = CsvParser.ParseCsv<dynamic>(csvPath);

            if (records.Count == 0)
                return;

            var columnNames = CsvParser.GetColumnNames(records);

            //Validate columns match table schema?

            SecureUserDbQueryCoordinator.RunTransaction(userConn =>
            {
                using var transaction = userConn.BeginTransaction();

                try
                {
                    foreach (var row in records)
                    {
                        var dict = (IDictionary<string, object>)row;

                        string columns = string.Join(", ", columnNames);
                        string placeholders = string.Join(", ", columnNames.Select(col => "@" + col));

                        using var cmd = userConn.CreateCommand();
                        cmd.CommandText = $"INSERT INTO {tableName} ({columns}) VALUES ({placeholders})";

                        foreach (var col in columnNames)
                        {
                            cmd.Parameters.AddWithValue("@" + col, dict.TryGetValue(col, out var val) ? val ?? DBNull.Value : DBNull.Value);
                        }

                        cmd.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            });
        }
    }
}


