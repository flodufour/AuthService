using AuthService.DTO;
using AuthService.Interfaces;

namespace AuthService.Services
{
    public class TokenService : ITokenService
    {
        private readonly IJwtService _jwtService;
        private readonly ITokenGenerator _tokenGenerator;
        private readonly IRefreshTokenService _refreshTokenService;

        public TokenService(
            IJwtService jwtService,
            ITokenGenerator tokenGenerator,
            IRefreshTokenService refreshTokenService)
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