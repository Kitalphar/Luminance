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


        private object _currentViewModel;
        public object CurrentViewModel
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

            CurrentViewModel = new LoginViewModel();
        }

        private void CloseApplication()
        {
            Application.Current.Shutdown();
        }
    }
}
