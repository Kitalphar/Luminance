using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using Luminance.Helpers;

namespace Luminance.ViewModels
{
    public class SetupViewModel : INotifyPropertyChanged
    {
        public event EventHandler? SetupCompleted;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string name) =>
       PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public ObservableCollection<LanguageOption> Languages { get; } = new();
        public ObservableCollection<CurrencyOption> Currencies { get; } = new();

        public ICommand CopyCommand { get; }
        public ICommand ContinueCommand { get; }

        public string RecoveryKey { get; } = AppSettings.Instance.Get("recoveryKey");

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
            public int Id { get; set; }
            public string Name { get; set; } = "";
        }

        public class CurrencyOption
        {
            public int Id { get; set; }
            public string Code { get; set; } = "";
            public string Name { get; set; } = "";
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
        }

        private void CopyToClipboard()
        {
            Clipboard.SetText(RecoveryKey);
        }

        private void ContinueToDashboard()
        {
            HasConfirmed = true;

            SaveSettingsChanges();
        }
        private void SaveSettingsChanges()
        {
            // Clear the recoveryKey from settings so setup won’t run again
            AppSettings.Instance.Set("recoveryKey", String.Empty);

            // Notify whoever’s listening that setup is done
            SetupCompleted?.Invoke(this, EventArgs.Empty);
        }

        private void LoadFromDatabase()
        {
            //// Load defaults (from settings table?
            //SelectedLanguage = Languages.FirstOrDefault(l => l.Id == 1); // Example
            //SelectedCurrency = Currencies.FirstOrDefault(c => c.Code == "USD");
            //StartingBalance = 100;
        }
    }
}
