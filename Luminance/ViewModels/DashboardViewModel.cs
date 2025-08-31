using System.ComponentModel;

namespace Luminance.ViewModels
{
    class DashboardViewModel : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private object? _accountsBalanceViewModel;
        public object? AccountsBalanceViewModel
        {
            get => _accountsBalanceViewModel;
            set
            {
                _accountsBalanceViewModel = value;
                OnPropertyChanged(nameof(AccountsBalanceViewModel));
            }
        }

        public DashboardViewModel()
        {
            AccountsBalanceViewModel = new AccountsBalanceViewModel();
        }
    }
}
