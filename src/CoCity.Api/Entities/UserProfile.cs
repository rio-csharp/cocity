using System.ComponentModel.DataAnnotations.Schema;

namespace CoCity.Api.Entities;

[Table("UserProfile")]
public class UserProfile
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = null!;
    public string NickName { get; set; } = null!;
    public string? AvatarUrl { get; set; }
    public string? Bio { get; set; }
    public string? Gender { get; set; }
    public DateOnly? Birthday { get; set; }

    private UserProfile()
    {
    }

    public UserProfile(int userId, string username, string nickName, string? avatarUrl = null, string? bio = null, string? gender = null, DateOnly? birthday = null)
    {
        UserId = userId;
        UserName = username;
        NickName = nickName;
        AvatarUrl = avatarUrl;
        Bio = bio;
        Gender = gender;
        Birthday = birthday;
    }
}