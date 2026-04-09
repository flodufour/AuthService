using AuthService.DTO;

namespace AuthService.Interfaces
{
    /// <summary>
    /// Defines the contract for identity management and authentication services, 
    /// handling user registration, session persistence, and security tokens.
    /// </summary>
    public interface IAuthManager
    {
        /// <summary>
        /// Organizes the creation of a new user account and generates first security tokens.
        /// </summary>
        /// <param name="request">The registration details provided by the user.</param>
        /// <returns>An <see cref="AuthResponse"/> containing the identity profile and JWT tokens.</returns>
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        /// <summary>
        /// Validates user credentials and issues a new set of access and refresh tokens.
        /// </summary>
        /// <param name="request">The login credentials (email/username and password).</param>
        /// <returns>The authenticated user's profile and session tokens.</returns>
        Task<AuthResponse> LoginAsync(LoginRequest request);
        /// <summary>
        /// Rotates the existing refresh token to extend the user session without requiring re-authentication.
        /// </summary>
        /// <param name="request">The expired or valid refresh token details.</param>
        /// <returns>A new pair of access and refresh tokens.</returns>
        Task<AuthResponse> RefreshAsync(RefreshTokenRequest request);
        /// <summary>
        /// Invalidates the user's current session and revokes active refresh tokens.
        /// </summary>
        /// <param name="request">The session identifiers to be terminated.</param>
        Task LogoutAsync(LogoutRequest request);
        /// <summary>
        /// Retrieves the profile information for the currently authenticated user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <returns>The user's account details and claims.</returns>
        Task<MeResponse> GetCurrentUserAsync(Guid userId);
    }
}
