namespace Luminance.Services
{
    public interface IAuthService
    {
        string LoginWithPassword(string enteredPassword, string storedSalt, string storedHash);
        //string LoginWithRecoveryKey(string encryptedUserKey, string recoveryKey);
    }
}
