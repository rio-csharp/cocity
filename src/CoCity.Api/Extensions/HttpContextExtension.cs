namespace CoCity.Api.Extensions;

public static class HttpContextExtension
{
    public static string GetClientIp(this HttpContext httpContext)
    {
        return httpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
    }
}
