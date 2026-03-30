public class MeResponse
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public bool IsEmailVerified { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<string> Roles { get; set; } = new();
    public DateTime? LastLogin { get; set; }
}