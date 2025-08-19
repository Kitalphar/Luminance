using System.ComponentModel;
using System.Security.Cryptography;
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
                IAuthService authService = new AuthService();

                string authenticationResult = authService.LoginWithPassword(userName, password);

                if (EvaluateAuthenticationResult(authenticationResult))
                {
                    //TODO: Switch to Home View.
                }
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
                {
                    //TODO: Switch to Home View.
                }
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

                case "AuthenticationFailed":
                    throw new UnauthorizedAccessException("Authentication failed. Invalid key or credentials.");

                case "UnknownError":
                    throw new CryptographicException("An unknown error occurred during authentication.");

                default:
                    throw new InvalidOperationException($"Unexpected authentication result: {authenticationResult}");
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
