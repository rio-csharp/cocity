using CoCity.Api.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace CoCity.Api.Tests.Services;

public class TokenServiceTests
{
    private readonly Mock<ITokenRepository> _tokenRepositoryMock;
    private readonly IConfiguration _configuration;
    private readonly TokenService _tokenService;
    private readonly User _user;

    public TokenServiceTests()
    {
        var inMemorySettings = new Dictionary<string, string>
        {
            {"Jwt:SecretKey", "supersecretkey12345678901234567890"},
            {"Jwt:Issuer", "TestIssuer"},
            {"Jwt:Audience", "TestAudience"}
        };
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        _tokenRepositoryMock = new Mock<ITokenRepository>();
        _tokenService = new TokenService(_configuration, _tokenRepositoryMock.Object);
        _user = new User("testuser", "hash") { Id = 1, IsActive = true };
    }

    [Fact]
    public void GenerateAccessToken_Returns_Valid_Jwt()
    {
        var token = _tokenService.GenerateAccessToken(_user);

        var handler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]);
        var parameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = _configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = _configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        handler.ValidateToken(token, parameters, out var validatedToken);
        Assert.NotNull(validatedToken);
    }

    [Fact]
    public void GenerateRefreshToken_Returns_Base64String()
    {
        var token = _tokenService.GenerateRefreshToken();
        var bytes = Convert.FromBase64String(token);
        Assert.Equal(16, bytes.Length); // Guid is 16 bytes
    }

    [Fact]
    public void ValidateAccessToken_ValidToken_ReturnsTrue()
    {
        var token = _tokenService.GenerateAccessToken(_user);
        var result = _tokenService.ValidateAccessToken(token);
        Assert.True(result);
    }

    [Fact]
    public void ValidateAccessToken_InvalidToken_ReturnsFalse()
    {
        var result = _tokenService.ValidateAccessToken("invalid.token.value");
        Assert.False(result);
    }

    [Fact]
    public async Task SaveRefreshTokenAsync_CallsRepositoryAdd()
    {
        var refreshToken = new RefreshToken { Token = "token", UserId = 1 };
        _tokenRepositoryMock.Setup(r => r.AddAsync(refreshToken)).ReturnsAsync(refreshToken);

        await _tokenService.SaveRefreshTokenAsync(refreshToken);

        _tokenRepositoryMock.Verify(r => r.AddAsync(refreshToken), Times.Once);
    }

    [Fact]
    public async Task SaveRefreshTokenAsync_WithUser_CallsRepositoryAdd()
    {
        var user = _user;
        var token = "token";
        _tokenRepositoryMock.Setup(r => r.AddAsync(It.IsAny<RefreshToken>()))
            .ReturnsAsync((RefreshToken t) => t);

        await _tokenService.SaveRefreshTokenAsync(token, user);

        _tokenRepositoryMock.Verify(r => r.AddAsync(It.Is<RefreshToken>(rt => rt.Token == token && rt.UserId == user.Id)), Times.Once);
    }

    [Fact]
    public async Task RevokeRefreshTokenAsync_ByToken_RevokesIfExists()
    {
        var refreshToken = new RefreshToken { Token = "token", UserId = 1, IsRevoked = false };
        _tokenRepositoryMock.Setup(r => r.GetByTokenAsync("token")).ReturnsAsync(refreshToken);
        _tokenRepositoryMock.Setup(r => r.UpdateAsync(refreshToken)).ReturnsAsync(refreshToken);

        await _tokenService.RevokeRefreshTokenAsync("token");

        Assert.True(refreshToken.IsRevoked);
        _tokenRepositoryMock.Verify(r => r.UpdateAsync(refreshToken), Times.Once);
    }

    [Fact]
    public async Task RevokeRefreshTokenAsync_ByToken_DoesNothingIfNotFound()
    {
        _tokenRepositoryMock.Setup(r => r.GetByTokenAsync("token")).ReturnsAsync((RefreshToken)null!);

        await _tokenService.RevokeRefreshTokenAsync("token");

        _tokenRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<RefreshToken>()), Times.Never);
    }

    [Fact]
    public async Task RevokeRefreshTokenAsync_ByRefreshToken_CallsByToken()
    {
        var refreshToken = new RefreshToken { Token = "token", UserId = 1 };
        _tokenRepositoryMock.Setup(r => r.GetByTokenAsync("token")).ReturnsAsync(refreshToken);
        _tokenRepositoryMock.Setup(r => r.UpdateAsync(refreshToken)).ReturnsAsync(refreshToken);

        await _tokenService.RevokeRefreshTokenAsync(refreshToken);

        Assert.True(refreshToken.IsRevoked);
        _tokenRepositoryMock.Verify(r => r.UpdateAsync(refreshToken), Times.Once);
    }

    [Fact]
    public async Task RevokeRefreshTokenAsync_ByUserId_CallsRepository()
    {
        await _tokenService.RevokeRefreshTokenAsync(1);
        _tokenRepositoryMock.Verify(r => r.RevokeAllTokenAsync(1), Times.Once);
    }

    [Fact]
    public async Task RotateRefreshTokenAsync_RevokesOldAndAddsNew()
    {
        var oldToken = "oldtoken";
        var userId = 1;
        _tokenRepositoryMock.Setup(r => r.GetByTokenAsync(oldToken)).ReturnsAsync(new RefreshToken { Token = oldToken, UserId = userId });
        _tokenRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<RefreshToken>())).ReturnsAsync((RefreshToken t) => t);
        _tokenRepositoryMock.Setup(r => r.AddAsync(It.IsAny<RefreshToken>())).ReturnsAsync((RefreshToken t) => t);

        var newToken = await _tokenService.RotateRefreshTokenAsync(oldToken, userId);

        Assert.NotNull(newToken);
        Assert.Equal(userId, newToken.UserId);
        Assert.False(string.IsNullOrWhiteSpace(newToken.Token));
        _tokenRepositoryMock.Verify(r => r.AddAsync(It.Is<RefreshToken>(t => t.UserId == userId)), Times.Once);
    }

    [Fact]
    public async Task ValidateTokenUserAsync_ReturnsFalse_IfTokenIsExpired()
    {
        var refreshToken = new RefreshToken
        {
            Token = "token",
            UserId = 1,
            ExpiresAt = DateTime.UtcNow.AddMinutes(-1), // expired
            IsRevoked = false
        };
        _tokenRepositoryMock.Setup(r => r.GetByTokenAsync("token")).ReturnsAsync(refreshToken);

        var result = await _tokenService.ValidateTokenUserAsync("token", 1);

        Assert.False(result);
    }

    [Fact]
    public async Task ValidateTokenUserAsync_ReturnsFalse_IfTokenIsRevoked()
    {
        var refreshToken = new RefreshToken
        {
            Token = "token",
            UserId = 1,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10), // not expired
            IsRevoked = true // revoked
        };
        _tokenRepositoryMock.Setup(r => r.GetByTokenAsync("token")).ReturnsAsync(refreshToken);

        var result = await _tokenService.ValidateTokenUserAsync("token", 1);

        Assert.False(result);
    }

    [Fact]
    public async Task ValidateTokenUserAsync_ReturnsTrue_IfTokenIsValid_NotExpired_NotRevoked()
    {
        var refreshToken = new RefreshToken
        {
            Token = "token",
            UserId = 1,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10), // not expired
            IsRevoked = false
        };
        _tokenRepositoryMock.Setup(r => r.GetByTokenAsync("token")).ReturnsAsync(refreshToken);

        var result = await _tokenService.ValidateTokenUserAsync("token", 1);

        Assert.True(result);
    }
}