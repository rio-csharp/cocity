namespace CoCity.Api.Tests.Services;

public class PasswordServiceTests
{
    private readonly IPasswordService _passwordService;

    public PasswordServiceTests()
    {
        _passwordService = new PasswordService();
    }

    [Fact]
    public void HashPassword_ShouldReturn_NonEmptyHash()
    {
        var password = "TestPassword123!";
        var hash = _passwordService.HashPassword(password);

        Assert.False(string.IsNullOrWhiteSpace(hash));
        Assert.NotEqual(password, hash);
    }

    [Fact]
    public void VerifyPassword_ShouldReturn_True_For_CorrectPassword()
    {
        var password = "MySecretPassword!";
        var hash = _passwordService.HashPassword(password);

        var result = _passwordService.VerifyPassword(password, hash);

        Assert.True(result);
    }

    [Fact]
    public void VerifyPassword_ShouldReturn_False_For_WrongPassword()
    {
        var password = "CorrectPassword";
        var wrongPassword = "WrongPassword";
        var hash = _passwordService.HashPassword(password);

        var result = _passwordService.VerifyPassword(wrongPassword, hash);

        Assert.False(result);
    }

    [Fact]
    public void HashPassword_SamePassword_ShouldReturn_DifferentHashes()
    {
        var password = "RepeatPassword";
        var hash1 = _passwordService.HashPassword(password);
        var hash2 = _passwordService.HashPassword(password);

        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void VerifyPassword_ShouldReturn_False_For_InvalidHash()
    {
        var password = "AnyPassword";
        var invalidHash = "not_a_valid_hash";

        var result = _passwordService.VerifyPassword(password, invalidHash);

        Assert.False(result);
    }
}