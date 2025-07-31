using System.Data.SQLite;
using System.IO;
using System.Transactions;
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

                var createTableQueries = new List<string>();

                //Get CREATE TABLE scripts from App.db
                AppDbQueryCoordinator.RunQuery(appConn => 
                {
                    using var command = new SQLiteCommand(SqlQueryHelper.createTableQueryString, appConn);
                    using var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        createTableQueries.Add(reader.GetString(0));
                    }
                });

                //Connect to User's database file.
                string connectionString = $"Data Source={dbFile};Version=3;";
                UserDatabaseService.Initialize(connectionString);


                //Create default tables in the user's database file.
                SecureUserDbQueryCoordinator.RunQuery(userConn =>
                {
                    //Enable WAL mode
                    using (var cmd = new SQLiteCommand("PRAGMA journal_mode=WAL;", userConn))
                        cmd.ExecuteNonQuery();

                    foreach (var createTableQuery in createTableQueries)
                    {
                        using var createCmd = new SQLiteCommand(createTableQuery, userConn);
                        createCmd.ExecuteNonQuery();
                    }
                });

                //Fill database with default values.



                InsertDeafultValues(string.Empty);

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private static void InsertDeafultValues(string csvPath)
        {
            //Create default accounts

            //Create default categories.

            //Create currencies

            List<dynamic> results.


            SecureUserDbQueryCoordinator.RunTransaction(userConn =>
            {
                using var transaction = userConn.BeginTransaction();

                try
                {

                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                }
            });
        }
    }
}


