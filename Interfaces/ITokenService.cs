using AuthService.DTO;

namespace AuthService.Interfaces
{
    public interface ITokenService
    {
        Task<AuthResponse> CreateTokensAsync(
            Guid userId,
            string email,
            string familyId);
    }
}
