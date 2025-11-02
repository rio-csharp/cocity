using System.ComponentModel.DataAnnotations.Schema;

namespace CoCity.Api.Entities;

[Table("RefreshToken")]
public class RefreshToken
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
}