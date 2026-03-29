namespace AuthService.Interfaces
{
    public interface IHashingService
    {
        string HashPassword(string password);

        bool VerifyPassword(string password, string hash);
    }
}