namespace CoCity.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _userService;
    private readonly ILogger<AuthController> _logger;
    private readonly IUserProfileService _userProfileService;

    public AuthController(IAuthService userService, IUserProfileService userProfileService, ILogger<AuthController> logger)
    {
        _userService = userService;
        _userProfileService = userProfileService;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<ActionResult<RegisterResponse>> Register([FromBody] RegisterRequest registerRequest)
    {
        var clientIp = HttpContext.GetClientIp();
        _logger.LogInformation("Register request from IP {ClientIp}, Username: {UserName}", clientIp, registerRequest.UserName);

        var registerResponse = await _userService.RegisterAsync(registerRequest, clientIp);

        _logger.LogInformation("Creating user profile for UserId {UserId}", registerResponse.UserId);
        try
        {
            await _userProfileService.CreateUserProfileAsync(registerResponse.UserId, registerRequest.UserName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create user profile for UserId {UserId}", registerResponse.UserId);
            await _userService.DeleteUserAsync(registerResponse.UserId);
            return StatusCode(500, "User registration failed during profile creation. Please try again later.");
        }

        return Ok(registerResponse);
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest loginRequest)
    {
        var clientIp = HttpContext.GetClientIp();
        _logger.LogInformation("Login request from IP {ClientIp}, Username: {UserName}", clientIp, loginRequest.UserName);

        var loginResponse = await _userService.LoginAsync(loginRequest);
        return Ok(loginResponse);
    }

    [Authorize]
    [HttpPost("refresh")]
    public async Task<ActionResult<RefreshResponse>> RefreshToken([FromBody] RefreshRequest refreshRequest)
    {
        var clientIp = HttpContext.GetClientIp();
        _logger.LogInformation("Refresh token request from IP {ClientIp}", clientIp);

        var refreshResponse = await _userService.RefreshTokenAsync(refreshRequest, this.GetUserId());
        return Ok(refreshResponse);
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<ActionResult<LogoutResponse>> Logout([FromBody] LogoutRequest logoutRequest)
    {
        var clientIp = HttpContext.GetClientIp();
        _logger.LogInformation("Logout request from IP {ClientIp}", clientIp);

        var logoutResponse = await _userService.LogoutAsync(logoutRequest, this.GetUserId());
        return Ok(logoutResponse);
    }

    [Authorize]
    [HttpPost("changepwd")]
    public async Task<ActionResult<ChangePasswordResponse>> ChangePassword([FromBody] ChangePasswordRequest changePasswordRequest)
    {
        var clientIp = HttpContext.GetClientIp();
        _logger.LogInformation("Change password request from IP {ClientIp}", clientIp);

        var changePasswordResponse = await _userService.ChangePasswordAsync(changePasswordRequest, this.GetUserId());
        return Ok(changePasswordResponse);
    }
}