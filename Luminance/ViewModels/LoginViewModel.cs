using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Windows.Input;
using Luminance.Helpers;
using Luminance.Services;

namespace Luminance.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        public ICommand HandleLoginCommand { get; }
        public ICommand HandleRegistrationCommand { get; }

        private string? _userName;
        public string UserName
        {
            get => _userName!;
            set
            {
                _userName = value;
                OnPropertyChanged(nameof(UserName));
            }
        }

        private string? _password;
        public string Password
        {
            get => _password!;
            set
            {
                _password = value;
                OnPropertyChanged(nameof(Password));
            }
        }

        private string? _errorMessage;
        public string? ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));
            }
        }

        public LoginViewModel()
        {
            HandleLoginCommand = new RelayCommand(HandleLogin);
            HandleRegistrationCommand = new RelayCommand(HandleRegistration);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void HandleLogin()
        {
            try
            {
                var sanitizationResult = SanitizeCredentials(UserName, Password);

                if (sanitizationResult.password.Length > 30)
                    LoginWithRecoveryKey( sanitizationResult.userName, sanitizationResult.password);

                LoginWithPassword(sanitizationResult.userName, sanitizationResult.password);
            }
            catch (Exception ex)
            {
                HandleUserDataError(ex.Message);
            }
        }
        private void HandleRegistration()
        {
            try
            {
                var sanitizationResult = SanitizeCredentials(UserName, Password);

                DatabaseCreationService.CreateNewDatabase(sanitizationResult.userName, sanitizationResult.password);
            }
            catch (Exception ex)
            {
                HandleUserDataError(ex.Message);
            }
        }

        private void LoginWithPassword(string userName, string password)
        {
            try
            {
                var result = FindUserDatabaseName(userName);
                string userNameHash = result.userNameHash;
                string dbName = result.dbName;

                //Encrypt password.
                string userKey = FindUserkey(userNameHash, password, false);

                //Attempt to decrypt the database file, if it fails, throw error.
                //On a success, store encrypted password, (userKey) in settings singleton.
                //Encrypt again.
                AppSettings.Instance.Set("userKey", userKey);

                //Query fieldKey, decrypt it and store it in settings singleton.
                //AppSettings.Instance.Set("fieldKey", fieldKey);

                //Switch to Home View.
            }
            catch
            {
                throw;
            }
        }

        private void LoginWithRecoveryKey(string userName, string recoveryKey)
        {
            try
            {
                var result = FindUserDatabaseName(userName);
                string userNameHash = result.userNameHash;
                string dbName = result.dbName;

                //Query encrypted userKey from App.db, and decrypt it with recoveryKey
                string userKey = FindUserkey(userNameHash,recoveryKey, true);


                //Attempt to decrypt the database file, if it fails, throw error.
                //On a success, store encrypted password, (userKey) in settings singleton.
                //Encrypt again.
                AppSettings.Instance.Set("userKey", userKey);

                //On success, query fieldKey, decrypt it and store it in settings singleton.
                //AppSettings.Instance.Set("fieldKey", fieldKey);
            }
            catch
            {
                throw;
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

        private static (string userNameHash, string dbName) FindUserDatabaseName(string userName)
        {
            ICryptoService cryptoService = new CryptoService();

            string userNameHash = cryptoService.HashUserName(userName);

            if (!UserExists(userNameHash))
                throw new DataException("ERR_USER_NOT_EXISTS(501)");

            string dbColumnName = "user_db";
            string dbName = RunSingleReturnQuery(SqlQueryHelper.findUserDataQueryString, dbColumnName , userNameHash); ;

            return (userNameHash, dbName);
        }

        private static void InitializeDatabaseConnection(string dbName)
        {
            string dataFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            string dbFile = Path.Combine(dataFolder, dbName);

            string connectionString = $"Data Source={dbFile};Version=3;";
            UserDatabaseService.Initialize(connectionString);

            AppSettings.Instance.Set("userDbLocation", dbFile);
            AppSettings.Instance.Set("userDbconnString", connectionString);
        }

        private static string FindUserkey(string userNameHash ,string passwordOrRecoveryKey, bool isRecoveryKey)
        {
            ICryptoService cryptoService = new CryptoService();
            string userKey = string.Empty;

            if(isRecoveryKey)
            {
                string encryptedUserKeyColumn = "user_key";
                string encryptedUserKey = RunSingleReturnQuery(SqlQueryHelper.findUserDataQueryString, encryptedUserKeyColumn, userNameHash);

                userKey = cryptoService.DecryptEncryptedUserKey(encryptedUserKey, passwordOrRecoveryKey);
            }

            string saltColumnName = "pw_salt";
            string salt = RunSingleReturnQuery(SqlQueryHelper.findUserDataQueryString, saltColumnName, userNameHash);

            userKey = cryptoService.GenerateUserKey(passwordOrRecoveryKey, salt);

            return userKey;
        }

        private static string RunSingleReturnQuery(string queryString, string columnValue, string filterValue)
        {
            string returnString = string.Empty;

            AppDbQueryCoordinator.RunQuery(conn =>
            {
                using var command = new SQLiteCommand(queryString, conn);
                command.Parameters.AddWithValue(SqlQueryHelper.columnParam, columnValue);
                command.Parameters.AddWithValue(SqlQueryHelper.usernameParam, filterValue);

                using var reader = command.ExecuteReader();
                if (!reader.Read())
                    throw new DataException("ERR_NO_DATA(502)");

                returnString = reader.GetString(0);
            });

            return returnString;
        }

        private static (string userName, string password) SanitizeCredentials(string userNameInput, string passwordInput)
        {
            var sanitizedUserName = InputSanitizer.SanitizeUsername(userNameInput);

            if (!sanitizedUserName.IsValid)
                throw new ArgumentException(sanitizedUserName.Error);

            var sanitizedPassword = InputSanitizer.SanitizePassword(passwordInput);

            if (!sanitizedPassword.IsValid)
                throw new ArgumentException(sanitizedPassword.Error);

            return (sanitizedUserName.Result, sanitizedPassword.Result);
        }

        private void HandleUserDataError(string errorCodeAndId)
        {
            try 
            {
                ErrorMessage = ErrorHandler.FindErrorMessage(errorCodeAndId);

                ErrorHandler.ShowErrorMessage(ErrorMessage);
            }
            catch (Exception ex)
            {
                ErrorMessage = ErrorHandler.FindErrorMessage(ex.Message);

                ErrorHandler.ShowErrorMessage(ErrorMessage);
            }
        }
    }
}
