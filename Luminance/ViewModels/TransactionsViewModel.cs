using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Windows;
using System.Windows.Input;
using Luminance.Helpers;
using Luminance.Services;
using Microsoft.Data.Sqlite;

namespace Luminance.ViewModels
{
    public class TransactionsViewModel :INotifyPropertyChanged
    {
        //TODO: Create Observable Collection for data read from Database.

        //TODO: Create Lists for edited, deleted and new rows.

        public ObservableCollection<TransactionRow>? TransactionsCollection { get; set; }

        public ObservableCollection<Accounts> AccountsCollection { get; } = new();
        public ObservableCollection<Categories> CategoriesCollection { get; } = new();

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<TransactionRow> EditedTransactions { get; } = new();
        public List<int> DeletedTransactionIds { get; } = new();
        public ObservableCollection<NewTransactionRow> NewTransactions { get; } = new();

        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public ICommand UploadCommand { get; }

        public class Accounts
        {
            public int account_id { get; set; }
            public string account_name { get; set; } = String.Empty;
            public string currency_code { get; set; } = String.Empty;
        }

        public class Categories
        {
            public int category_id { get; set; }
            public string category_name { get; set; } = String.Empty;
            public string type { get; set; } = String.Empty;
            public int parent_category_id { get; set; }
        }

        public int SelectedAccountId { get; private set; }

        private Accounts? _selectedAccount;
        public Accounts? SelectedAccount
        {
            get => _selectedAccount;
            set
            {
                _selectedAccount = value;
                OnPropertyChanged(nameof(SelectedAccount));

                if (value != null)
                {
                    SelectedCurrency = value.currency_code; //Keep currency in sync
                    SelectedAccountId = value.account_id;   //Store id for DB inserts
                }
            }
        }

        private int _selectedCategoryId;
        public int SelectedCategoryId
        {
            get => _selectedCategoryId;
            set
            {
                _selectedCategoryId = value;
                OnPropertyChanged(nameof(SelectedCategoryId));
            }
        }

        private string _selectedCurrency;
        public string SelectedCurrency
        {
            get => _selectedCurrency;
            set
            {
                _selectedCurrency = value;
                OnPropertyChanged(nameof(SelectedCurrency));
            }
        }

        private string _newDescription;
        public string? NewDescription
        {
            get => _newDescription;
            set
            {
                _newDescription = value;
                OnPropertyChanged(nameof(NewDescription));
            }
        }

        private decimal _inputBalance;
        public decimal InputBalance
        {
            get => _inputBalance;
            set
            {
                _inputBalance = value;
                OnPropertyChanged(nameof(InputBalance));
            }
        }
        
        private DateTime? _selectedDate;
        public DateTime? SelectedDate
        {
            get => _selectedDate;
            set
            {
                // if null or in future, use Now
                if (!value.HasValue || value > DateTime.Now)
                    _selectedDate = DateTime.Now;
                else
                    _selectedDate = value;

                OnPropertyChanged(nameof(SelectedDate));
                OnPropertyChanged(nameof(SelectedDateIso8601));
            }
        }

        public string SelectedDateIso8601 =>
            (_selectedDate ?? DateTime.Now).ToString("yyyy-MM-ddTHH:mm:ss");

        public  TransactionsViewModel()
        {
            TransactionsCollection = new ObservableCollection<TransactionRow>();

            UploadCommand = new RelayCommand(UploadChanges);

            InputBalance = 0;
            SelectedDate = DateTime.Now;

            _ = UpdateDataGridAsync();
            _ = GetAccountsAsync();
            _ = GetCategoriesAsync();
        }

        public class TransactionRow
        {
            public required int transaction_id { get; set; }
            public required string account_name { get; set; }
            public string? description { get; set; }
            public required decimal trans_amount { get; set; }
            public required string currency_code { get; set; }
            public required string category_name { get; set; }
            public required DateTime trans_date { get; set; }
            public required int category_id { get; set; } //For internal logic only
        }

        public class NewTransactionRow
        {
            public required int account_id { get; set; }
            public string? description { get; set; }
            public required decimal trans_amount { get; set; }
            public required string currency_code { get; set; }
            public required int category_id { get; set; }
            public required string trans_date { get; set; }
        }

        private async Task UpdateDataGridAsync()
        {
            await Task.Run(() => 
            {
                SecureUserDbQueryCoordinator.RunQuery(userConn =>
                {
                    using var command = new SqliteCommand(SqlQueryHelper.GetTransactionsFromDbQueryString, userConn);
                    using var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        TransactionRow row = new TransactionRow
                        {
                            transaction_id = reader.GetInt32(0),
                            account_name = reader.GetString(1),
                            description = reader.GetString(2),
                            trans_amount = InvariantCultureHelper.ParseDecimal(reader.GetString(3)),
                            currency_code = reader.GetString(4),
                            category_name = reader.GetString(5),
                            trans_date = reader.GetDateTime(6),
                            category_id = reader.GetInt32(7),
                        };

                        Application.Current.Dispatcher.Invoke(() => TransactionsCollection.Add(row));
                    }
                });
            });
        }
        
