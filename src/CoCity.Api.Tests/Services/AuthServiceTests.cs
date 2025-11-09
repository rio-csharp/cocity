using CoCity.Api.Entities;
using CoCity.Api.Models;

namespace CoCity.Api.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordService> _passwordServiceMock;
    private readonly Mock<ILogger<AuthService>> _loggerMock;
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly AuthService _authService;
    private readonly string _refreshToken = "Mnhroz8srEyMcea76KAsSg==";

    public AuthServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _passwordServiceMock = new Mock<IPasswordService>();
        _loggerMock = new Mock<ILogger<AuthService>>();
        _tokenServiceMock = new Mock<ITokenService>();
        _authService = new AuthService(_loggerMock.Object, _userRepositoryMock.Object, _passwordServiceMock.Object, _tokenServiceMock.Object);
    }

    [Fact]
    public async Task RegisterAsync_UserAlreadyExists_ThrowsUserAlreadyExistsException()
    {
        var request = new RegisterRequest("existingUser", "password123");
        var existingUser = new User(request.UserName, "hash");
        _userRepositoryMock.Setup(r => r.GetByNameAsync(request.UserName))
            .ReturnsAsync(existingUser);

        await Assert.ThrowsAsync<UserAlreadyExistsException>(async () =>
            await _authService.RegisterAsync(request, "127.0.0.1"));
    }

    [Fact]
    public async Task RegisterAsync_NewUser_ReturnsRegisterResponse()
    {
        var request = new RegisterRequest("newUser", "password123");
        _userRepositoryMock.Setup(r => r.GetByNameAsync(request.UserName))
            .ReturnsAsync((User)null!);
        _passwordServiceMock.Setup(p => p.HashPassword(request.Password))
            .Returns("hashedPassword");

        var createdUser = new User(request.UserName, "hashedPassword")
        {
            Id = 42,
            RegisterIp = "127.0.0.1"
        };
        _userRepositoryMock.Setup(r => r.AddAsync(It.IsAny<User>()))
            .ReturnsAsync(createdUser);

        var response = await _authService.RegisterAsync(request, "127.0.0.1");

        Assert.NotNull(response);
        Assert.Equal(createdUser.Id, response.UserId);
        Assert.Equal(createdUser.UserName, response.UserName);
    }

    [Fact]
    public async Task LoginAsync_InvalidUser_ThrowsInvalidCredentialsException()
    {
        var request = new LoginRequest("nouser", "password");
        _userRepositoryMock.Setup(r => r.GetByNameAsync(request.UserName))
            .ReturnsAsync((User)null!);

        await Assert.ThrowsAsync<InvalidCredentialsException>(async () =>
            await _authService.LoginAsync(request));
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ThrowsInvalidCredentialsException()
    {
        var request = new LoginRequest("user", "wrongpassword");
        var user = new User(request.UserName, "hash") { IsActive = true };
        _userRepositoryMock.Setup(r => r.GetByNameAsync(request.UserName))
            .ReturnsAsync(user);
        _passwordServiceMock.Setup(p => p.VerifyPassword(request.Password, user.PasswordHash))
            .Returns(false);

        await Assert.ThrowsAsync<InvalidCredentialsException>(async () =>
            await _authService.LoginAsync(request));
    }

    [Fact]
    public async Task LoginAsync_InactiveUser_ThrowsInvalidCredentialsException()
    {
        var request = new LoginRequest("user", "password");
        var user = new User(request.UserName, "hash") { IsActive = false };
        _userRepositoryMock.Setup(r => r.GetByNameAsync(request.UserName))
            .ReturnsAsync(user);
        _passwordServiceMock.Setup(p => p.VerifyPassword(request.Password, user.PasswordHash))
            .Returns(true);

        await Assert.ThrowsAsync<InvalidCredentialsException>(async () =>
            await _authService.LoginAsync(request));
    }

    [Fact]
    public async Task LoginAsync_ValidUser_ReturnsLoginResponse()
    {
        var request = new LoginRequest("user", "password");
        var user = new User(request.UserName, "hash") { Id = 1, IsActive = true };
        _userRepositoryMock.Setup(r => r.GetByNameAsync(request.UserName))
            .ReturnsAsync(user);
        _passwordServiceMock.Setup(p => p.VerifyPassword(request.Password, user.PasswordHash))
            .Returns(true);
        _tokenServiceMock.Setup(t => t.GenerateAccessToken(user)).Returns("access");
        _tokenServiceMock.Setup(t => t.GenerateRefreshToken()).Returns("refresh");
        _tokenServiceMock.Setup(t => t.SaveRefreshTokenAsync("refresh", user)).Returns(Task.CompletedTask);

        var response = await _authService.LoginAsync(request);

        Assert.NotNull(response);
        Assert.Equal("access", response.AccessToken);
        Assert.Equal("refresh", response.RefreshToken);
        Assert.Equal(3600, response.ExpiresIn);
        Assert.Equal(user.Id, response.User.UserId);
        Assert.Equal(user.UserName, response.User.UserName);
    }

    [Fact]
    public async Task LogoutAsync_InvalidToken_ThrowsInvalidCredentialsException()
    {
        var request = new LogoutRequest(_refreshToken);
        _tokenServiceMock.Setup(t => t.ValidateTokenUserAsync(request.RefreshToken, 1))
            .ReturnsAsync(false);

        await Assert.ThrowsAsync<InvalidCredentialsException>(async () =>
            await _authService.LogoutAsync(request, 1));
    }

    [Fact]
    public async Task LogoutAsync_ValidToken_RevokesTokenAndReturnsResponse()
    {
        var request = new LogoutRequest(_refreshToken);
        _tokenServiceMock.Setup(t => t.ValidateTokenUserAsync(request.RefreshToken, 1))
            .ReturnsAsync(true);
        _tokenServiceMock.Setup(t => t.RevokeRefreshTokenAsync(request.RefreshToken))
            .Returns(Task.CompletedTask);

        var response = await _authService.LogoutAsync(request, 1);

        Assert.NotNull(response);
        Assert.Equal("Logout successful", response.Message);
    }

    [Fact]
    public async Task RefreshTokenAsync_InvalidToken_ThrowsInvalidCredentialsException()
    {
        var request = new RefreshRequest(_refreshToken);
        _tokenServiceMock.Setup(t => t.ValidateTokenUserAsync(request.RefreshToken, 1))
            .ReturnsAsync(false);

        await Assert.ThrowsAsync<InvalidCredentialsException>(async () =>
            await _authService.RefreshTokenAsync(request, 1));
    }

    [Fact]
    public async Task RefreshTokenAsync_ValidToken_ReturnsRefreshResponse()
    {
        var request = new RefreshRequest(_refreshToken);
        var user = new User("user", "hash") { Id = 1, IsActive = true };
        var rotatedToken = new RefreshToken { Token = "newrefresh", UserId = 1, User = user };
        _tokenServiceMock.Setup(t => t.ValidateTokenUserAsync(request.RefreshToken, 1))
            .ReturnsAsync(true);
        _tokenServiceMock.Setup(t => t.RotateRefreshTokenAsync(request.RefreshToken, 1))
            .ReturnsAsync(rotatedToken);
        _userRepositoryMock.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(user);
        _tokenServiceMock.Setup(t => t.GenerateAccessToken(user)).Returns("access");

        var response = await _authService.RefreshTokenAsync(request, 1);

        Assert.NotNull(response);
        Assert.Equal("access", response.AccessToken);
        Assert.Equal("newrefresh", response.RefreshToken);
        Assert.Equal(3600, response.ExpiresIn);
    }

    [Fact]
    public async Task ChangePasswordAsync_NewPasswordSameAsOld_ReturnsErrorMessage()
    {
        var request = new ChangePasswordRequest("old", "old", _refreshToken);

        var response = await _authService.ChangePasswordAsync(request, 1);

        Assert.NotNull(response);
        Assert.Equal("New password cannot be the same as the old password", response.Message);
    }

    [Fact]
    public async Task ChangePasswordAsync_InvalidToken_ThrowsInvalidCredentialsException()
    {
        var request = new ChangePasswordRequest("old", "new", _refreshToken);
        _tokenServiceMock.Setup(t => t.ValidateTokenUserAsync(request.RefreshToken, 1))
            .ReturnsAsync(false);

        await Assert.ThrowsAsync<InvalidCredentialsException>(async () =>
            await _authService.ChangePasswordAsync(request, 1));
    }

    [Fact]
    public async Task ChangePasswordAsync_UserNotFound_ThrowsInvalidCredentialsException()
    {
        var request = new ChangePasswordRequest("old", "new", _refreshToken);
        _tokenServiceMock.Setup(t => t.ValidateTokenUserAsync(request.RefreshToken, 1))
            .ReturnsAsync(true);
        _userRepositoryMock.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync((User)null!);

        await Assert.ThrowsAsync<InvalidCredentialsException>(async () =>
            await _authService.ChangePasswordAsync(request, 1));
    }

    [Fact]
    public async Task ChangePasswordAsync_WrongOldPassword_ThrowsInvalidCredentialsException()
    {
        var request = new ChangePasswordRequest("old", "new", _refreshToken);
        var user = new User("user", "hash") { Id = 1, IsActive = true };
        _tokenServiceMock.Setup(t => t.ValidateTokenUserAsync(request.RefreshToken, 1))
            .ReturnsAsync(true);
        _userRepositoryMock.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(user);
        _passwordServiceMock.Setup(p => p.VerifyPassword(request.OldPassword, user.PasswordHash))
            .Returns(false);

        await Assert.ThrowsAsync<InvalidCredentialsException>(async () =>
            await _authService.ChangePasswordAsync(request, 1));
    }

    [Fact]
    public async Task ChangePasswordAsync_ValidRequest_ChangesPasswordAndRevokesTokens()
    {
        var request = new ChangePasswordRequest("old", "new", _refreshToken);
        var user = new User("user", "hash") { Id = 1, IsActive = true };
        _tokenServiceMock.Setup(t => t.ValidateTokenUserAsync(request.RefreshToken, 1))
            .ReturnsAsync(true);
        _userRepositoryMock.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(user);
        _passwordServiceMock.Setup(p => p.VerifyPassword(request.OldPassword, user.PasswordHash))
            .Returns(true);
        _passwordServiceMock.Setup(p => p.HashPassword(request.NewPassword))
            .Returns("newhash");
        _userRepositoryMock.Setup(r => r.UpdateAsync(user))
            .ReturnsAsync(user);
        _tokenServiceMock.Setup(t => t.RevokeRefreshTokenAsync(user.Id))
            .Returns(Task.CompletedTask);

        var response = await _authService.ChangePasswordAsync(request, 1);

        Assert.NotNull(response);
        Assert.Equal("Password changed successfully", response.Message);
        Assert.Equal("newhash", user.PasswordHash);
    }
}