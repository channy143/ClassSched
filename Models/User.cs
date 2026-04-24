using SQLite;

namespace ClassSched.Models;

[Table("Users")]
public class User
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [NotNull, Unique]
    public string Email { get; set; } = string.Empty;

    [NotNull]
    public string PasswordHash { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public bool IsEmailVerified { get; set; } = false;

    public string? EmailVerificationCode { get; set; }

    public DateTime? VerificationCodeExpiry { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastLoginAt { get; set; }
}
