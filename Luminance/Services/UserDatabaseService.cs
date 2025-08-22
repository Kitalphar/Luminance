using Microsoft.Data.Sqlite;
using System.Threading.Channels;
using System.Windows.Documents;
using Luminance.Helpers;

namespace Luminance.Services
{
    public class UserDatabaseService
    {
        private static bool _accessAllowed = false;
        internal static void AllowAccess() => _accessAllowed = true;
        internal static void DenyAccess() => _accessAllowed = false;
        internal static bool IsAccessAllowed => _accessAllowed;


        private static UserDatabaseService? _instance;
        public string ConnectionString { get; }

        public static void Initialize(string connectionString)
        {
            if (_instance == null)
                _instance = new UserDatabaseService(connectionString);

            //Initialize SqlCipher
            SQLitePCL.Batteries_V2.Init();
        }

        public static UserDatabaseService Instance =>
            _instance ?? throw new InvalidOperationException("ERR_SERVICE_NOT_INITIALIZED(10)");

        internal static SqliteConnection OpenConnection()
        {
            if (!IsAccessAllowed)
                throw new InvalidOperationException("ERR_DATABASE_ACCESS_DENIED(220)");

            var userKey = GetUserKey();

            var conn = new SqliteConnection(Instance.ConnectionString);
            conn.Open();

            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = $"PRAGMA key = '{userKey}';";
                cmd.ExecuteNonQuery();

                cmd.Parameters.Clear();
                cmd.CommandText = "PRAGMA foreign_keys = ON;";
                cmd.ExecuteNonQuery();
            }

            return conn;
        }

        private UserDatabaseService(string connectionString)
        {
            ConnectionString = connectionString;
            //verify file existence, integrity, etc?
        }

        public void DecryptDatabase()
        {
            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();

            var userKey = GetUserKey();

            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = $"PRAGMA key = '{userKey}';";
                cmd.ExecuteNonQuery();
            }

            // Test a simple query to ensure the key works (throws if invalid)
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "PRAGMA schema_version;";
                cmd.ExecuteScalar();
            }
        }

        //No-op with SQLCipher (DB always encrypted at rest).
        //Could be used for PRAGMA rekey if password changes.
        public void EncryptDatabase()
        {
            // Intentionally left blank.
        }

        private static string GetUserKey()
        {
            var userKey = AppSettings.Instance.Get("userKey");

            if (string.IsNullOrWhiteSpace(userKey))
                throw new InvalidOperationException("ERR_USERKEY_NOT_AVAILABLE(221)");

            return userKey;
        }

        public void RekeyDatabase(string newKey)
        {
            using var conn = OpenConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = $"PRAGMA rekey = '{newKey}';";
            cmd.ExecuteNonQuery();
        }
    }
}
