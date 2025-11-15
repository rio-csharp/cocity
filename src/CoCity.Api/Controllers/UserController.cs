using CoCity.Api.Exceptions;

namespace CoCity.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserProfileService _service;
    private readonly ILogger<UserController> _logger;

    public UserController(IUserProfileService service, ILogger<UserController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet("profile")]
    public async Task<ActionResult<UserProfileResponseModel>> GetCurrentUserProfile()
    {
        var userId = this.GetUserId();
        _logger.LogInformation("Fetching profile for user {UserId} from IP {ClientIp}", userId, HttpContext.GetClientIp());

        var profile = await _service.GetCurrentUserProfileAsync(userId);
        if (profile == null) return NotFound();
        return Ok(profile);
    }

    [HttpGet("profile/{userId:int}")]
    public async Task<ActionResult<UserProfileResponseModel>> GetUserProfileById(int userId)
    {
        var currentUserId = this.GetUserId();
        _logger.LogInformation("User {CurrentUserId} from IP {ClientIp} is fetching profile for user {UserId}", currentUserId, HttpContext.GetClientIp(), userId);

        var profile = await _service.GetUserProfileByIdAsync(currentUserId, userId);
        if (profile == null) return NotFound();
        return Ok(profile);
    }

    [HttpPut("profile")]
    public async Task<ActionResult<UpdateUserProfileResponseModel>> UpdateCurrentUserProfile([FromBody] UpdateUserProfileRequestModel updateModel)
    {
        var userId = this.GetUserId();
        _logger.LogInformation("User {UserId} from IP {ClientIp} is updating their profile", userId, HttpContext.GetClientIp());

        var updated = await _service.UpdateCurrentUserProfileAsync(userId, updateModel);
        if (!updated) throw new UpdateFailedException("user does not exist or birthday format is not correct. Expected format: YYYY-MM-DD.");

        return Ok(new UpdateUserProfileResponseModel("Profile updated successfully"));
    }
}