using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using Luminance.Helpers;
using Luminance.Services;
using Microsoft.Data.Sqlite;

namespace Luminance.ViewModels
{
    public class TransactionsViewModel :INotifyPropertyChanged
    {
        public ObservableCollection<TransactionRow>? TransactionsCollection { get; set; }

        public ObservableCollection<Accounts> AccountsCollection { get; } = new();
        public ObservableCollection<Categories> CategoriesCollection { get; } = new();

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<TransactionRow> EditedTransactions { get; } = new();
        public ObservableCollection<TransactionRow> DeletedTransactions { get; } = new();
        public ObservableCollection<NewTransactionRow> NewTransactions { get; } = new();

        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public ICommand UpdateCommand { get; }

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

        private string? _selectedCurrency;
        public string? SelectedCurrency
        {
            get => _selectedCurrency;
            set
            {
                _selectedCurrency = value;
                OnPropertyChanged(nameof(SelectedCurrency));
            }
        }

        private string? _newDescription;
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

        public string ExplanationMessage => GetLocalizedString("transactions_explanation", 20);
        public string WarningMessage => GetLocalizedString("transactions_warning", 21);

        public string UpdateButtonText => GetLocalizedString("update_button", 24);
        public string AccountInputBoxDescription => GetLocalizedString("transactions_input_account_label", 31);
        public string DescriptionInputBoxDescription => GetLocalizedString("transactions_input_description_label", 32);
        public string AmountInputBoxDescription => GetLocalizedString("transactions_input_amount_label", 33);
        public string CategoryInputBoxDescription => GetLocalizedString("transactions_input_category_label", 34);
        public string DateInputBoxDescription => GetLocalizedString("transactions_input_date_label", 35);
        


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
            public required int account_id { get; set; } //For internal logic only
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

        public  TransactionsViewModel()
        {
            TransactionsCollection = new ObservableCollection<TransactionRow>();

            UpdateCommand = new RelayCommand(UpdateChanges);

            InputBalance = 0;
            SelectedDate = DateTime.Now;

            _ = UpdateDataGridAsync();
            _ = GetAccountsAsync();
            _ = GetCategoriesAsync();
        }

