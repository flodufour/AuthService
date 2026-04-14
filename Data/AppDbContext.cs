using AuthService.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Data;

/// <summary>
/// The primary database context for the Authentication Service.
/// Handles User accounts and Refresh Token persistence.
/// </summary>
public class AppDbContext : DbContext
{
    // Constructor passing configuration options to the base DbContext
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    // Table representing registered users
    public DbSet<User> Users => Set<User>();
    // Table representing active or expired refresh tokens for JWT sessions
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    /// <summary>
    /// Configures the database schema and constraints using the Fluent API.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Ensure Email is unique at the database level for faster lookups and data integrity;
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // Add an index to TokenHash to optimize performance when validating tokens during login;
        modelBuilder.Entity<RefreshToken>()
            .HasIndex(r => r.TokenHash);
    }
}
