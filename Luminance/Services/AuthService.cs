using System.Data;
using Microsoft.Data.Sqlite;
using System.IO;
using Luminance.Helpers;

namespace Luminance.Services
{
    internal class AuthService : IAuthService
    {
        private enum AuthenticationStatus
        {
            Success,
            AuthenticationFailed,
            UnknownError
        }

        public string LoginWithPassword(string userName, string password)
        {
            var result = FindUserDatabaseName(userName);
            string userNameHash = result.userNameHash;
            string dbName = result.dbName;

            //Encrypt password.
            string userKey = FindUserkey(userNameHash, password, false);

            //Storing userKey early for SqlCipher
            AppSettings.Instance.Set("userKey", userKey);

            InitializeDatabaseConnection(dbName);

            //Query fieldKey. If decryption on the database file fails, the
            //Authentication fails.
            string encryptedFieldKey = RunFieldKeyQuery();

            //decrypt fieldkey
            string fieldKey = ReturnDecryptedFieldKey(encryptedFieldKey, userKey);

            AppSettings.Instance.Set("fieldKey", fieldKey);

            return AuthenticationStatus.Success.ToString();
        }

        public string LoginWithRecoveryKey(string userName, string recoveryKey)
        {
            var result = FindUserDatabaseName(userName);
            string userNameHash = result.userNameHash;
            string dbName = result.dbName;

            //Query encrypted userKey from App.db, and decrypt it with recoveryKey
            string userKey = FindUserkey(userNameHash, recoveryKey, true);

            //Storing userkey early for SqlCipher
            AppSettings.Instance.Set("userKey", userKey);

            InitializeDatabaseConnection(dbName);

            //Query fieldKey. If decryption on the database file fails, the
            //Authentication fails.
            string encryptedFieldKey = RunFieldKeyQuery();

            //decrypt fieldkey
            string fieldKey = ReturnDecryptedFieldKey(encryptedFieldKey, userKey);

            AppSettings.Instance.Set("fieldKey", fieldKey);
            
            return AuthenticationStatus.Success.ToString();
        }

        private static void InitializeDatabaseConnection(string dbName)
        {
            string dataFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            string dbFile = Path.Combine(dataFolder, dbName);

            string connectionString = $"Data Source={dbFile};";
            UserDatabaseService.Initialize(connectionString);

            AppSettings.Instance.Set("userDbLocation", dbFile);
            AppSettings.Instance.Set("userDbconnString", connectionString);
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

        private static (string userNameHash, string dbName) FindUserDatabaseName(string userName)
        {
            ICryptoService cryptoService = new CryptoService();

            string userNameHash = cryptoService.HashUserName(userName);

            if (!UserExists(userNameHash))
                throw new DataException("ERR_USER_NOT_EXISTS(501)");

            string dbColumnName = "user_db";
            string dbName = RunUserDataSingleReturnQuery(dbColumnName, userNameHash);

            return (userNameHash, dbName);
        }

        private static string FindUserkey(string userNameHash, string passwordOrRecoveryKey, bool isRecoveryKey)
        {
            ICryptoService cryptoService = new CryptoService();
            string userKey = string.Empty;

            if (isRecoveryKey)
            {
                string encryptedUserKeyColumn = "user_key";
                string encryptedUserKey = RunUserDataSingleReturnQuery(encryptedUserKeyColumn, userNameHash);

                userKey = cryptoService.DecryptEncryptedUserKey(encryptedUserKey, passwordOrRecoveryKey);
            }

            string saltColumnName = "pw_salt";
            string salt = RunUserDataSingleReturnQuery(saltColumnName, userNameHash);

            userKey = cryptoService.GenerateUserKey(passwordOrRecoveryKey, salt);

            return userKey;
        }

        private static string RunUserDataSingleReturnQuery(string columnValue, string filterValue)
        {
            string returnString = string.Empty;

            string queryString = SqlQueryHelper.UserDataSingleColumnReturnQueryBuilder(columnValue);

            AppDbQueryCoordinator.RunQuery(conn =>
            {
                using var command = new SqliteCommand(queryString, conn);
                command.Parameters.AddWithValue(SqlQueryHelper.usernameParam, filterValue);

                using var reader = command.ExecuteReader();
                if (!reader.Read())
                    throw new DataException("ERR_NO_DATA(502)");

                returnString = reader.GetString(0);
            });

            return returnString;
        }

        private static string RunFieldKeyQuery()
        {
            string encryptedFieldKey = string.Empty;

            SecureUserDbQueryCoordinator.RunQuery(conn =>
            {
                using var command = new SqliteCommand(SqlQueryHelper.findFieldKeyQueryString, conn);
                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    encryptedFieldKey = reader.GetString(0);
                }
            });

            return encryptedFieldKey;
        }

        private static string ReturnDecryptedFieldKey(string encryptedFieldKey, string userKey)
        {
            ICryptoService cryptoService = new CryptoService();

            return cryptoService.DecryptFieldKey(encryptedFieldKey, userKey);
        }
    }
}
