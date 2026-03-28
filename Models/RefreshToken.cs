namespace AuthService.Models;

public class RefreshToken
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string TokenHash { get; set; } = null!;
    public Guid UserId { get; set; }

    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool Revoked { get; set; }

    public string? ReplacedByToken { get; set; }

    public string FamilyId { get; set; } = null!;
}