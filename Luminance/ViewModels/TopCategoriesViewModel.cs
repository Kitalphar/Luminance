using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using Luminance.Helpers;
using Luminance.Services;
using Microsoft.Data.Sqlite;

namespace Luminance.ViewModels
{
    public enum CategorySummaryMode
    {
        CurrentMonth,
        LastMonth
    }

    public class TopCategoriesViewModel : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private Category _firstCategory;
        public Category FirstCategory
        {
            get => _firstCategory;
            set
            {
                _firstCategory = value;
                OnPropertyChanged(nameof(FirstCategory));
            }
        }

        private Category _secondCategory;
        public Category SecondCategory
        {
            get => _secondCategory;
            set
            {
                _secondCategory = value;
                OnPropertyChanged(nameof(SecondCategory));
            }
        }

        private Category _thirdCategory;
        public Category ThirdCategory
        {
            get => _thirdCategory;
            set
            {
                _thirdCategory = value;
                OnPropertyChanged(nameof(ThirdCategory));
            }
        }

        private Category _fourthCategory;
        public Category FourthCategory
        {
            get => _fourthCategory;
            set
            {
                _fourthCategory = value;
                OnPropertyChanged(nameof(FourthCategory));
            }
        }

        private Category _fifthCategory;
        public Category FifthCategory
        {
            get => _fifthCategory;
            set
            {
                _fifthCategory = value;
                OnPropertyChanged(nameof(FifthCategory));
            }
        }

        public class Category
        {
            public required string category_name { get; set; }
            public required string balance { get; set; }
        }

        private Brush _firstCategoryBrush;
        public Brush FirstCategoryBrush
        {
            get => _firstCategoryBrush;
            set
            {
                _firstCategoryBrush = value;
                OnPropertyChanged(nameof(FirstCategoryBrush));
            }
        }

        private Brush _secondCategoryBrush;
        public Brush SecondCategoryBrush
        {
            get => _secondCategoryBrush;
            set
            {
                _secondCategoryBrush = value;
                OnPropertyChanged(nameof(SecondCategoryBrush));
            }
        }

        private Brush _thirdCategoryBrush;
        public Brush ThirdCategoryBrush
        {
            get => _thirdCategoryBrush;
            set
            {
                _thirdCategoryBrush = value;
                OnPropertyChanged(nameof(ThirdCategoryBrush));
            }
        }

        private Brush _fourthCategoryBrush;
        public Brush FourthCategoryBrush
        {
            get => _fourthCategoryBrush;
            set
            {
                _fourthCategoryBrush = value;
                OnPropertyChanged(nameof(FourthCategoryBrush));
            }
        }

        private Brush _fifthCategoryBrush;
        public Brush FifthCategoryBrush
        {
            get => _fifthCategoryBrush;
            set
            {
                _fifthCategoryBrush = value;
                OnPropertyChanged(nameof(FifthCategoryBrush));
            }
        }

        public Brush PositiveBalanceBrush { get; set; } = Application.Current.TryFindResource("PositiveBalanceBrush") as Brush ?? Brushes.Green;
        public Brush NegativeBalanceBrush { get; set; } = (Brush)Application.Current.TryFindResource("NegativeBalanceBrush") as Brush ?? Brushes.Red;


        private string? _viewTitle;
        public string? ViewTitle
        {
            get => _viewTitle;
            set
            {
                _viewTitle = value;
                OnPropertyChanged(nameof(ViewTitle));
            }
        }

        private CategorySummaryMode _selectedSummaryMode;
        public CategorySummaryMode SelectedSummaryMode
        {
            get => _selectedSummaryMode;
            set
            {
                _selectedSummaryMode = value;
                OnPropertyChanged(nameof(SelectedSummaryMode));
            }
        }

        public TopCategoriesViewModel(CategorySummaryMode summaryMode)
        {
            SelectedSummaryMode = summaryMode;

            FirstCategoryBrush = NegativeBalanceBrush;
            SecondCategoryBrush = NegativeBalanceBrush;
            ThirdCategoryBrush = NegativeBalanceBrush;
            FourthCategoryBrush = NegativeBalanceBrush;
            FifthCategoryBrush = NegativeBalanceBrush;

            switch (SelectedSummaryMode)
            {
                case CategorySummaryMode.CurrentMonth:
                    ViewTitle = GetLocalizedString("top_categories_current_month", 36);
                    break;
                case CategorySummaryMode.LastMonth:
                    ViewTitle = GetLocalizedString("top_categories_last_month", 37);
                    break;
            }


            _ = GetTopCategoriesAsync();
        }

        private string GetLocalizedString(string key, int id)
        {
            var helper = new LocalizationHelper(key, id, AppSettings.Instance.Get("language"), false);
            return helper.GetLocalizedString();
        }

        private async Task GetTopCategoriesAsync()
        {
            await Task.Run(() =>
            {
                SecureUserDbQueryCoordinator.RunQuery(userConn =>
                {
                    string queryString = string.Empty;

                    switch (SelectedSummaryMode)
                    {
                        case CategorySummaryMode.CurrentMonth:
                            queryString = SqlQueryHelper.GetTopFiveCategoryFromCurrentMonthQueryString;
                            break;
                        case CategorySummaryMode.LastMonth:
                            queryString = SqlQueryHelper.GetTopFiveCategoryFromLastMonthQueryString;
                            break;
                    }

                    using var command = new SqliteCommand(queryString, userConn);
                    using var reader = command.ExecuteReader();

                    //Modify categories table later to have limits on every major category.
                    decimal categoryLimit = -500m;
                    int count = 1;

                    while (reader.Read())
                    {
                        Category cat = new Category
                        {
                            category_name = reader.GetString(0),
                            balance = reader.GetDecimal(1).ToString("C", CultureInfo.CurrentCulture),
                        };

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            switch (count)
                            {
                                case 1:
                                    FirstCategory = cat;

                                    if (reader.GetDecimal(1) > categoryLimit)
                                        FirstCategoryBrush = PositiveBalanceBrush;
                                    break;
                                case 2:
                                    SecondCategory = cat;

                                    if (reader.GetDecimal(1) > categoryLimit)
                                        SecondCategoryBrush = PositiveBalanceBrush;
                                    break;
                                case 3:
                                    ThirdCategory = cat;

                                    if (reader.GetDecimal(1) > categoryLimit)
                                        ThirdCategoryBrush = PositiveBalanceBrush;
                                    break;
                                case 4:
                                    FourthCategory = cat;

                                    if (reader.GetDecimal(1) > categoryLimit)
                                        FourthCategoryBrush = PositiveBalanceBrush;
                                    break;
                                case 5:
                                    FifthCategory = cat;

                                    if (reader.GetDecimal(1) > categoryLimit)
                                        FifthCategoryBrush = PositiveBalanceBrush;
                                    break;
                            }
                        });
                        count++;
                    }
                });

                OnPropertyChanged(nameof(FirstCategory));
                OnPropertyChanged(nameof(SecondCategory));
                OnPropertyChanged(nameof(ThirdCategory));
                OnPropertyChanged(nameof(FourthCategory));
                OnPropertyChanged(nameof(FifthCategory));
            });
        }
    }
}
