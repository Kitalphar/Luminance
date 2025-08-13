using System.Data.SQLite;
using System.IO;
using System.Windows;
using Luminance.Helpers;

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

                //Derive user Encryption Key from Password.
                string userKey = cryptoService.GenerateUserKey(password, passwordSalt);

                //Encrypt userKey with the RecoveryKey to allow for password recovery.
                string encryptedUserKey = cryptoService.GenerateEncryptedUserKey(userKey, recoveryKey);

                //Create a per-user Database.
                string dbName = String.Concat(cryptoService.ObfuscateDatabaseName(userName), ".db");
                GenerateDatabase(dbName);

                //Store encrypted user data in App.db
                AppDbQueryCoordinator.RunQuery(conn =>
                {
                    //const string createUser = "INSERT INTO accounts (user_name,user_db,pw_salt,user_key) VALUES (@username,@dbname,@pwsalt,@userkey)";

                    using var command1 = new SQLiteCommand();

                    using var command = new SQLiteCommand(SqlQueryHelper.createUserQueryString, conn);
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
                    //const string insertFieldKey = "INSERT INTO fieldsec (field_key) VALUES(@fieldkey)";

                    using var command = new SQLiteCommand(SqlQueryHelper.insertFieldKeyQueryString, conn);
                    command.Parameters.AddWithValue(SqlQueryHelper.userFieldKeyParam, encryptedFieldKey);

                    command.ExecuteNonQuery();
                });

                AppSettings.Instance.Set("userKey", userKey);
                AppSettings.Instance.Set("fieldKey", fieldKey);

                // TEMPORARY MESSAGEBOX, REMOVE LATER.
                MessageBoxResult recoveryMessageBox = MessageBox.Show(
                recoveryKey,
                "Recovery Key",
                MessageBoxButton.OK,
                MessageBoxImage.Warning
                );

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
                using var command = new SQLiteCommand(SqlQueryHelper.userExistQueryString, conn);
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

                SQLiteConnection.CreateFile(dbFile);

                //Get CREATE TABLE scripts from App.db
                var createTableQueries = GetDbScriptsFromAppDb(SqlQueryHelper.createTableQueryString);

                //Connect to User's database file.
                string connectionString = $"Data Source={dbFile};Version=3;";
                UserDatabaseService.Initialize(connectionString);

                //Enable WAL mode for future queries.
                SecureUserDbQueryCoordinator.RunQuery(userConn => 
                {
                    using (var cmd = new SQLiteCommand("PRAGMA journal_mode=WAL;", userConn))
                        cmd.ExecuteNonQuery();
                });

                //Create default tables in the user's database file.
                ExecuteUserDbScripts(createTableQueries);

                //Create default categories.
                string categoriesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources/csv/categories.csv");
                InsertDeafultValuesFromCsv(categoriesPath);

                //Create currencies (This has to be before accounts because of foreign key constraints.)
                string currenciesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources/csv/currencies.csv");
                InsertDeafultValuesFromCsv(currenciesPath);

                //Look up querystrings and create default accounts
                var createAccountsQueries = GetDbScriptsFromAppDb(SqlQueryHelper.InsertDefaultValuesQueryString);

                ExecuteUserDbScripts(createAccountsQueries);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private static List<string> GetDbScriptsFromAppDb(string queryString)
        {
            var queryList = new List<string>();

            AppDbQueryCoordinator.RunQuery(appConn =>
            {
                using var command = new SQLiteCommand(queryString, appConn);
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
                    using var createCmd = new SQLiteCommand(queryScript, userConn);
                    createCmd.ExecuteNonQuery();
                }
            });
        }

        private static void InsertDeafultValuesFromCsv(string csvPath)
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

                        // Example for inserting into a Categories table
                        cmd.CommandText = "INSERT INTO Categories (Id, Name) VALUES (@Id, @Name)";
                        cmd.Parameters.AddWithValue("@Id", dict["Id"]);
                        cmd.Parameters.AddWithValue("@Name", dict["Name"]);

                        cmd.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
                catch (Exception ex)
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


