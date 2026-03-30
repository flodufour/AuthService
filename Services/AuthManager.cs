using AuthService.Data;
using AuthService.DTO;
using AuthService.Models;
using Microsoft.EntityFrameworkCore;
using AuthService.Interfaces;

namespace AuthService.Services
{

    public class AuthManager : IAuthManager
    {
        private readonly AppDbContext _context;
        private readonly IHashingService _hashingService;
        private readonly ITokenService _tokenService;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly IPasswordPolicyService _passwordPolicyService;

        public AuthManager(
            AppDbContext context,
            IHashingService hashingService,
            ITokenService tokenService,
            IRefreshTokenService refreshTokenService,
            IPasswordPolicyService passwordPolicyService)
        {
            _context = context;
            _hashingService = hashingService;
            _tokenService = tokenService;
            _refreshTokenService = refreshTokenService;
            _passwordPolicyService = passwordPolicyService;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            var normalizedEmail = request.Email.ToUpper();

            var existingUser = await _context.Users
                .FirstOrDefaultAsync(x => x.NormalizedEmail == normalizedEmail);

            if (existingUser != null)
                throw new Exception("User already exists");

            var validation = _passwordPolicyService.Validate(request.Password);

            if (!validation.IsValid)
                throw new Exception(string.Join(", ", validation.Errors));

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                NormalizedEmail = request.Email.ToUpper(),
                PasswordHash = _hashingService.HashPassword(request.Password),
                CreatedAt = DateTime.UtcNow,
                IsEmailVerified = false
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

            user.LastLogin = DateTime.UtcNow;
            user.FailedLoginAttempts = 0;
            await _context.SaveChangesAsync();

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

        public async Task<MeResponse> GetCurrentUserAsync(Guid userId)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                throw new Exception("User not found");

            return new MeResponse
            {
                Id = user.Id,
                Email = user.Email,
                IsEmailVerified = user.IsEmailVerified,
                CreatedAt = user.CreatedAt,
                LastLogin = user.LastLogin
            };
        }
    }
}