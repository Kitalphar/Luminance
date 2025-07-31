using System.Data.SQLite;
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

        public static SQLiteConnection OpenConnection()
        {
            var conn = new SQLiteConnection(AppDatabaseService.Instance.ConnectionString);
            conn.Open();

            return conn;
        }

        public static AppDatabaseService Instance =>
            _instance ?? throw new InvalidOperationException("ERR_SERVICE_NOT_INITIALIZED(101)");

        private AppDatabaseService(string connectionString)
        {
            ConnectionString = connectionString;

            // Open a temp connection to set up WAL and load initial app settings
            using var conn = new SQLiteConnection(connectionString);
            conn.Open();

            using (var cmd = new SQLiteCommand("PRAGMA journal_mode=WAL;", conn))
                cmd.ExecuteNonQuery();

            LoadAppSettings(conn);
        }

        private void LoadAppSettings(SQLiteConnection conn)
        {
            using var cmd = new SQLiteCommand("SELECT key, value FROM settings", conn);
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
