using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;

namespace Luminance.Services
{
    public class CryptoService : ICryptoService
    {
        public string ConcatenateSalt(string input, string salt)
        {
            return string.Concat(input, salt);
        }

        public string GeneratePasswordSalt()
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[16];
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }
        public string GenerateRecoveryKey()
        {
            const string charSet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[32];
            rng.GetBytes(bytes);

            var keyBuilder = new StringBuilder(32);

            foreach (var b in bytes)
            {
                //Get an index from 0 to 61 (size of charSet) and add corresponding character to the key
                keyBuilder.Append(charSet[b % charSet.Length]);
            }

            return keyBuilder.ToString();
        }
        public string HashUserName(string userName)
        {
            byte[] hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(userName));
            return ToBase64Url(hashBytes);
        }

        private static string ToBase64Url(byte[] bytes)
        {
            return Convert.ToBase64String(bytes)
                .Replace("+", "3")
                .Replace("/", "6")
                .Replace("=", "9");
        }

        private static string EncryptToBase64(byte[] plainBytes, byte[] key)
        {
            byte[] nonce = new byte[12];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(nonce);

            byte[] cipherText = new byte[plainBytes.Length];
            byte[] authenticationTag = new byte[16];

            using var aesGcm = new AesGcm(key, 16);
            aesGcm.Encrypt(nonce, plainBytes, cipherText, authenticationTag);

            byte[] result = new byte[nonce.Length + cipherText.Length + authenticationTag.Length];
            Buffer.BlockCopy(nonce, 0, result, 0, nonce.Length);
            Buffer.BlockCopy(cipherText, 0, result, nonce.Length, cipherText.Length);
            Buffer.BlockCopy(authenticationTag, 0, result, nonce.Length + cipherText.Length, authenticationTag.Length);

            return Convert.ToBase64String(result);
        }

        private static byte[] DecryptFromBase64(string base64CipherText, byte[] key)
        {
            byte[] encryptedBytes = Convert.FromBase64String(base64CipherText);
            const int nonceLength = 12;
            const int tagLength = 16;

            if (encryptedBytes.Length < nonceLength + tagLength)
                throw new ArgumentException("ERR_INVALID_ENCRYPTED_FORMAT(301)");

            byte[] nonce = new byte[nonceLength];
            byte[] authenticationTag = new byte[tagLength];
            byte[] cipherText = new byte[encryptedBytes.Length - nonceLength - tagLength];

            Buffer.BlockCopy(encryptedBytes, 0, nonce, 0, nonceLength);
            Buffer.BlockCopy(encryptedBytes, nonceLength, cipherText, 0, cipherText.Length);
            Buffer.BlockCopy(encryptedBytes, nonceLength + cipherText.Length, authenticationTag, 0, tagLength);

            byte[] plainText = new byte[cipherText.Length];

            using var aesGcm = new AesGcm(key, 16);
            aesGcm.Decrypt(nonce, cipherText, authenticationTag, plainText);

            return plainText;
        }

        public static byte[] ConvertTo16ByteKey(string input)
        {
            byte[] hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
            return hashBytes.Take(16).ToArray();
        }

        public string GenerateUserKey(string password, string passwordSalt)
        {
            var passwordBytes = Encoding.UTF8.GetBytes(password);
            var saltBytes = Convert.FromBase64String(passwordSalt);

            var argon2 = new Argon2id(passwordBytes)
            {
                Salt = saltBytes,
                //parallelism = Math.Min(4, Environment.ProcessorCount),
                DegreeOfParallelism = Environment.ProcessorCount,
                MemorySize = 65536, // 64 MB
                Iterations = 4
            };

            byte[] key = argon2.GetBytes(32);

            return Convert.ToBase64String(key);
        }

        public string GenerateEncryptedUserKey(string userKey, string recoveryKey)
        {
            var userKeyBytes = Encoding.UTF8.GetBytes(userKey);
            var recoveryKeyBytes = ConvertTo16ByteKey(recoveryKey);

            return EncryptToBase64(userKeyBytes, recoveryKeyBytes);
        }

        public string DecryptEncryptedUserKey(string encryptedUserKey, string recoveryKey)
        {
            byte[] keyBytes = ConvertTo16ByteKey(recoveryKey);
            byte[] decryptedBytes = DecryptFromBase64(encryptedUserKey, keyBytes);

            return Encoding.UTF8.GetString(decryptedBytes);
        }

        public string ObfuscateDatabaseName(string userName)
        {
            const int length = 12;
            const string allowedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            //Generate a random database filename
            var randomBytes = new byte[length];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);

            var chars = new char[length];
            for (int i = 0; i < length; i++)
            {
                chars[i] = allowedChars[randomBytes[i] % allowedChars.Length];
            }

            string originDBName = new string (chars);
            string normalizedUserName = userName.Trim().ToLowerInvariant();
            string combinedText = ConcatenateSalt(originDBName, userName);

            //Generate hashed database file name for App.db
            byte[] fullHash = SHA256.HashData(Encoding.UTF8.GetBytes(combinedText));
            byte[] shortHash = fullHash.Take(12).ToArray();
            string fileName = Convert.ToHexString(shortHash);

            return fileName;
        }

        public string GenerateFieldKey()
        {
            byte[] key = new byte[16];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(key);
            return Convert.ToBase64String(key);
        }

        public string EncryptFieldKey(string fieldKey, string userKey)
        {
            
            byte[] fieldKeyBytes = Convert.FromBase64String(fieldKey);
            var userKeyBytes = ConvertTo16ByteKey(userKey);

            return EncryptToBase64(fieldKeyBytes, userKeyBytes);
        }

        public string DecryptFieldKey(string encryptedFieldKey, string userKey)
        {
            var userKeyBytes = ConvertTo16ByteKey(userKey);

            byte[] decryptedBytes = DecryptFromBase64(encryptedFieldKey, userKeyBytes);
            return Convert.ToBase64String(decryptedBytes);
        }

        public string EncryptData(string plainText, string fieldKey)
        {
            byte[] fieldKeyBytes = ConvertTo16ByteKey(fieldKey);
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);

            return EncryptToBase64(plainBytes, fieldKeyBytes);
        }

        public string DecryptData(string cipherText, string fieldKey)
        {
            byte[] fieldKeyBytes = ConvertTo16ByteKey(fieldKey);
            byte[] plainBytes = DecryptFromBase64(cipherText, fieldKeyBytes);

            return Encoding.UTF8.GetString(plainBytes);
        }
    }
}