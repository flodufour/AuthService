using AuthService.Models;

namespace AuthService.Interfaces
{
    public interface IRefreshTokenService
    {
        string HashToken(string token);

        Task<RefreshToken> SaveTokenAsync(
            Guid userId,
            string token,
            string familyId,
            DateTime expiresAt);

        Task<RefreshToken?> ValidateTokenAsync(string token);

        Task<RefreshToken> RotateTokenAsync(
            RefreshToken oldToken,
            string newToken);

        Task RevokeFamilyAsync(string familyId);

        Task LogoutAsync(string token);
    }
}
