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

        private object? _lastMonthViewModel;
        public object? LastMonthViewModel
        {
            get => _lastMonthViewModel;
            set
            {
                _lastMonthViewModel = value;
                OnPropertyChanged(nameof(LastMonthViewModel));
            }
        }

        private object? _currentMonthViewModel;
        public object? CurrentMonthViewModel
        {
            get => _currentMonthViewModel;
            set
            {
                _currentMonthViewModel = value;
                OnPropertyChanged(nameof(CurrentMonthViewModel));
            }
        }

        public DashboardViewModel()
        {
            AccountsBalanceViewModel = new AccountsBalanceViewModel();
            CurrentMonthViewModel = new TopCategoriesViewModel(CategorySummaryMode.CurrentMonth);
            LastMonthViewModel = new TopCategoriesViewModel(CategorySummaryMode.LastMonth);
        }
    }
}
