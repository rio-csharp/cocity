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

        if ((origin != null && origin.StartsWith("http://localhost")) || host == "localhost")
        {
            if (string.IsNullOrWhiteSpace(_requiredToken))
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsync("Localhost access is not allowed.");
                return;
            }
            if (!context.Request.Headers.TryGetValue(LocalhostTokenHeader, out var token) || token != _requiredToken)
            {
                _logger.LogWarning("Unauthorized localhost access attempt from {Host}, the wrong token is [{Token}]", context.GetClientIp(), token);
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Missing or invalid localhost token.");
                return;
            }
        }

        await _next(context);
    }
}