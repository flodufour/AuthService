using AuthService.Data;
using AuthService.DTO;
using AuthService.Exceptions;
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
        private readonly ITokenGenerator _tokenGenerator;
        private readonly IEmailService _emailService;

        public AuthManager(
            AppDbContext context,
            IHashingService hashingService,
            ITokenService tokenService,
            IRefreshTokenService refreshTokenService,
            IPasswordPolicyService passwordPolicyService,
            ITokenGenerator tokenGenerator,
            IEmailService emailService)
        {
            _context = context;
            _hashingService = hashingService;
            _tokenService = tokenService;
            _refreshTokenService = refreshTokenService;
            _passwordPolicyService = passwordPolicyService;
            _tokenGenerator = tokenGenerator;
            _emailService = emailService;
        }

        public async Task RegisterAsync(RegisterRequest request)
        {
            var normalizedEmail = request.Email.Trim().ToUpperInvariant();

            var existingUser = await _context.Users
                .FirstOrDefaultAsync(x => x.NormalizedEmail == normalizedEmail);

            if (existingUser != null)
                throw new AuthException("User already exists");

            var validation = _passwordPolicyService.Validate(request.Password);

            if (!validation.IsValid)
                throw new AuthException(string.Join(", ", validation.Errors));

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = request.Email,
                NormalizedEmail = normalizedEmail,
                PasswordHash = _hashingService.HashPassword(request.Password),
                CreatedAt = DateTime.UtcNow,
                IsEmailVerified = false
            };

            var rawVerificationToken = _tokenGenerator.GenerateRefreshToken();
            user.EmailVerificationToken = _refreshTokenService.HashToken(rawVerificationToken);
            user.EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            await _emailService.SendVerificationEmailAsync(user.Email, rawVerificationToken);
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var normalizedEmail = request.Email.Trim().ToUpperInvariant();

            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.NormalizedEmail == normalizedEmail);

            if (user == null)
                throw new AuthException("Invalid credentials");

            if (!user.IsEmailVerified)
                throw new AuthException("Please verify your email address before logging in.");

            if (user.LockedUntil.HasValue && user.LockedUntil.Value > DateTime.UtcNow)
                throw new AuthException("Account is temporarily locked. Try again later.");

            var isValid = _hashingService.VerifyPassword(
                request.Password,
                user.PasswordHash
            );

            if (!isValid)
            {
                user.FailedLoginAttempts++;
                if (user.FailedLoginAttempts >= 5)
                    user.LockedUntil = DateTime.UtcNow.AddMinutes(15);
                await _context.SaveChangesAsync();
                throw new AuthException("Invalid credentials");
            }

            var familyId = Guid.NewGuid().ToString();

            user.LastLogin = DateTime.UtcNow;
            user.FailedLoginAttempts = 0;
            user.LockedUntil = null;
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
                throw new AuthException("Invalid refresh token");

            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.Id == storedToken.UserId);

            if (user == null)
                throw new AuthException("User not found");

            var newRefreshToken = _tokenGenerator.GenerateRefreshToken();

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
                throw new AuthException("User not found");

            return new MeResponse
            {
                Id = user.Id,
                Email = user.Email,
                IsEmailVerified = user.IsEmailVerified,
                CreatedAt = user.CreatedAt,
                LastLogin = user.LastLogin
            };
        }

        public async Task VerifyEmailAsync(VerifyEmailRequest request)
        {
            var tokenHash = _refreshTokenService.HashToken(request.Token);

            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.EmailVerificationToken == tokenHash);

            if (user == null)
                throw new AuthException("Invalid or expired verification token");

            if (user.EmailVerificationTokenExpiry == null || user.EmailVerificationTokenExpiry < DateTime.UtcNow)
                throw new AuthException("Invalid or expired verification token");

            user.IsEmailVerified = true;
            user.EmailVerificationToken = null;
            user.EmailVerificationTokenExpiry = null;

            await _context.SaveChangesAsync();
        }

        public async Task ResendVerificationAsync(ResendVerificationRequest request)
        {
            var normalizedEmail = request.Email.Trim().ToUpperInvariant();

            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.NormalizedEmail == normalizedEmail);

            // Always return without error to prevent email enumeration
            if (user == null || user.IsEmailVerified)
                return;

            var rawToken = _tokenGenerator.GenerateRefreshToken();
            user.EmailVerificationToken = _refreshTokenService.HashToken(rawToken);
            user.EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24);

            await _context.SaveChangesAsync();

            await _emailService.SendVerificationEmailAsync(user.Email, rawToken);
        }

        public async Task ForgotPasswordAsync(ForgotPasswordRequest request)
        {
            var normalizedEmail = request.Email.Trim().ToUpperInvariant();

            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.NormalizedEmail == normalizedEmail);

            // Always return without error to prevent email enumeration
            if (user == null)
                return;

            var rawToken = _tokenGenerator.GenerateRefreshToken();

            user.PasswordResetToken = _refreshTokenService.HashToken(rawToken);
            user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1);

            await _context.SaveChangesAsync();

            await _emailService.SendPasswordResetEmailAsync(user.Email, rawToken);
        }

        public async Task ResetPasswordAsync(ResetPasswordRequest request)
        {
            var tokenHash = _refreshTokenService.HashToken(request.Token);

            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.PasswordResetToken == tokenHash);

            if (user == null)
                throw new AuthException("Invalid or expired reset token");

            if (user.PasswordResetTokenExpiry == null || user.PasswordResetTokenExpiry < DateTime.UtcNow)
                throw new AuthException("Invalid or expired reset token");

            var validation = _passwordPolicyService.Validate(request.NewPassword);
            if (!validation.IsValid)
                throw new AuthException(string.Join(", ", validation.Errors));

            user.PasswordHash = _hashingService.HashPassword(request.NewPassword);
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiry = null;

            // Revoke all active refresh tokens so all sessions must re-authenticate
            var families = await _context.RefreshTokens
                .Where(t => t.UserId == user.Id && !t.Revoked)
                .Select(t => t.FamilyId)
                .Distinct()
                .ToListAsync();

            foreach (var familyId in families)
                await _refreshTokenService.RevokeFamilyAsync(familyId);

            await _context.SaveChangesAsync();
        }
    }
}