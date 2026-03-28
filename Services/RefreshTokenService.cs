using System.Security.Cryptography;
using System.Text;
using AuthService.Data;
using AuthService.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Services
{

    public class RefreshTokenService
    {
        private readonly AppDbContext _context;

        public RefreshTokenService(AppDbContext context)
        {
            _context = context;
        }

        public string HashToken(string token)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(token);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        public async Task<RefreshToken> SaveTokenAsync(
            Guid userId,
            string token,
            string familyId,
            DateTime expiresAt)
        {
            var refreshToken = new RefreshToken
            {
                UserId = userId,
                TokenHash = HashToken(token),
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = expiresAt,
                Revoked = false,
                FamilyId = familyId
            };

            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();

            return refreshToken;
        }

        public async Task<RefreshToken?> ValidateTokenAsync(string token)
        {
            var tokenHash = HashToken(token);

            var storedToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(x => x.TokenHash == tokenHash);

            if (storedToken == null)
                return null;

            if (storedToken.Revoked)
                return null;

            if (storedToken.ExpiresAt < DateTime.UtcNow)
                return null;

            return storedToken;
        }

        public async Task<RefreshToken> RotateTokenAsync(
            RefreshToken oldToken,
            string newToken)
        {
            oldToken.Revoked = true;

            var newRefreshToken = new RefreshToken
            {
                UserId = oldToken.UserId,
                TokenHash = HashToken(newToken),
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                Revoked = false,
                FamilyId = oldToken.FamilyId,
                ReplacedByToken = oldToken.TokenHash
            };

            _context.RefreshTokens.Add(newRefreshToken);

            await _context.SaveChangesAsync();

            return newRefreshToken;
        }

        public async Task RevokeFamilyAsync(string familyId)
        {
            var tokens = await _context.RefreshTokens
                .Where(x => x.FamilyId == familyId && !x.Revoked)
                .ToListAsync();

            foreach (var token in tokens)
            {
                token.Revoked = true;
            }

            await _context.SaveChangesAsync();
        }


        public async Task LogoutAsync(string token)
        {
            var tokenHash = HashToken(token);

            var storedToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(x => x.TokenHash == tokenHash);

            if (storedToken != null)
            {
                storedToken.Revoked = true;
                await _context.SaveChangesAsync();
            }
        }
    }
}