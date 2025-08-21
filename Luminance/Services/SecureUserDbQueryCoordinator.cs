using Microsoft.Data.Sqlite;

namespace Luminance.Services
{
    public static class SecureUserDbQueryCoordinator
    {
        private static readonly object _lock = new();
        private static int _activeQuerySessions = 0;

        private static void StartSecureSession()
        {
            lock (_lock)
            {
                if (_activeQuerySessions == 0)
                {
                    UserDatabaseService.Instance.DecryptDatabase();
                    UserDatabaseService.AllowAccess();

                }

                _activeQuerySessions++;
            }
        }

        private static void EndSecureSession()
        {
            lock (_lock)
            {
                _activeQuerySessions--;

                if (_activeQuerySessions == 0)
                {
                    UserDatabaseService.DenyAccess();
                    //UserDatabaseService.Instance.EncryptDatabase();
                }
            }
        }

        public static void RunQueriesInParallel(IEnumerable<Action<SqliteConnection>> queryActions)
        {
            StartSecureSession();
            try
            {
                Parallel.ForEach(queryActions, queryAction =>
                {
                    using var conn = UserDatabaseService.OpenConnection();
                    queryAction(conn);
                });
            }
            finally
            {
                EndSecureSession();
            }
        }

        public static void RunQueriesInParallel(IEnumerable<Func<SqliteConnection, Task>> queryTasks)
        {
            StartSecureSession();
            try
            {
                var tasks = queryTasks.Select(async queryTask =>
                {
                    using var conn = UserDatabaseService.OpenConnection();
                    await queryTask(conn);
                });

                Task.WaitAll(tasks.ToArray());
            }
            finally
            {
                EndSecureSession();
            }
        }

        public static void RunQuery(Action<SqliteConnection> queryAction)
        {
            StartSecureSession();
            try
            {
                using var conn = UserDatabaseService.OpenConnection();
                queryAction(conn);
            }
            finally
            {
                EndSecureSession();
            }
        }

        public static T RunQuery<T>(Func<SqliteConnection, T> queryFunc)
        {
            StartSecureSession();
            try
            {
                using var conn = UserDatabaseService.OpenConnection();
                return queryFunc(conn);
            }
            finally
            {
                EndSecureSession();
            }
        }

        public static async Task RunQueryAsync(Func<SqliteConnection, Task> queryFunc)
        {
            StartSecureSession();
            try
            {
                using var conn = UserDatabaseService.OpenConnection();
                await queryFunc(conn);
            }
            finally
            {
                EndSecureSession();
            }
        }


        public static void RunTransaction(Action<SqliteConnection> transactionAction)
        {
            StartSecureSession();
            try
            {
                using var conn = UserDatabaseService.OpenConnection();
                transactionAction(conn);
            }
            finally
            {
                EndSecureSession();
            }

        }

    }
}
