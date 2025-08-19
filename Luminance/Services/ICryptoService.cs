namespace Luminance.Services
{
    public interface ICryptoService
    {
        public string ConcatenateSalt(string input, string salt);
        string GeneratePasswordSalt();
        string GenerateRecoveryKey();
        string HashUserName(string userName);
        string GenerateUserKey(string password, string passwordSalt);
        string GenerateEncryptedUserKey(string userKey, string recoveryKey);
        string DecryptEncryptedUserKey(string encryptedUserKey, string recoveryKey);
        string ObfuscateDatabaseName(string originalName);
        string GenerateFieldKey();
        string EncryptFieldKey(string fieldKey, string userKey);
        string DecryptFieldKey(string encryptedFieldKey, string userKey);
        string EncryptData(string plainText, string fieldKey);
        string DecryptData(string cipherText, string fieldKey);
    }
}
