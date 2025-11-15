namespace CoCity.Api.IServices;

public interface ITokenService
{
    string GenerateAccessToken(User user);

    string GenerateRefreshToken();

    bool ValidateAccessToken(string token);

    Task SaveRefreshTokenAsync(RefreshToken token);

    Task SaveRefreshTokenAsync(string token, User user);

    Task RevokeRefreshTokenAsync(RefreshToken token);

    Task RevokeRefreshTokenAsync(string token);

    Task RevokeRefreshTokenAsync(int userId);

    Task<RefreshToken> RotateRefreshTokenAsync(string token, int userId);

    Task<bool> ValidateTokenUserAsync(string token, int userId);
}