using AuthService.DTO;
using AuthService.Models;
using AuthService.Security;
using AuthService.Services;

namespace AuthService.Services
{
    public class TokenService
    {
        private readonly JwtService _jwtService;
        private readonly TokenGenerator _tokenGenerator;
        private readonly RefreshTokenService _refreshTokenService;

        public TokenService(
            JwtService jwtService,
            TokenGenerator tokenGenerator,
            RefreshTokenService refreshTokenService)
        {
            _jwtService = jwtService;
            _tokenGenerator = tokenGenerator;
            _refreshTokenService = refreshTokenService;
        }

        public async Task<AuthResponse> CreateTokensAsync(
            Guid userId,
            string email,
            string familyId)
        {
            var accessToken = _jwtService.GenerateToken(userId, email);

            var refreshToken = _tokenGenerator.GenerateRefreshToken();

            var refreshTokenExpiry = DateTime.UtcNow.AddDays(7);

            await _refreshTokenService.SaveTokenAsync(
                userId,
                refreshToken,
                familyId,
                refreshTokenExpiry
            );

            return new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(15),
                RefreshTokenExpiresAt = refreshTokenExpiry,
                TokenType = "Bearer"
            };
        }
    }
}