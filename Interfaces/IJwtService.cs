namespace AuthService.Interfaces
{
    /// <summary>
    /// Defines the contract for generating and managing JSON Web Tokens (JWT).
    /// </summary>
    public interface IJwtService
    {
        /// <summary>
        /// Creates a signed JWT for a specific user containing their identity claims.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="email">The user's email address to be included as a claim.</param>
        /// <returns>A string representing the encoded and signed JWT.</returns>
        string GenerateToken(Guid userId, string email);
    }
}
