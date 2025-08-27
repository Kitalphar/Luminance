using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Windows;
using System.Windows.Input;
using Luminance.Helpers;
using Luminance.Services;
using Microsoft.Data.Sqlite;

namespace Luminance.ViewModels
{
    public class SetupViewModel : INotifyPropertyChanged
    {
        public event EventHandler? SetupCompleted;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string name) =>
       PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public ObservableCollection<LanguageOption> LanguagesCollection { get; } = new();
        public ObservableCollection<CurrencyOption> CurrenciesCollection { get; } = new();

        public ICommand CopyCommand { get; }
        public ICommand ContinueCommand { get; }

        public string RecoveryKey { get; } = AppSettings.Instance.Get("recoveryKey");
        public string SetupViewTitle => GetLocalizedString("first_setup_title", 12);
        public string SetupViewSubTitle => GetLocalizedString("first_setup_subtitle", 13);
        public string SetupViewRecoveryKeyMessage => GetLocalizedString("recovery_key_message", 14);
        public string SetupViewCheckbox => GetLocalizedString("first_setup_checkbox", 15);

        public string SetupViewBalanceLabel => GetLocalizedString("first_setup_balancelabel", 16);
        public string SetupViewCurrencyLabel => GetLocalizedString("first_setup_currencylabel", 17);
        public string SetupViewLanguageLabel => GetLocalizedString("first_setup_languagelabel", 18);
        public string SetupViewContinueButton => GetLocalizedString("first_setup_continuebutton", 19);


        private bool _hasConfirmed;
        public bool HasConfirmed
        {
            get => _hasConfirmed;
            set
            {
                _hasConfirmed = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasConfirmed)));
            }
        }

        public class LanguageOption
        {
            public string ISO_code { get; set; } = String.Empty;
            public string Name { get; set; } = String.Empty;
        }

        public class CurrencyOption
        {
            public string Currency_code { get; set; } = String.Empty;
            public string CUrrency_Name { get; set; } = String.Empty;
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

        private LanguageOption? _selectedLanguage;
        public LanguageOption? SelectedLanguage
        {
            get => _selectedLanguage;
            set
            {
                _selectedLanguage = value;
                OnPropertyChanged(nameof(SelectedLanguage));
            }
        }

        private CurrencyOption? _selectedCurrency;
        public CurrencyOption? SelectedCurrency
        {
            get => _selectedCurrency;
            set
            {
                _selectedCurrency = value;
                OnPropertyChanged(nameof(SelectedCurrency));
            }
        }

        private decimal _startingBalance;
        public decimal StartingBalance
        {
            get => _startingBalance;
            set
            {
                _startingBalance = value;
                OnPropertyChanged(nameof(StartingBalance));
            }
        }

        public SetupViewModel()
        {
            CopyCommand = new RelayCommand(CopyToClipboard);
            ContinueCommand = new RelayCommand(ContinueToDashboard);
            
            try
            {
                GetLanguagesFromDb();
                GetCurrenciesFromDb();

                StartingBalance = 0;
            }
            catch (Exception ex)
            {
                HandleUserDataError(ex.Message);
            }
        }

        private string GetLocalizedString(string key, int id)
        {
            var helper = new LocalizationHelper(key, id, AppSettings.Instance.Get("language"));
            return helper.GetLocalizedString();
        }

        private void GetLanguagesFromDb()
        {
            AppDbQueryCoordinator.RunQuery(conn =>
            {
                using var command = new SqliteCommand(SqlQueryHelper.GetAvailableLanguagesQueryString, conn);

                using var reader = command.ExecuteReader();

                if (!reader.HasRows)
                    throw new DataException("ERR_NO_DATA(502)");

                while(reader.Read())
                {
                    LanguageOption lang = new()
                    {
                        ISO_code = reader.GetString(0),
                        Name = reader.GetString(1)
                    };

                    LanguagesCollection.Add(lang);
                }
            });

            SelectedLanguage = LanguagesCollection.FirstOrDefault();
        }

        private void GetCurrenciesFromDb()
        {
            string queryString;

            //Set USD as default first.
            queryString = SqlQueryHelper.GetDefaultCurrencyQueryString;

            QueryCurrencies(queryString);

            SelectedCurrency = CurrenciesCollection.FirstOrDefault();

            //Fill the rest.
            queryString = SqlQueryHelper.GetCurrenciesQueryString;

            QueryCurrencies(queryString);
        }

        private void QueryCurrencies(string queryString)
        {
            SecureUserDbQueryCoordinator.RunQuery(userConn =>
            {
                using var command = new SqliteCommand(queryString, userConn);
                using var reader = command.ExecuteReader();

                if (!reader.HasRows)
                    throw new DataException("ERR_NO_DATA(502)");

                while (reader.Read())
                {
                    CurrencyOption currencies = new()
                    {
                        Currency_code = reader.GetString(0),
                        CUrrency_Name = reader.GetString(1)
                    };

                    CurrenciesCollection.Add(currencies);
                }
            });
        }

        private void StartingBalanceDecimalUpDown_ValueChanged(object sender, RoutedPropertyChangedEventArgs<decimal?> e)
        {
            decimal? newValue = e.NewValue;
            if (newValue.HasValue)
                 StartingBalance = newValue.Value;
        }

        private void CopyToClipboard()
        {
            Clipboard.SetText(RecoveryKey);
        }

        private void ContinueToDashboard()
        {
            //Shouldn't get here unless it's true but double checking anyway.
            if (HasConfirmed)
            {
                // Clear the recoveryKey from settings so setup won’t run again
                AppSettings.Instance.Set("recoveryKey", String.Empty);

                //Overwrite language value in settings.
                SaveSettingsChanges("language");

                //Overwrite all default accounts currency setting.
                SaveUserSettingsChanges(SqlQueryHelper.UpdateUserAccountsCurrencyQueryString, false);

                //Overwrite default Main account starting balance.
                SaveUserSettingsChanges(SqlQueryHelper.UpdateUserAccountsMainAccountBalanceQueryString, true);

                // Notify MainWindow that setup is done
                SetupCompleted?.Invoke(this, EventArgs.Empty);
            }
        }
        private void SaveSettingsChanges(string settingKey)
        {
            AppDbQueryCoordinator.RunQuery(conn =>
            {
                using var command = new SqliteCommand(SqlQueryHelper.UpdateSettingQueryString, conn);
                command.Parameters.AddWithValue(SqlQueryHelper.valueParam, SelectedLanguage.ISO_code);
                command.Parameters.AddWithValue(SqlQueryHelper.keyParam, settingKey);
                

                command.ExecuteNonQuery();
            });
        }

        private void SaveUserSettingsChanges(string queryString, bool isBalance)
        {
            SecureUserDbQueryCoordinator.RunQuery(userConn =>
            {
                using var command = new SqliteCommand(queryString, userConn);
                if (isBalance)
                    command.Parameters.AddWithValue(SqlQueryHelper.valueParam, StartingBalance);
                else
                    command.Parameters.AddWithValue(SqlQueryHelper.valueParam, SelectedCurrency.Currency_code);

                command.ExecuteNonQuery();
            });
        }

        //Maybe relocate this into the helper class?
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
