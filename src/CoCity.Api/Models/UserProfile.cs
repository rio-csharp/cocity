namespace CoCity.Api.Models;

public sealed record UserProfileResponseModel
{
    [JsonPropertyName("userId")]
    public string UserId { get; init; } = null!;
    [JsonPropertyName("username")]
    public string Username { get; init; } = null!;
    [JsonPropertyName("nickName")]
    public string NickName { get; init; } = null!;
    [JsonPropertyName("avatarUrl")]
    public string? AvatarUrl { get; init; }
    [JsonPropertyName("bio")]
    public string? Bio { get; init; }
    [JsonPropertyName("gender")]
    public string? Gender { get; init; }
    [JsonPropertyName("birthday")]
    public string? Birthday { get; init; }
    public UserProfileResponseModel(string userId, string username, string nickName, string? avatarUrl = null, string? bio = null, string? gender = null, string? birthday = null)
    {
        UserId = userId;
        Username = username;
        NickName = nickName;
        AvatarUrl = avatarUrl;
        Bio = bio;
        Gender = gender;
        Birthday = birthday;
    }
}

public sealed record UpdateUserProfileRequestModel
{
    [JsonPropertyName("nickName")]
    [StringLength(100, ErrorMessage = "Nick name cannot exceed 100 characters.")]
    public string? NickName { get; init; }
    [JsonPropertyName("avatarUrl")]
    [StringLength(255, ErrorMessage = "Avatar URL cannot exceed 255 characters.")]
    public string? AvatarUrl { get; init; }
    [JsonPropertyName("bio")]
    [StringLength(500, ErrorMessage = "Bio cannot exceed 500 characters.")]
    public string? Bio { get; init; }
    [JsonPropertyName("gender")]
    [StringLength(20, ErrorMessage = "Gender cannot exceed 20 characters.")]
    public string? Gender { get; init; }
    [JsonPropertyName("birthday")]
    [StringLength(10, ErrorMessage = "Birthday cannot exceed 10 characters (e.g., YYYY-MM-DD).")]
    public string? Birthday { get; init; }
}

public sealed record UpdateUserProfileResponseModel
{
    [JsonPropertyName("message")]
    public string Message { get; init; } = null!;
    public UpdateUserProfileResponseModel(string message)
    {
        Message = message;
    }
}