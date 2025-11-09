namespace CoCity.Api.Middlewares;

public class LocalhostAuthMiddleware
{
    private readonly RequestDelegate _next;
    private const string LocalhostTokenHeader = "X-Localhost-Token";
    private readonly string _requiredToken;
    private readonly ILogger<LocalhostAuthMiddleware> _logger;

    public LocalhostAuthMiddleware(RequestDelegate next, IConfiguration configuration, ILogger<LocalhostAuthMiddleware> logger)
    {
        _next = next;
        _logger = logger;
        _requiredToken = configuration.GetValue<string>("LocalhostToken") ?? string.Empty;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var origin = context.Request.Headers["Origin"].FirstOrDefault();
        var host = context.Request.Host.Host;

        if (IsLocalhost(host))
        {
            await _next(context);
            return;
        }

        if ((origin != null && IsLocalhostOrigin(origin)))
        {
            if (string.IsNullOrWhiteSpace(_requiredToken))
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsync("Localhost access is not allowed.");
                return;
            }
            if (!context.Request.Headers.TryGetValue(LocalhostTokenHeader, out var token) || token != _requiredToken)
            {
                _logger.LogWarning("Unauthorized localhost access attempt from {Client}, the wrong token is [{Token}]", context.GetClientIp(), token);
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Missing or invalid localhost token.");
                return;
            }
        }

        await _next(context);
    }

    private bool IsLocalhostOrigin(string origin)
    {
        if (string.IsNullOrEmpty(origin))
            return false;
        try
        {
            var uri = new Uri(origin);
            return IsLocalhost(uri.Host);
        }
        catch
        {
            return false;
        }
    }

    private bool IsLocalhost(string host)
    {
        return host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
            || host.Equals("127.0.0.1")
            || host.Equals("::1");
    }
}