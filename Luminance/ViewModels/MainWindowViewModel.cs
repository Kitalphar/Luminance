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

        public MainWindowViewModel()
        {
            CloseButtonCommand = new RelayCommand(CloseApplication);
            MaximizeWindowCommand = new RelayCommand(MaximizeWindow);
            MinimizeWindowCommand = new RelayCommand(MinimizeWindow);
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

    }
}
