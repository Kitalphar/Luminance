namespace Luminance.ViewModels
{
    internal class TransactionsViewModel
    {
        //TODO: Create Observable Collection for data read from Database.

        //TODO: Create Lists for edited, deleted and new rows.

        public class TransactionRow
        {
            public required int Transaction_id;
            public required int Account_id;
            public required string Description;
            public required double Trans_amount;
            public required string Currency_code;
            public required int Category_id;
            public required DateTime Trans_date;
            public required string Status;
        }

        private static bool OnRowEdit()
        {
            //User edits Datagrid row, edit Observable Collection and Datagrid data.
            //Save it in an editedRows Collection/List

            //Edit database row immediately or pool queries together in one "upload" type function?

            //Return true if success, false if not.
            return true;
        }

        private static bool OnRowDelete()
        {
            //User deletes Datagrid row, delete it from Observable Collection and Datagrid data.
            //Save it in a deletedRows Collection/List.

            //Edit database row immediately or pool queries together in one "upload" type function?

            //Return true if success, false if not.
            return true;
        }
            
        private static bool NewRowCreation()
        {
            //User creates Datagrid row, add it to Observable Collection and Datagrid data.
            //Save it in a newRows Collection/List.

            //Edit database row immediately or pool queries together in one "upload" type function?

            //Return true if success, false if not.
            return true;
        }
    }
}
