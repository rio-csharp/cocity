using CoCity.Api.Entities;
using CoCity.Api.Models;

namespace CoCity.Api.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IPasswordService> _passwordServiceMock;
    private readonly Mock<ILogger<AuthService>> _loggerMock;
    private readonly AuthService _authService;
    private readonly string _refreshToken = "Mnhroz8srEyMcea76KAsSg==";

    public AuthServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _passwordServiceMock = new Mock<IPasswordService>();
        _loggerMock = new Mock<ILogger<AuthService>>();
        _authService = new AuthService(_loggerMock.Object, _userRepositoryMock.Object, _passwordServiceMock.Object);
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
}