        private string GetLocalizedString(string key, int id)
        {
            var helper = new LocalizationHelper(key, id, AppSettings.Instance.Get("language"), false);
            return helper.GetLocalizedString();
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
                            account_id = reader.GetInt32(8),
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
       
        public void UpdateChanges()
        {
            foreach (var row in EditedTransactions)
                row.category_id = ResolveCategoryId(row.category_name);

            bool isUpdated = false;
            decimal balanceToInsert = 0;

            //If user entered a balance, it should be a legitimate entry.
            if (InputBalance != 0)
            {
                //Safeguard against false positive entry
                if (InputBalance > 0)
                {
                    balanceToInsert = InputBalance;

                    // Check the type of the selected category
                    var selectedCategory = CategoriesCollection
                        .FirstOrDefault(c => c.category_id == SelectedCategoryId);

                    if (selectedCategory != null && selectedCategory.type.Equals("expense", StringComparison.OrdinalIgnoreCase))
                    {
                        //If it's an expense, make the number negative
                        balanceToInsert = Math.Abs(balanceToInsert) * -1;
                        //InputBalance = balanceToInsert;
                    }
                }

                //Safeguard against false negative entry
                if (InputBalance < 0)
                {
                    balanceToInsert = InputBalance;

                    // Check the type of the selected category
                    var selectedCategory = CategoriesCollection
                        .FirstOrDefault(c => c.category_id == SelectedCategoryId);

                    if (selectedCategory != null && selectedCategory.type.Equals("income", StringComparison.OrdinalIgnoreCase))
                    {
                        //If it's an income, make the number positive
                        balanceToInsert = Math.Abs(balanceToInsert);
                        //InputBalance = balanceToInsert;
                    }
                }

                //Create New TransactionRow and push to Collection.
                NewTransactionRow row = new NewTransactionRow
                {
                    account_id = SelectedAccountId,
                    description = NewDescription,
                    trans_amount = balanceToInsert,
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
                    if (DeletedTransactions.Count != 0)
                    {
                        foreach (var row in DeletedTransactions)
                        {
                            //Look up original value to determine a difference later.
                            decimal originalValue = 0;

                            using var lookupOriginalCmd = userConn.CreateCommand();
                            lookupOriginalCmd.CommandText = SqlQueryHelper.LookUpOriginalTransactionValueQueryString;
                            lookupOriginalCmd.Parameters.AddWithValue(SqlQueryHelper.idParam, row.transaction_id);
                            using var reader = lookupOriginalCmd.ExecuteReader();

                            if (reader.Read())
                                originalValue = reader.GetDecimal(0);

                            //Delete datatable row
                            using var deleteCmd = userConn.CreateCommand();
                            deleteCmd.CommandText = SqlQueryHelper.DeleteTransactionRowFromDbQueryString;
                            deleteCmd.Parameters.AddWithValue(SqlQueryHelper.idParam, row.transaction_id);
                            deleteCmd.ExecuteNonQuery();

                            //If it's an income, value needs to be subtracted.
                            if (originalValue > 0)
                            {
                                originalValue = Math.Abs(originalValue) * -1;
                            }
                            else //If it's an expense, value needs to be added back.
                            {
                                originalValue = Math.Abs(originalValue);
                            }

                            //Update account's balance value.
                            using var updateBalanceCmd = userConn.CreateCommand();
                            updateBalanceCmd.CommandText = SqlQueryHelper.UpdateAccountBalanceQueryString;
                            updateBalanceCmd.Parameters.AddWithValue(SqlQueryHelper.valueParam, originalValue);
                            updateBalanceCmd.Parameters.AddWithValue(SqlQueryHelper.idParam, row.account_id);

                            updateBalanceCmd.ExecuteNonQuery();
                        }

                        if (!isUpdated) isUpdated = true;

                        DeletedTransactions.Clear();
                    }
                    
                    //Handle edited rows.
                    if (EditedTransactions.Count != 0)
                    {
                        foreach (var row in EditedTransactions)
                        {
                            //Look up original value to determine a difference later.
                            decimal originalValue = 0;
                            decimal newAmount = row.trans_amount;

                            using var lookupOriginalCmd = userConn.CreateCommand();
                            lookupOriginalCmd.CommandText = SqlQueryHelper.LookUpOriginalTransactionValueQueryString;
                            lookupOriginalCmd.Parameters.AddWithValue(SqlQueryHelper.idParam, row.transaction_id);
                            using var reader = lookupOriginalCmd.ExecuteReader();

                            if (reader.Read())
                                originalValue = reader.GetDecimal(0);

                            //Update row in the datatable.
                            using var updateCmd = userConn.CreateCommand();
                            updateCmd.CommandText = SqlQueryHelper.UpdateTransactionRowQueryString;
                            updateCmd.Parameters.AddWithValue(SqlQueryHelper.descriptionParam, row.description ?? "");
                            updateCmd.Parameters.AddWithValue(SqlQueryHelper.valueParam, row.trans_amount);
                            updateCmd.Parameters.AddWithValue(SqlQueryHelper.categoryParam, row.category_id);
                            updateCmd.Parameters.AddWithValue(SqlQueryHelper.idParam, row.transaction_id);

                            updateCmd.ExecuteNonQuery();

                            //Update account's balance value
                            if (originalValue != 0)
                            {
                                decimal updateValue = newAmount - originalValue;

                                using var updateBalanceCmd = userConn.CreateCommand();
                                updateBalanceCmd.CommandText = SqlQueryHelper.UpdateAccountBalanceQueryString;
                                updateBalanceCmd.Parameters.AddWithValue(SqlQueryHelper.valueParam, updateValue);
                                updateBalanceCmd.Parameters.AddWithValue(SqlQueryHelper.idParam, row.account_id);

                                updateBalanceCmd.ExecuteNonQuery();
                            }
                        }

                        if (!isUpdated) isUpdated = true;

                        EditedTransactions.Clear();
                    }

                    //New row entry.
                    if (NewTransactions.Count != 0)
                    {
                        foreach (var row in NewTransactions)
                        {
                            //INSERT row in transactions table.
                            using var insertCmd = userConn.CreateCommand();
                            insertCmd.CommandText = SqlQueryHelper.InsertNewTransactionRowQueryString;
                            insertCmd.Parameters.AddWithValue(SqlQueryHelper.idParam, row.account_id);
                            insertCmd.Parameters.AddWithValue(SqlQueryHelper.descriptionParam, row.description ?? "");
                            insertCmd.Parameters.AddWithValue(SqlQueryHelper.valueParam, row.trans_amount);
                            insertCmd.Parameters.AddWithValue(SqlQueryHelper.currencyParam, row.currency_code);
                            insertCmd.Parameters.AddWithValue(SqlQueryHelper.categoryParam, row.category_id);
                            insertCmd.Parameters.AddWithValue(SqlQueryHelper.dateParam, row.trans_date);

                            insertCmd.ExecuteNonQuery();

                            //UPDATE account's balance value.
                            using var updateBalanceCmd = userConn.CreateCommand();
                            updateBalanceCmd.CommandText = SqlQueryHelper.UpdateAccountBalanceQueryString;
                            updateBalanceCmd.Parameters.AddWithValue(SqlQueryHelper.valueParam, row.trans_amount);
                            updateBalanceCmd.Parameters.AddWithValue(SqlQueryHelper.idParam, row.account_id);

                            updateBalanceCmd.ExecuteNonQuery();
                        }

                        if (!isUpdated) isUpdated = true;

                        NewTransactions.Clear();
                    }

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            });

            if (isUpdated)
            {
                TransactionsCollection.Clear();

                _ = UpdateDataGridAsync();
            }
        }
    }
}
