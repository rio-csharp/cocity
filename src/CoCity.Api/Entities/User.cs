using System.ComponentModel.DataAnnotations.Schema;

namespace CoCity.Api.Entities;

[Table("User")]
public class User
{
    public int Id { get; set; }
    public string UserName { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime LastLoginAt { get; set; }
    public bool IsActive { get; set; }
    public string? RegisterIp { get; set; }

    private User()
    {
    }

    public User(string userName, string passwordHash, string? registerIp = null)
    {
        ThrowIfNullOrWhiteSpace(userName, nameof(userName));
        ThrowIfNullOrWhiteSpace(passwordHash, nameof(passwordHash));
        UserName = userName;
        PasswordHash = passwordHash;
        CreatedAt = DateTime.UtcNow;
        LastLoginAt = DateTime.UtcNow;
        IsActive = true;
        RegisterIp = registerIp;
    }
}