        private async Task GetAccountsAsync()
        {
            await Task.Run(() =>
            {
                SecureUserDbQueryCoordinator.RunQuery(userConn =>
                {
                    using var command = new SqliteCommand(SqlQueryHelper.GetAccountsFromDbQueryString, userConn);
                    using var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        Accounts row = new Accounts
                        {
                            account_id = reader.GetInt32(0),
                            account_name = reader.GetString(1),
                            currency_code = reader.GetString(2)
                        };

                        Application.Current.Dispatcher.Invoke(() => AccountsCollection.Add(row));
                    }
                });
                //Set initial Currency based on the default account.
                SelectedAccount = AccountsCollection.FirstOrDefault();
            });
        }

        private async Task GetCategoriesAsync()
        {
            await Task.Run(() =>
            {
                SecureUserDbQueryCoordinator.RunQuery(userConn =>
                {
                    using var command = new SqliteCommand(SqlQueryHelper.GetCategoriesFromDbQueryStrin, userConn);
                    using var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        Categories row = new Categories
                        {
                            category_id = reader.GetInt32(0),
                            category_name = reader.GetString(1),
                            type = reader.GetString(2),
                            parent_category_id = reader.GetInt32(3)
                        };

                        Application.Current.Dispatcher.Invoke(() => CategoriesCollection.Add(row));
                    }
                });
                //Set initial Currency based on the default account.
                SelectedCategoryId = CategoriesCollection.FirstOrDefault().category_id;
            });
        }

        private int ResolveCategoryId(string? userInput)
        {
            if (string.IsNullOrWhiteSpace(userInput))
                return GetMiscCategoryId();

            var match = CategoriesCollection
                .FirstOrDefault(c =>
                    c.category_name.Equals(userInput.Trim(), StringComparison.OrdinalIgnoreCase));

            return match?.category_id ?? GetMiscCategoryId();
        }

        private int GetMiscCategoryId()
        {
            var misc = CategoriesCollection
                .FirstOrDefault(c => c.category_name.Equals("Miscellaneous", StringComparison.OrdinalIgnoreCase));

            return misc?.category_id ?? -1; //If -1 is returned, treat as error
        }

        public void UploadChanges()
        {
            foreach (var row in EditedTransactions)
                row.category_id = ResolveCategoryId(row.category_name);

            

            //If user entered a balance, it should be a legitimate entry.
            if (InputBalance != 0)
            {
                //Safeguard against false positive entry
                if (InputBalance > 0)
                {
                    decimal balanceToInsert = InputBalance;

                    // Check the type of the selected category
                    var selectedCategory = CategoriesCollection
                        .FirstOrDefault(c => c.category_id == SelectedCategoryId);

                    if (selectedCategory != null && selectedCategory.type.Equals("expense", StringComparison.OrdinalIgnoreCase))
                    {
                        //If it's an expense, make the number negative
                        balanceToInsert = Math.Abs(balanceToInsert) * -1;
                        InputBalance = balanceToInsert;
                    }
                }

                //Safeguard against false negative entry
                if (InputBalance < 0)
                {
                    decimal balanceToInsert = InputBalance;

                    // Check the type of the selected category
                    var selectedCategory = CategoriesCollection
                        .FirstOrDefault(c => c.category_id == SelectedCategoryId);

                    if (selectedCategory != null && selectedCategory.type.Equals("income", StringComparison.OrdinalIgnoreCase))
                    {
                        //If it's an income, make the number positive
                        balanceToInsert = Math.Abs(balanceToInsert);
                        InputBalance = balanceToInsert;
                    }
                }

                //Create New TransactionRow and push to Collection.
                NewTransactionRow row = new NewTransactionRow
                {
                    account_id = SelectedAccountId,
                    description = NewDescription,
                    trans_amount = InputBalance,
                    currency_code = SelectedCurrency,
                    category_id = SelectedCategoryId,
                    trans_date = SelectedDateIso8601
                };

                NewTransactions.Add(row);
            }

            SecureUserDbQueryCoordinator.RunQuery(userConn =>
            {
                using var transaction = userConn.BeginTransaction();

                try
                {
                    //Handle deleted rows
                    foreach (var id in DeletedTransactionIds)
                    {
                        using var deleteCmd = userConn.CreateCommand();
                        deleteCmd.CommandText = SqlQueryHelper.DeleteTransactionRowFromDbQueryString;
                        deleteCmd.Parameters.AddWithValue(SqlQueryHelper.idParam, id);
                        deleteCmd.ExecuteNonQuery();
                    }
                    DeletedTransactionIds.Clear();

                    //Handle edited rows.
                    foreach (var row in EditedTransactions)
                    {
                        using var updateCmd = userConn.CreateCommand();
                        updateCmd.CommandText = SqlQueryHelper.UpdateTransactionRowQueryString;
                        updateCmd.Parameters.AddWithValue(SqlQueryHelper.descriptionParam, row.description ?? "");
                        updateCmd.Parameters.AddWithValue(SqlQueryHelper.valueParam, row.trans_amount);
                        updateCmd.Parameters.AddWithValue(SqlQueryHelper.categoryParam, row.category_id);
                        updateCmd.Parameters.AddWithValue(SqlQueryHelper.idParam, row.transaction_id);
                        
                        updateCmd.ExecuteNonQuery();
                    }
                    EditedTransactions.Clear();


                    //New row entry.
                    foreach (var row in NewTransactions)
                    {
                        using var insertCmd = userConn.CreateCommand();
                        insertCmd.CommandText = SqlQueryHelper.InsertNewTransactionRowQueryString;
                        insertCmd.Parameters.AddWithValue(SqlQueryHelper.idParam, row.account_id);
                        insertCmd.Parameters.AddWithValue(SqlQueryHelper.descriptionParam, row.description ?? "");
                        insertCmd.Parameters.AddWithValue(SqlQueryHelper.valueParam, row.trans_amount);
                        insertCmd.Parameters.AddWithValue(SqlQueryHelper.currencyParam, row.currency_code);
                        insertCmd.Parameters.AddWithValue(SqlQueryHelper.categoryParam, row.category_id);
                        insertCmd.Parameters.AddWithValue(SqlQueryHelper.dateParam, row.trans_date);

                        insertCmd.ExecuteNonQuery();
                    }
                    NewTransactions.Clear();

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            });

            TransactionsCollection.Clear();

            _ = UpdateDataGridAsync();
        }
    }
}
