using System.Security.Cryptography;
using AuthService.Interfaces;

namespace AuthService.Security
{
    public class TokenGenerator : ITokenGenerator
    {
        public string GenerateRefreshToken()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        }
    }
}
