namespace CoCity.Api.Extensions;

public static class ControllerExtension
{
    public static int GetUserId(this ControllerBase controller)
    {
        var userIdString = controller.User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value
            ?? controller.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdString, out int userId))
        {
            return userId;
        }
        return 0;
    }
}