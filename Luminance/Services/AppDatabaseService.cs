using Microsoft.Data.Sqlite;
using Luminance.Helpers;

namespace Luminance.Services
{
    public class AppDatabaseService
    {
        private static AppDatabaseService? _instance;
        public string ConnectionString { get; }

        public static void Initialize(string connectionString)
        {
            if (_instance == null)
                _instance = new AppDatabaseService(connectionString);
        }

        public static SqliteConnection OpenConnection()
        {
            var conn = new SqliteConnection(AppDatabaseService.Instance.ConnectionString);
            conn.Open();

            return conn;
        }

        public static AppDatabaseService Instance =>
            _instance ?? throw new InvalidOperationException("ERR_SERVICE_NOT_INITIALIZED(101)");

        private AppDatabaseService(string connectionString)
        {
            ConnectionString = connectionString;

            // Open a temp connection to set up WAL and load initial app settings
            using var conn = new SqliteConnection(connectionString);
            conn.Open();

            using (var cmd = new SqliteCommand("PRAGMA journal_mode=WAL;", conn))
                cmd.ExecuteNonQuery();

            LoadAppSettings(conn);
        }

        private void LoadAppSettings(SqliteConnection conn)
        {
            using var cmd = new SqliteCommand("SELECT key, value FROM settings", conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                string key = reader.GetString(0);
                string value = reader.GetString(1);

                AppSettings.Instance.Set(key, value);
            }
        }
    }
}
