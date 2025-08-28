using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using Luminance.Helpers;
using Luminance.Views;

namespace Luminance.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public ICommand CloseButtonCommand { get; }
        public ICommand MaximizeWindowCommand { get; }
        public ICommand MinimizeWindowCommand { get; }

        public event PropertyChangedEventHandler? PropertyChanged;

        private Dictionary<string, string> _localizationDictionary = new Dictionary<string, string>();

        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public string DashboardMenuItem => GetLocalizedMenuString("sidebar_dashboard");
        public string TransactionsMenuItem => GetLocalizedMenuString("sidebar_transactions");


        private object? _currentViewModel;
        public object? CurrentViewModel
        {
            get => _currentViewModel;
            set
            {
                _currentViewModel = value;
                OnPropertyChanged(nameof(CurrentViewModel));
            }
        }

        //Menu button & Close Window Button isEnabled tied to this.
        private bool _isSetupComplete;
        public bool IsSetupComplete
        {
            get => _isSetupComplete;
            set
            {
                _isSetupComplete = value;
                OnPropertyChanged(nameof(IsSetupComplete));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public MainWindowViewModel()
        {
            CloseButtonCommand = new RelayCommand(CloseApplication);
            MaximizeWindowCommand = new RelayCommand(MaximizeWindow);
            MinimizeWindowCommand = new RelayCommand(MinimizeWindow);

            //If recoverKey is present, the user just got registered.
            string recoveryKey = AppSettings.Instance.Get("recoveryKey");

            if (!string.IsNullOrWhiteSpace(recoveryKey))
            {
                //Run First Time Setup.
                var setupViewModel = new SetupViewModel();
                setupViewModel.SetupCompleted += (s, e) => AfterSetupTasks();
                CurrentViewModel = new SetupView(setupViewModel);
            }
            else
            {
                //Continue to Dashboard.
                var dashboardViewModel = new DashboardViewModel();
                CurrentViewModel = dashboardViewModel;
                IsSetupComplete = true;
            }

            //If recoveryKey is not present, run Menu localization.
            if (string.IsNullOrWhiteSpace(recoveryKey))
            {
                UpdateLocalisedMenuStrings();
            }
        }

        private void CloseApplication()
        {
            Application.Current.Shutdown();
        }

        private void MaximizeWindow()
        {
            if ( Application.Current.MainWindow.WindowState == WindowState.Normal)
            {
                Application.Current.MainWindow.WindowState = WindowState.Maximized;
            }
            else
            {
                Application.Current.MainWindow.WindowState = WindowState.Normal;
            }
        }

        private void MinimizeWindow()
        {
            Application.Current.MainWindow.WindowState = WindowState.Minimized;
        }

        private void AfterSetupTasks()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                //Enables menu radiobuttons again.
                //Enables ability to quit App again.
                IsSetupComplete = true;

                //Set localised menu titles after settings changed.
                UpdateLocalisedMenuStrings();

                var dashboardViewModel = new DashboardViewModel();
                CurrentViewModel = dashboardViewModel;
            });
        }

        private string GetLocalizedMenuString(string key)
        {
            if (_localizationDictionary.TryGetValue(key, out var localizedValue))
                return localizedValue;
            return $"[{key}]"; //Fallback if missing key
        }

        private Dictionary<string, string> GetLocalisedStringsFromDb()
        {
            string menuItemsStringKey = "sidebar";
            var helper = new LocalizationHelper(menuItemsStringKey + "\\_%", 0, AppSettings.Instance.Get("language"), true);

            Dictionary<string, string> translatedStrings = new Dictionary<string, string>();

            translatedStrings = helper.GetLocalizedStringsDictionary();

            return translatedStrings;
        }

        private void UpdateLocalisedMenuStrings()
        {
            _localizationDictionary = GetLocalisedStringsFromDb();

            OnPropertyChanged(nameof(DashboardMenuItem));
            OnPropertyChanged(nameof(TransactionsMenuItem));
        }

    }
}
