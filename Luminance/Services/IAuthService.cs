namespace Luminance.Services
{
    public interface IAuthService
    {
        string LoginWithPassword(string userName, string password);
        string LoginWithRecoveryKey(string userName, string recoveryKey);
    }
}
