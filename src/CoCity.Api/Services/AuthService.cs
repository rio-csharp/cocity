using CoCity.Api.Exceptions;

namespace CoCity.Api.Services;

public class AuthService : IAuthService
{
    private readonly ILogger<AuthService> _logger;
    private readonly IUserRepository _userRepository;
    private readonly IPasswordService _passwordService;
    private readonly ITokenService _tokenService;

    public AuthService(ILogger<AuthService> logger, IUserRepository userRepository, IPasswordService passwordService, ITokenService tokenService)
    {
        _logger = logger;
        _userRepository = userRepository;
        _passwordService = passwordService;
        _tokenService = tokenService;
    }

    public async Task<ChangePasswordResponse> ChangePasswordAsync(ChangePasswordRequest changePasswordRequest, int userId)
    {
        _logger.LogInformation("Change password request for user ID: {UserId}", userId);
        if (changePasswordRequest.OldPassword == changePasswordRequest.NewPassword)
        {
            _logger.LogWarning("New password is the same as the old password for user ID: {UserId}", userId);
            return new ChangePasswordResponse("New password cannot be the same as the old password");
        }
        if (!await _tokenService.ValidateTokenUserAsync(changePasswordRequest.RefreshToken, userId))
        {
            _logger.LogWarning("Invalid refresh token provided for password change for user ID: {UserId}", userId);
            throw new InvalidCredentialsException();
        }
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("User not found for ID: {UserId}", userId);
            throw new InvalidCredentialsException();
        }
        if (!_passwordService.VerifyPassword(changePasswordRequest.OldPassword, user.PasswordHash))
        {
            _logger.LogWarning("Old password verification failed for user ID: {UserId}", userId);
            throw new InvalidCredentialsException();
        }
        user.PasswordHash = _passwordService.HashPassword(changePasswordRequest.NewPassword);
        await _userRepository.UpdateAsync(user);
        await _tokenService.RevokeRefreshTokenAsync(user.Id);
        _logger.LogInformation("Password changed successfully for user ID: {UserId}", userId);
        return new ChangePasswordResponse("Password changed successfully");
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest loginRequest)
    {
        _logger.LogInformation("Login attempt for user: {UserName}", loginRequest.UserName);
        var user = await _userRepository.GetByNameAsync(loginRequest.UserName);
        if (user is null || !_passwordService.VerifyPassword(loginRequest.Password, user.PasswordHash))
        {
            _logger.LogWarning("Login failed for user: {UserName}", loginRequest.UserName);
            throw new InvalidCredentialsException();
        }
        if (!user.IsActive)
        {
            _logger.LogWarning("Login attempt for inactive user: {UserName}", loginRequest.UserName);
            throw new InvalidCredentialsException();
        }
        var token = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();
        var response = new LoginResponse(
            accessToken: token,
            refreshToken: refreshToken,
            expiresIn: 3600,
            user: new LoginRequestResponse(user.Id, user.UserName)
        );
        await _tokenService.SaveRefreshTokenAsync(refreshToken, user);
        _logger.LogInformation("User logged in successfully: {UserName} (ID: {UserId})", user.UserName, user.Id);
        return response;
    }

    public async Task<LogoutResponse> LogoutAsync(LogoutRequest logoutRequest, int userId)
    {
        _logger.LogInformation("Logout attempt with refresh token: {RefreshToken}", logoutRequest.RefreshToken);
        if (!await _tokenService.ValidateTokenUserAsync(logoutRequest.RefreshToken, userId))
        {
            _logger.LogWarning("Invalid logout attempt with refresh token: {RefreshToken}", logoutRequest.RefreshToken);
            throw new InvalidCredentialsException();
        }
        await _tokenService.RevokeRefreshTokenAsync(logoutRequest.RefreshToken);
        _logger.LogInformation("Logout successful for refresh token: {RefreshToken}", logoutRequest.RefreshToken);
        return new LogoutResponse("Logout successful");
    }

    public async Task<RefreshResponse> RefreshTokenAsync(RefreshRequest refreshRequest, int userId)
    {
        _logger.LogInformation("Refresh token attempt with token: {RefreshToken}", refreshRequest.RefreshToken);
        if (!await _tokenService.ValidateTokenUserAsync(refreshRequest.RefreshToken, userId))
        {
            _logger.LogWarning("Invalid refresh token attempt with token: {RefreshToken}", refreshRequest.RefreshToken);
            throw new InvalidCredentialsException();
        }
        var token = await _tokenService.RotateRefreshTokenAsync(refreshRequest.RefreshToken, userId);
        var response = new RefreshResponse(
            accessToken: _tokenService.GenerateAccessToken((await _userRepository.GetByIdAsync(userId))!),
            refreshToken: token.Token,
            expiresIn: 3600
        );
        _logger.LogInformation("Refresh token successful for user ID: {UserId}", userId);
        return response;
    }

    public async Task<RegisterResponse> RegisterAsync(RegisterRequest registerRequest, string? registerIp)
    {
        _logger.LogInformation("Attempting to register user: {UserName}", registerRequest.UserName);
        var user = await _userRepository.GetByNameAsync(registerRequest.UserName);
        if (user is not null)
        {
            throw new UserAlreadyExistsException(registerRequest.UserName);
        }

        var hashedPassword = _passwordService.HashPassword(registerRequest.Password);
        var newUser = new User(registerRequest.UserName, hashedPassword);
        if (!string.IsNullOrWhiteSpace(registerIp))
        {
            newUser.RegisterIp = registerIp;
        }

        var createdUser = await _userRepository.AddAsync(newUser);
        var response = new RegisterResponse(createdUser.Id, createdUser.UserName);

        _logger.LogInformation("User registered successfully: {UserName} (ID: {UserId})", response.UserName, response.UserId);
        return response;
    }
}
