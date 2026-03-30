namespace AuthService.Models
{
    public class User
    {
        public Guid Id { get; set; }

        public string Email { get; set; } = null!;
        public string NormalizedEmail { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;

        public bool IsEmailVerified { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLogin { get; set; }

        public int FailedLoginAttempts { get; set; } = 0;
        public DateTime? LockedUntil { get; set; }

        public string? EmailVerificationToken { get; set; }

        public string? PasswordResetToken { get; set; }
        public DateTime? PasswordResetTokenExpiry { get; set; }
    }
}
