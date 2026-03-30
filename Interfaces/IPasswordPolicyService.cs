using AuthService.DTO;
namespace AuthService.Interfaces
{
    public interface IPasswordPolicyService
    {
        PasswordValidationResult Validate(string password);
    }
}
