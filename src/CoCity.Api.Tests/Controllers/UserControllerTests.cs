using CoCity.Api.Controllers;
using CoCity.Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CoCity.Api.Tests.Controllers;

public class UserControllerTests
{
    [Fact]
    public async Task GetCurrentUserProfile_ReturnsOk_WhenProfileExists()
    {
        var mockService = new Mock<IUserProfileService>();
        var mockLogger = new Mock<ILogger<UserController>>();
        var controller = new UserController(mockService.Object, mockLogger.Object);

        var expectedProfile = new UserProfileResponseModel("1", "testuser", "Test Nick");
        mockService
            .Setup(s => s.GetCurrentUserProfileAsync(1))
            .ReturnsAsync(expectedProfile);

        var httpContext = new DefaultHttpContext();
        httpContext.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "1") };
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt"));
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        var result = await controller.GetCurrentUserProfile();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var profile = Assert.IsType<UserProfileResponseModel>(okResult.Value);
        Assert.Equal(expectedProfile.UserId, profile.UserId);
        Assert.Equal(expectedProfile.Username, profile.Username);
        Assert.Equal(expectedProfile.NickName, profile.NickName);
    }

    [Fact]
    public async Task GetCurrentUserProfile_ReturnsNotFound_WhenProfileIsNull()
    {
        var mockService = new Mock<IUserProfileService>();
        var mockLogger = new Mock<ILogger<UserController>>();
        var controller = new UserController(mockService.Object, mockLogger.Object);

        mockService
            .Setup(s => s.GetCurrentUserProfileAsync(1))
            .ReturnsAsync((UserProfileResponseModel?)null);

        var httpContext = new DefaultHttpContext();
        httpContext.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "1") };
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt"));
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        var result = await controller.GetCurrentUserProfile();

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetUserProfileById_ReturnsOk_WhenProfileExists()
    {
        var mockService = new Mock<IUserProfileService>();
        var mockLogger = new Mock<ILogger<UserController>>();
        var controller = new UserController(mockService.Object, mockLogger.Object);

        var expectedProfile = new UserProfileResponseModel("2", "otheruser", "Other Nick");
        mockService
            .Setup(s => s.GetUserProfileByIdAsync(1, 2))
            .ReturnsAsync(expectedProfile);

        var httpContext = new DefaultHttpContext();
        httpContext.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "1") };
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt"));
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        var result = await controller.GetUserProfileById(2);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var profile = Assert.IsType<UserProfileResponseModel>(okResult.Value);
        Assert.Equal(expectedProfile.UserId, profile.UserId);
        Assert.Equal(expectedProfile.Username, profile.Username);
        Assert.Equal(expectedProfile.NickName, profile.NickName);
    }

    [Fact]
    public async Task GetUserProfileById_ReturnsNotFound_WhenProfileIsNull()
    {
        var mockService = new Mock<IUserProfileService>();
        var mockLogger = new Mock<ILogger<UserController>>();
        var controller = new UserController(mockService.Object, mockLogger.Object);

        mockService
            .Setup(s => s.GetUserProfileByIdAsync(1, 2))
            .ReturnsAsync((UserProfileResponseModel?)null);

        var httpContext = new DefaultHttpContext();
        httpContext.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "1") };
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt"));
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        var result = await controller.GetUserProfileById(2);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task UpdateCurrentUserProfile_ReturnsOk_WhenUpdateSucceeds()
    {
        var mockService = new Mock<IUserProfileService>();
        var mockLogger = new Mock<ILogger<UserController>>();
        var controller = new UserController(mockService.Object, mockLogger.Object);

        var updateModel = new UpdateUserProfileRequestModel
        {
            NickName = "New Nick",
            AvatarUrl = "http://avatar.url",
            Bio = "New bio",
            Gender = "Other",
            Birthday = "2000-01-01"
        };

        mockService
            .Setup(s => s.UpdateCurrentUserProfileAsync(1, updateModel))
            .ReturnsAsync(true);

        var httpContext = new DefaultHttpContext();
        httpContext.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "1") };
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt"));
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        var result = await controller.UpdateCurrentUserProfile(updateModel);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<UpdateUserProfileResponseModel>(okResult.Value);
        Assert.Equal("Profile updated successfully", response.Message);
    }

    [Fact]
    public async Task UpdateCurrentUserProfile_ReturnsNotFound_WhenUpdateFails()
    {
        var mockService = new Mock<IUserProfileService>();
        var mockLogger = new Mock<ILogger<UserController>>();
        var controller = new UserController(mockService.Object, mockLogger.Object);

        var updateModel = new UpdateUserProfileRequestModel
        {
            NickName = "New Nick",
            AvatarUrl = "http://avatar.url",
            Bio = "New bio",
            Gender = "Other",
            Birthday = "2000-01-01"
        };

        mockService
            .Setup(s => s.UpdateCurrentUserProfileAsync(1, updateModel))
            .ReturnsAsync(false);

        var httpContext = new DefaultHttpContext();
        httpContext.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "1") };
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt"));
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        await Assert.ThrowsAsync<UpdateFailedException>(() => controller.UpdateCurrentUserProfile(updateModel));
    }
}