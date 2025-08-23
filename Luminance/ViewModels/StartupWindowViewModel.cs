using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using Luminance.Helpers;

namespace Luminance.ViewModels
{
    class StartupWindowViewModel : INotifyPropertyChanged
    {
        public ICommand CloseButtonCommand { get; }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));


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

        public StartupWindowViewModel() 
        {
            CloseButtonCommand = new RelayCommand(CloseApplication);

            var loginViewModel = new LoginViewModel();
            loginViewModel.LoginSucceeded += OnLoginSuccess;

            CurrentViewModel = loginViewModel;

            //System.Diagnostics.Debug.WriteLine("CurrentViewModel set: " + (CurrentViewModel != null));
        }

        private void OnLoginSuccess()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose;

                var mainWindow = new MainWindow();
                Application.Current.MainWindow = mainWindow;
                mainWindow.Show();

                //Cloose the StartupWindow
                Application.Current.Windows
                    .OfType<Window>()
                    .FirstOrDefault(w => w is StartupWindow)
                    ?.Close();
            });
        }

        private void CloseApplication()
        {
            Application.Current.Shutdown();
        }
    }
}
