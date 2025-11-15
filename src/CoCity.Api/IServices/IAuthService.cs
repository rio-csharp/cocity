namespace CoCity.Api.IServices;

public interface IAuthService
{
    Task<RegisterResponse> RegisterAsync(RegisterRequest registerRequest, string? registerIp);

    Task<LoginResponse> LoginAsync(LoginRequest loginRequest);

    Task<RefreshResponse> RefreshTokenAsync(RefreshRequest refreshRequest, int userId);

    Task<LogoutResponse> LogoutAsync(LogoutRequest logoutRequest, int userId);

    Task<ChangePasswordResponse> ChangePasswordAsync(ChangePasswordRequest changePasswordRequest, int userId);
    Task DeleteUserAsync(int userId);
}