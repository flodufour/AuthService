using AuthService.DTO;

namespace AuthService.Interfaces
{
    public interface IAuthManager
    {
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        Task<AuthResponse> LoginAsync(LoginRequest request);
        Task<AuthResponse> RefreshAsync(RefreshTokenRequest request);
        Task LogoutAsync(LogoutRequest request);
        Task<MeResponse> GetCurrentUserAsync(Guid userId);
    }
}
