using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using Luminance.Helpers;
using Luminance.Services;
using Microsoft.Data.Sqlite;

namespace Luminance.ViewModels
{
    public class AccountsBalanceViewModel : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private Brush _mainAccountBalanceBrush = Brushes.Black;
        public Brush MainAccountBalanceBrush
        {
            get => _mainAccountBalanceBrush;
            set
            {
                _mainAccountBalanceBrush = value;
                OnPropertyChanged(nameof(MainAccountBalanceBrush));
            }
        }

        private Brush _savingsAccountBalanceBrush = Brushes.Black;
        public Brush SavingsAccountBalanceBrush
        {
            get => _savingsAccountBalanceBrush;
            set
            {
                _savingsAccountBalanceBrush = value;
                OnPropertyChanged(nameof(SavingsAccountBalanceBrush));
            }
        }

        public Brush PositiveBalanceBrush { get; set; } = Application.Current.TryFindResource("PositiveBalanceBrush") as Brush ?? Brushes.Green;
        public Brush NegativeBalanceBrush { get; set; } = (Brush)Application.Current.TryFindResource("NegativeBalanceBrush") as Brush ?? Brushes.Red;

        public Accounts MainAccount { get; private set; }
        public Accounts SavingsAccount { get; private set; }

        public ObservableCollection<Accounts> AccountsCollection { get; } = new();
        public class Accounts
        {
            public required int account_id { get; set; }
            public required string account_name { get; set; }
            public required string balance { get; set; }
            public required string currency_symbol { get; set; }
        }

        public AccountsBalanceViewModel()
        {
            MainAccountBalanceBrush = NegativeBalanceBrush;
            SavingsAccountBalanceBrush = NegativeBalanceBrush;

            _ = GetAccountsAsync();
        }

        private async Task GetAccountsAsync()
        {
            await Task.Run(() =>
            {
                SecureUserDbQueryCoordinator.RunQuery(userConn =>
                {
                    using var command = new SqliteCommand(SqlQueryHelper.GetUserAccountDetailsFromDbQueryString, userConn);
                    using var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        Accounts acc = new Accounts
                        {
                            account_id = reader.GetInt32(0),
                            account_name = reader.GetString(1),
                            balance = reader.GetDecimal(2).ToString("C", CultureInfo.CurrentCulture),
                            currency_symbol = reader.GetString(3)
                        };

                        

                        //This is for later...
                        Application.Current.Dispatcher.Invoke(() => AccountsCollection.Add(acc));

                        if (MainAccount == null && acc.account_id == 1)
                        {
                            MainAccount = acc;

                            if (reader.GetDecimal(2) > 0)
                                MainAccountBalanceBrush = PositiveBalanceBrush;
                        }
                        else if (SavingsAccount == null && acc.account_id == 2)
                        {
                            SavingsAccount = acc;

                            if (reader.GetDecimal(2) > 0)
                                SavingsAccountBalanceBrush = PositiveBalanceBrush;
                        }
                    }
                });

                OnPropertyChanged(nameof(MainAccount));
                OnPropertyChanged(nameof(SavingsAccount));
            });
        }

        ////More testable? Use this in the future maybe?
        //private async Task UpdateAccountsAsync()
        //{
        //    await Task.Yield();

        //    SecureUserDbQueryCoordinator.RunQuery(userConn =>
        //    {
        //        using var command = new SqliteCommand(SqlQueryHelper.GetTransactionsFromDbQueryString, userConn);
        //        using var reader = command.ExecuteReader();

        //        var accounts = new List<Accounts>();

        //        while (reader.Read())
        //        {
        //            accounts.Add(new Accounts
        //            {
        //                account_id = reader.GetInt32(0),
        //                account_name = reader.GetString(1),
        //                balance = reader.GetDecimal(2).ToString("C", CultureInfo.CurrentCulture),
        //                currency_symbol = reader.GetString(3)
        //            });
        //        }

        //        Application.Current.Dispatcher.Invoke(() =>
        //        {
        //            foreach (var acc in accounts)
        //                AccountsCollection.Add(acc);
        //        });
        //    });
        //}

    }
}
