using CoCity.Api.Middlewares;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace CoCity.Api.Tests.MiddlewareTests;
public class LocalhostAuthMiddlewareTests
{
    private static DefaultHttpContext CreateContext(string host, string origin = null, string token = null)
    {
        var context = new DefaultHttpContext();
        context.Request.Host = new HostString(host);
        if (origin != null)
            context.Request.Headers["Origin"] = origin;
        if (token != null)
            context.Request.Headers["X-Localhost-Token"] = token;
        context.Response.Body = new MemoryStream();
        return context;
    }

    [Fact]
    public async Task Allows_All_When_Host_Is_Localhost()
    {
        var context = CreateContext("localhost", "http://example.com");
        var nextCalled = false;
        var middleware = new LocalhostAuthMiddleware(
            ctx => { nextCalled = true; return Task.CompletedTask; },
            new ConfigurationBuilder().Build(),
            Mock.Of<ILogger<LocalhostAuthMiddleware>>()
        );

        await middleware.InvokeAsync(context);

        Assert.True(nextCalled);
        Assert.Equal(200, context.Response.StatusCode == 0 ? 200 : context.Response.StatusCode);
    }

    [Fact]
    public async Task Allows_All_When_Host_Is_127_0_0_1()
    {
        var context = CreateContext("127.0.0.1", "http://example.com");
        var nextCalled = false;
        var middleware = new LocalhostAuthMiddleware(
            ctx => { nextCalled = true; return Task.CompletedTask; },
            new ConfigurationBuilder().Build(),
            Mock.Of<ILogger<LocalhostAuthMiddleware>>()
        );

        await middleware.InvokeAsync(context);

        Assert.True(nextCalled);
        Assert.Equal(200, context.Response.StatusCode == 0 ? 200 : context.Response.StatusCode);
    }

    [Fact]
    public async Task Allows_All_When_Host_Is_IPv6_Localhost()
    {
        var context = CreateContext("::1", "http://example.com");
        var nextCalled = false;
        var middleware = new LocalhostAuthMiddleware(
            ctx => { nextCalled = true; return Task.CompletedTask; },
            new ConfigurationBuilder().Build(),
            Mock.Of<ILogger<LocalhostAuthMiddleware>>()
        );

        await middleware.InvokeAsync(context);

        Assert.True(nextCalled);
        Assert.Equal(200, context.Response.StatusCode == 0 ? 200 : context.Response.StatusCode);
    }

    [Fact]
    public async Task Allows_NonLocalhost_Origin_When_Origin_Is_Not_Localhost()
    {
        var context = CreateContext("api.example.com", "http://example.com");
        var nextCalled = false;
        var middleware = new LocalhostAuthMiddleware(
            ctx => { nextCalled = true; return Task.CompletedTask; },
            new ConfigurationBuilder().Build(),
            Mock.Of<ILogger<LocalhostAuthMiddleware>>()
        );

        await middleware.InvokeAsync(context);

        Assert.True(nextCalled);
        Assert.Equal(200, context.Response.StatusCode == 0 ? 200 : context.Response.StatusCode);
    }

    [Fact]
    public async Task Denies_Localhost_Origin_If_Token_Not_Configured()
    {
        var context = CreateContext("api.example.com", "http://localhost");
        var middleware = new LocalhostAuthMiddleware(
            ctx => Task.CompletedTask,
            new ConfigurationBuilder().Build(),
            Mock.Of<ILogger<LocalhostAuthMiddleware>>()
        );

        await middleware.InvokeAsync(context);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = new StreamReader(context.Response.Body).ReadToEnd();
        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        Assert.Contains("Localhost access is not allowed", body);
    }

    [Fact]
    public async Task Denies_Localhost_Origin_If_Token_Missing()
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(
            new[] { new KeyValuePair<string, string?>("LocalhostToken", "expected") }
        ).Build();

        var context = CreateContext("api.example.com", "http://localhost");
        var logger = new Mock<ILogger<LocalhostAuthMiddleware>>();
        var middleware = new LocalhostAuthMiddleware(
            ctx => Task.CompletedTask,
            config,
            logger.Object
        );

        await middleware.InvokeAsync(context);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = new StreamReader(context.Response.Body).ReadToEnd();
        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
        Assert.Contains("Missing or invalid localhost token", body);
    }

    [Fact]
    public async Task Denies_Localhost_Origin_If_Token_Invalid()
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(
            new[] { new KeyValuePair<string, string?>("LocalhostToken", "expected") }
        ).Build();

        var context = CreateContext("api.example.com", "http://localhost", "wrong");
        var logger = new Mock<ILogger<LocalhostAuthMiddleware>>();
        var middleware = new LocalhostAuthMiddleware(
            ctx => Task.CompletedTask,
            config,
            logger.Object
        );

        await middleware.InvokeAsync(context);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = new StreamReader(context.Response.Body).ReadToEnd();
        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
        Assert.Contains("Missing or invalid localhost token", body);
    }

    [Fact]
    public async Task Allows_Localhost_Origin_With_Valid_Token()
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(
            new[] { new KeyValuePair<string, string?>("LocalhostToken", "expected") }
        ).Build();

        var context = CreateContext("api.example.com", "http://localhost", "expected");
        var nextCalled = false;
        var middleware = new LocalhostAuthMiddleware(
            ctx => { nextCalled = true; return Task.CompletedTask; },
            config,
            Mock.Of<ILogger<LocalhostAuthMiddleware>>()
        );

        await middleware.InvokeAsync(context);

        Assert.True(nextCalled);
        Assert.Equal(200, context.Response.StatusCode == 0 ? 200 : context.Response.StatusCode);
    }
}
