using AuthService.DTO;
using AuthService.Interfaces;

namespace AuthService.Security
{
    public class PasswordPolicyService : IPasswordPolicyService
    {
        public PasswordValidationResult Validate(string password)
        {
            var result = new PasswordValidationResult();

            if (string.IsNullOrWhiteSpace(password))
                result.Errors.Add("Password is required");

            if (password.Length < 10)
                result.Errors.Add("Minimum 10 characters");

            if (!password.Any(char.IsUpper))
                result.Errors.Add("Missing uppercase");

            if (!password.Any(char.IsLower))
                result.Errors.Add("Missing lowercase");

            if (!password.Any(char.IsDigit))
                result.Errors.Add("Missing number");

            var commonPasswords = new[] { "Password123" };

            if (commonPasswords.Contains(password.ToLower()))
                result.Errors.Add("You deserve to be hacked");
                
            result.IsValid = !result.Errors.Any();

            return result;
        }
    }
}
