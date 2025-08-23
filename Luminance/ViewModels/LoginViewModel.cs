using System.ComponentModel;
using System.Data;
using System.Windows.Input;
using Luminance.Helpers;
using Luminance.Services;
using Microsoft.Data.Sqlite;

namespace Luminance.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        public ICommand HandleLoginCommand { get; }
        public ICommand HandleRegistrationCommand { get; }

        public event Action? LoginSucceeded;

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

        public string WelcomeMessage { get; set; }
        

        public LoginViewModel()
        {
            HandleLoginCommand = new RelayCommand(HandleLogin);
            HandleRegistrationCommand = new RelayCommand(HandleRegistration);

            string stringKey = "loginwindow_description";
            WelcomeMessage = GetLocalizedString(stringKey);
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
                else
                    LoginWithPassword(sanitizationResult.userName, sanitizationResult.password);
            }
            catch (Exception ex)
            {
                HandleUserDataError(ex.Message);
            }
        }

        private string GetLocalizedString(string stringKey)
        {

            string returnString = String.Empty;
            string language = AppSettings.Instance.Get("language");
            string queryString = SqlQueryHelper.SingleReturnLocalisationQueryStringBuilder(language);

            AppDbQueryCoordinator.RunQuery(conn =>
            {
                using var command = new SqliteCommand(queryString, conn);
                command.Parameters.AddWithValue(SqlQueryHelper.keyParam, stringKey);

                using var reader = command.ExecuteReader();
                if (!reader.Read())
                    throw new DataException("ERR_NO_DATA(502)");

                returnString = reader.GetString(0);
            });

            return returnString;
        }


        private void HandleRegistration()
        {
            try
            {
                var sanitizationResult = SanitizeCredentials(UserName, Password);

                DatabaseCreationService.CreateNewDatabase(sanitizationResult.userName, sanitizationResult.password);

                LoginSucceeded?.Invoke();
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
                IAuthService authService = new AuthService();

                string authenticationResult = authService.LoginWithPassword(userName, password);

                if (EvaluateAuthenticationResult(authenticationResult))
                    LoginSucceeded?.Invoke(); //Switch to MainWindow.
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
                IAuthService authService = new AuthService();

                string authenticationResult = authService.LoginWithPassword(userName, recoveryKey);

                if (EvaluateAuthenticationResult(authenticationResult))
                    LoginSucceeded?.Invoke(); //Switch to MainWindow.
            }
            catch
            {
                throw;
            }
        }

        private bool EvaluateAuthenticationResult(string authenticationResult)
        {
            switch(authenticationResult)
            {
                case "Success":
                    return true;

                case "FieldKeyMissing":
                    throw new UnauthorizedAccessException("ERR_NO_DATA(502)");

                default:
                    throw new InvalidOperationException("ERR_UNKNOWN(801)");
            }
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
