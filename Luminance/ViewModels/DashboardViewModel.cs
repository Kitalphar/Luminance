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

        private object? _topCategoriesViewModel;
        public object? TopCategoriesViewModel
        {
            get => _topCategoriesViewModel;
            set
            {
                _topCategoriesViewModel = value;
                OnPropertyChanged(nameof(TopCategoriesViewModel));
            }
        }

        public DashboardViewModel()
        {
            AccountsBalanceViewModel = new AccountsBalanceViewModel();
            TopCategoriesViewModel = new TopCategoriesViewModel();
        }
    }
}
