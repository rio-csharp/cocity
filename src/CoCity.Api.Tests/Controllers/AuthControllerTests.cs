using CoCity.Api.Controllers;
using CoCity.Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CoCity.Api.Tests.Controllers;

public class AuthControllerTests
{
    [Fact]
    public async Task Register_ReturnsOK_WithRegisterResponse()
    {
        var mockAuthService = new Mock<IAuthService>();
        var mockLogger = new Mock<ILogger<AuthController>>();
        var mockUserProfileService = new Mock<IUserProfileService>();
        var controller = new AuthController(mockAuthService.Object, mockUserProfileService.Object, mockLogger.Object);

        var registerRequest = new RegisterRequest("testuser", "password123");
        var expectedResponse = new RegisterResponse(1, "testuser");

        mockAuthService
            .Setup(s => s.RegisterAsync(registerRequest, It.IsAny<string>()))
            .ReturnsAsync(expectedResponse);

        var httpContext = new DefaultHttpContext();
        httpContext.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        var result = await controller.Register(registerRequest);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<RegisterResponse>(okResult.Value);
        Assert.Equal(expectedResponse.UserId, response.UserId);
        Assert.Equal(expectedResponse.UserName, response.UserName);
    }

    [Fact]
    public async Task Login_ReturnsOK_WithLoginResponse()
    {
        var mockAuthService = new Mock<IAuthService>();
        var mockLogger = new Mock<ILogger<AuthController>>();
        var mockUserProfileService = new Mock<IUserProfileService>();
        var controller = new AuthController(mockAuthService.Object, mockUserProfileService.Object, mockLogger.Object);

        var loginRequest = new LoginRequest("testuser", "password123");
        var expectedUser = new LoginRequestResponse(1, "testuser");
        var expectedResponse = new LoginResponse("access", "refresh", 3600, expectedUser);

        mockAuthService
            .Setup(s => s.LoginAsync(loginRequest))
            .ReturnsAsync(expectedResponse);

        var httpContext = new DefaultHttpContext();
        httpContext.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        var result = await controller.Login(loginRequest);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<LoginResponse>(okResult.Value);
        Assert.Equal(expectedResponse.AccessToken, response.AccessToken);
        Assert.Equal(expectedResponse.RefreshToken, response.RefreshToken);
        Assert.Equal(expectedResponse.ExpiresIn, response.ExpiresIn);
        Assert.Equal(expectedResponse.User.UserId, response.User.UserId);
        Assert.Equal(expectedResponse.User.UserName, response.User.UserName);
    }

    [Fact]
    public async Task RefreshToken_ReturnsOK_WithRefreshResponse()
    {
        var mockAuthService = new Mock<IAuthService>();
        var mockLogger = new Mock<ILogger<AuthController>>();
        var mockUserProfileService = new Mock<IUserProfileService>();
        var controller = new AuthController(mockAuthService.Object, mockUserProfileService.Object, mockLogger.Object);

        var refreshRequest = new RefreshRequest("refresh_token");
        var expectedResponse = new RefreshResponse("access", "refresh", 3600);

        mockAuthService
            .Setup(s => s.RefreshTokenAsync(refreshRequest, 1))
            .ReturnsAsync(expectedResponse);

        var httpContext = new DefaultHttpContext();
        httpContext.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");
        var claims = new[] { new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub, "1") };
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt"));
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        var result = await controller.RefreshToken(refreshRequest);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<RefreshResponse>(okResult.Value);
        Assert.Equal(expectedResponse.AccessToken, response.AccessToken);
        Assert.Equal(expectedResponse.RefreshToken, response.RefreshToken);
        Assert.Equal(expectedResponse.ExpiresIn, response.ExpiresIn);
    }

    [Fact]
    public async Task Logout_ReturnsOK_WithLogoutResponse()
    {
        var mockAuthService = new Mock<IAuthService>();
        var mockLogger = new Mock<ILogger<AuthController>>();
        var mockUserProfileService = new Mock<IUserProfileService>();
        var controller = new AuthController(mockAuthService.Object, mockUserProfileService.Object, mockLogger.Object);

        var logoutRequest = new LogoutRequest("refresh_token");
        var expectedResponse = new LogoutResponse("Logged out");

        mockAuthService
            .Setup(s => s.LogoutAsync(logoutRequest, 1))
            .ReturnsAsync(expectedResponse);

        var httpContext = new DefaultHttpContext();
        httpContext.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");
        var claims = new[] { new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub, "1") };
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt"));
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        var result = await controller.Logout(logoutRequest);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<LogoutResponse>(okResult.Value);
        Assert.Equal(expectedResponse.Message, response.Message);
    }

    [Fact]
    public async Task ChangePassword_ReturnsOK_WithChangePasswordResponse()
    {
        var mockAuthService = new Mock<IAuthService>();
        var mockLogger = new Mock<ILogger<AuthController>>();
        var mockUserProfileService = new Mock<IUserProfileService>();
        var controller = new AuthController(mockAuthService.Object, mockUserProfileService.Object, mockLogger.Object);

        var changePasswordRequest = new ChangePasswordRequest("oldpwd", "newpwd123", "refresh_token");
        var expectedResponse = new ChangePasswordResponse("Password changed");

        mockAuthService
            .Setup(s => s.ChangePasswordAsync(changePasswordRequest, 1))
            .ReturnsAsync(expectedResponse);

        var httpContext = new DefaultHttpContext();
        httpContext.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");
        var claims = new[] { new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub, "1") };
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt"));
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        var result = await controller.ChangePassword(changePasswordRequest);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ChangePasswordResponse>(okResult.Value);
        Assert.Equal(expectedResponse.Message, response.Message);
    }
}