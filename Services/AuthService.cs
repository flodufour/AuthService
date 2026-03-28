using AuthService.Data;
using AuthService.DTO;
using AuthService.Models;
using AuthService.Security;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Services
{

    public class AuthService
    {
        private readonly AppDbContext _context;
        private readonly HashingService _hashingService;
        private readonly TokenService _tokenService;
        private readonly RefreshTokenService _refreshTokenService;

        public AuthService(
            AppDbContext context,
            HashingService hashingService,
            TokenService tokenService,
            RefreshTokenService refreshTokenService)
        {
            _context = context;
            _hashingService = hashingService;
            _tokenService = tokenService;
            _refreshTokenService = refreshTokenService;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(x => x.Email == request.Email);

            if (existingUser != null)
                throw new Exception("User already exists");

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                PasswordHash = _hashingService.HashPassword(request.Password),
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var familyId = Guid.NewGuid().ToString();

            return await _tokenService.CreateTokensAsync(
                user.Id,
                user.Email,
                familyId
            );
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Email == request.Email);

            if (user == null)
                throw new Exception("Invalid credentials");

            var isValid = _hashingService.VerifyPassword(
                request.Password,
                user.PasswordHash
            );

            if (!isValid)
                throw new Exception("Invalid credentials");

            var familyId = Guid.NewGuid().ToString();

            return await _tokenService.CreateTokensAsync(
                user.Id,
                user.Email,
                familyId
            );
        }

        public async Task<AuthResponse> RefreshAsync(RefreshTokenRequest request)
        {
            var storedToken = await _refreshTokenService
                .ValidateTokenAsync(request.RefreshToken);

            if (storedToken == null)
                throw new Exception("Invalid refresh token");

            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Id == storedToken.UserId);

            if (user == null)
                throw new Exception("User not found");

            var newRefreshToken = Guid.NewGuid().ToString();

            await _refreshTokenService.RotateTokenAsync(
                storedToken,
                newRefreshToken
            );

            var accessToken = _tokenService.CreateTokensAsync(
                user.Id,
                user.Email,
                storedToken.FamilyId
            );

            return await accessToken;
        }

        public async Task LogoutAsync(LogoutRequest request)
        {
            await _refreshTokenService.LogoutAsync(request.RefreshToken);
        }
    }
}