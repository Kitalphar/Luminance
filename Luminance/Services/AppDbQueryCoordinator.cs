using System.Data.SQLite;

namespace Luminance.Services
{
    public static class AppDbQueryCoordinator
    {
        public static void RunQueriesInParallel(IEnumerable<Action<SQLiteConnection>> queryActions)
        {
            Parallel.ForEach(queryActions, queryAction =>
            {
                using var conn = AppDatabaseService.OpenConnection();
                queryAction(conn);
            });
        }

        public static void RunQueriesInParallel(IEnumerable<Func<SQLiteConnection, Task>> queryTasks)
        {
            var tasks = queryTasks.Select(async queryTask =>
            {
                using var conn = AppDatabaseService.OpenConnection();
                await queryTask(conn);
            });

            Task.WaitAll(tasks.ToArray());
        }

        public static void RunQuery(Action<SQLiteConnection> queryAction)
        {
            using var conn = AppDatabaseService.OpenConnection();
            queryAction(conn);
        }

        public static T RunQuery<T>(Func<SQLiteConnection, T> queryFunc)
        {
            using var conn = AppDatabaseService.OpenConnection();
            return queryFunc(conn);
        }


        public static async Task RunQueryAsync(Func<SQLiteConnection, Task> queryFunc)
        {
            using var conn = AppDatabaseService.OpenConnection();
            await queryFunc(conn);
        }
    }
}
