using System.Windows.Controls;
using System.Windows.Input;
using Luminance.ViewModels;

namespace Luminance.Views
{
    public partial class TransactionsView : UserControl
    {
        public TransactionsView()
        {
            InitializeComponent();
        }

        private void DataGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.Row.Item is TransactionsViewModel.TransactionRow row)
            {
                var vm = (TransactionsViewModel)DataContext;

                if (!vm.EditedTransactions.Contains(row))
                    vm.EditedTransactions.Add(row);
            }
        }

        private void DataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                var grid = (DataGrid)sender;
                if (grid.SelectedItem is TransactionsViewModel.TransactionRow row)
                {
                    var vm = (TransactionsViewModel)DataContext;

                    //Only mark as deleted if it exists in DB
                    if (row.transaction_id > 0)
                        vm.DeletedTransactionIds.Add(row.transaction_id);
                }
            }
        }

    }
}
