namespace AuthService.Interfaces
{
    public interface ITokenGenerator
    {
        public string GenerateRefreshToken();
    }
}
