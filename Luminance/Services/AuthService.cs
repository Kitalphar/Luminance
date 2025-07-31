using System.IO;

namespace Luminance.Services
{
    internal class AuthService : IAuthService
    {
        private readonly ICryptoService _cryptoService;

        public AuthService(ICryptoService cryptoService)
        {
            _cryptoService = cryptoService;
        }
        
        public string LoginWithPassword(string enteredPassword, string storedSalt, string storedHash)
        {
            //// Verify the password
            //bool isPasswordValid = VerifyPassword(enteredPassword, storedHash, storedSalt);

            //if (!isPasswordValid)
            //    throw new UnauthorizedAccessException("Invalid password");

            // Generate a user key based on the password
            string dbName = "something";
            InitializeDatabase(dbName);

            return "userKey"; // Replace with actual logic to generate userKey
        }

        //public string LoginWithRecoveryKey(string encryptedUserKey, string recoveryKey)
        //{
        //    // Decrypt the encrypted user key using the recovery key
        //    string dbName = "something";
        //    InitializeDatabase(dbName);

        //    return _cryptoService.DecryptData(encryptedUserKey, recoveryKey); // ????
        //}


        private void InitializeDatabase(string dbName)
        {
            string dataFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            string dbFile = Path.Combine(dataFolder, $"{dbName}.db");

            if (!File.Exists(dbFile))
            {
                throw new FileNotFoundException("User database not found.");
            }

            string connectionString = $"Data Source={dbFile};Version=3;";
            UserDatabaseService.Initialize(connectionString);
        }
    }
}
