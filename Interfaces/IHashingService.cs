namespace AuthService.Interfaces
{
    /// <summary>
    /// Provides a contract for password hashing and verification to ensure secure credential storage.
    /// </summary>
    public interface IHashingService
    {
        /// <summary>
        /// Converts a plain-text password into a secure cryptographic hash.
        /// </summary>
        /// <param name="password">The raw password string to be processed.</param>
        /// <returns>A salted hash representation of the password.</returns>
        string HashPassword(string password);
        /// <summary>
        /// Compares a plain-text password against a stored hash to determine its validity.
        /// </summary>
        /// <param name="password">The input password provided by the user.</param>
        /// <param name="hash">The legitimate hash stored in the system.</param>
        /// <returns><c>true</c> if the password matches the hash; otherwise, <c>false</c>.</returns>
        bool VerifyPassword(string password, string hash);
    }
}
