namespace CoCity.Api.Models;

public sealed record RegisterRequest
{
    [Required(ErrorMessage = "Username is required")]
    [JsonPropertyName("username")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Username length must be between 3 and 50")]
    public string UserName { get; init; }

    [Required(ErrorMessage = "Password is required")]
    [JsonPropertyName("password")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password length must be between 6 and 100")]
    public string Password { get; init; }

    public RegisterRequest(string userName, string password)
    {
        UserName = userName;
        Password = password;
    }
}

public sealed record RegisterResponse
{
    [JsonPropertyName("userId")]
    public int UserId { get; init; }

    [JsonPropertyName("username")]
    public string UserName { get; init; }

    public RegisterResponse(int userId, string userName)
    {
        UserId = userId;
        UserName = userName;
    }
}

public sealed record LoginRequest
{
    [Required(ErrorMessage = "Username is required")]
    [JsonPropertyName("username")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Invalid username or password")]
    public string UserName { get; init; }

    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Invalid username or password")]
    [JsonPropertyName("password")]
    public string Password { get; init; }

    public LoginRequest(string userName, string password)
    {
        UserName = userName;
        Password = password;
    }
}

public sealed record LoginRequestResponse
{
    [JsonPropertyName("userId")]
    public int UserId { get; init; }

    [JsonPropertyName("username")]
    public string UserName { get; init; }

    public LoginRequestResponse(int userId, string userName)
    {
        UserId = userId;
        UserName = userName;
    }
}

public sealed record LoginResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; init; }

    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; init; }

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; init; }

    [JsonPropertyName("user")]
    public LoginRequestResponse User { get; init; }

    public LoginResponse(string accessToken, string refreshToken, int expiresIn, LoginRequestResponse user)
    {
        AccessToken = accessToken;
        RefreshToken = refreshToken;
        ExpiresIn = expiresIn;
        User = user;
    }
}

public sealed record RefreshRequest
{
    [Required(ErrorMessage = "Refresh token is required")]
    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; init; }
    public RefreshRequest(string refreshToken)
    {
        RefreshToken = refreshToken;
    }
}

public sealed record RefreshResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; init; }

    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; init; }

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; init; }

    public RefreshResponse(string accessToken, string refreshToken, int expiresIn)
    {
        AccessToken = accessToken;
        RefreshToken = refreshToken;
        ExpiresIn = expiresIn;
    }
}

public sealed record LogoutRequest
{
    [Required(ErrorMessage = "Refresh token is required")]
    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; init; }

    public LogoutRequest(string refreshToken)
    {
        RefreshToken = refreshToken;
    }
}

public sealed record LogoutResponse
{
    [JsonPropertyName("message")]
    public string Message { get; init; }

    public LogoutResponse(string message)
    {
        Message = message;
    }
}

public sealed record ChangePasswordRequest
{
    [Required(ErrorMessage = "Old password is required")]
    [JsonPropertyName("old_password")]
    public string OldPassword { get; init; }

    [Required(ErrorMessage = "New password is required")]
    [JsonPropertyName("new_password")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password length must be between 6 and 100")]
    public string NewPassword { get; init; }

    [JsonPropertyName("refresh_token")]
    [Required(ErrorMessage = "Refresh token is required")]
    public string RefreshToken { get; init; }

    public ChangePasswordRequest(string oldPassword, string newPassword, string refreshToken)
    {
        OldPassword = oldPassword;
        NewPassword = newPassword;
        RefreshToken = refreshToken;
    }
}

public sealed record ChangePasswordResponse
{
    [JsonPropertyName("message")]
    public string Message { get; init; }

    public ChangePasswordResponse(string message)
    {
        Message = message;
    }
}