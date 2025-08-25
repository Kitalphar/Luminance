using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using Luminance.Helpers;

namespace Luminance.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public ICommand CloseButtonCommand { get; }
        public ICommand MaximizeWindowCommand { get; }
        public ICommand MinimizeWindowCommand { get; }

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

        //Menu button isEnabled tied to this.
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

            string recoveryKey = AppSettings.Instance.Get("recoveryKey");

            if (!string.IsNullOrWhiteSpace(recoveryKey))
            {
                var setupViewModel = new SetupViewModel();
                setupViewModel.SetupCompleted += (s, e) => AfterSetupTasks();
                CurrentViewModel = setupViewModel;
                IsSetupComplete = false;

                //Disable ability to quit App.
            }
            else
            {
                var dashboardViewModel = new DashboardViewModel();
                CurrentViewModel = dashboardViewModel;
                IsSetupComplete = true;
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
                //Enable menu radiobuttons again.
                IsSetupComplete = true;

                //Enable ability to quit App again.

                var dashboardViewModel = new DashboardViewModel();
                CurrentViewModel = dashboardViewModel;
            });
        }

    }
}
