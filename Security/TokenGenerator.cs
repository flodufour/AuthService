using System.Security.Cryptography;

namespace AuthService.Security
{
    public class TokenGenerator
    {
        public string GenerateRefreshToken()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        }
    }
}
