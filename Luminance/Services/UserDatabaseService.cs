using System.Data.SQLite;

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
        }

        public static UserDatabaseService Instance =>
            _instance ?? throw new InvalidOperationException("ERR_SERVICE_NOT_INITIALIZED(10)");

        internal static SQLiteConnection OpenConnection()
        {
            if (!IsAccessAllowed)
                throw new InvalidOperationException("ERR_DATABASE_ACCESS_DENIED(220)");

            var conn = new SQLiteConnection(Instance.ConnectionString);
            conn.Open();
            return conn;
        }

        private UserDatabaseService(string connectionString)
        {
            ConnectionString = connectionString;
            //verify file existence, integrity, etc?
        }

        public void DecryptDatabase()
        {
            //logic to decrypt the DB file here
        }

        public void EncryptDatabase()
        {
            //logic to encrypt the DB file here
        }
    }
}
