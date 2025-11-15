using CoCity.Api.Middlewares;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Text.Json;

namespace CoCity.Api.Tests.MiddlewareTests;

public class ExceptionHandlingMiddlewareTests
{
    [Fact]
    public async Task Passes_Through_When_No_Exception()
    {
        var context = new DefaultHttpContext();
        var nextCalled = false;
        var middleware = new ExceptionHandlingMiddleware(
            ctx => { nextCalled = true; return Task.CompletedTask; },
            Mock.Of<ILogger<ExceptionHandlingMiddleware>>()
        );

        await middleware.InvokeAsync(context);

        Assert.True(nextCalled);
        Assert.Equal(200, context.Response.StatusCode == 0 ? 200 : context.Response.StatusCode);
    }

    [Fact]
    public async Task Catches_Exception_And_Returns_Json()
    {
        var context = new DefaultHttpContext();
        var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        var logger = new Mock<ILogger<ExceptionHandlingMiddleware>>();
        var middleware = new ExceptionHandlingMiddleware(
            ctx => throw new InvalidOperationException("fail!"),
            logger.Object
        );

        await middleware.InvokeAsync(context);

        responseBody.Seek(0, SeekOrigin.Begin);
        var body = new StreamReader(responseBody).ReadToEnd();
        var json = JsonSerializer.Deserialize<JsonElement>(body);

        Assert.Equal((int)HttpStatusCode.InternalServerError, context.Response.StatusCode);
        Assert.Equal("application/json", context.Response.ContentType);
        Assert.Equal("An unexpected error occurred.", json.GetProperty("message").GetString());
        Assert.Equal("fail!", json.GetProperty("detail").GetString());
        logger.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Unhandled exception occurred")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}