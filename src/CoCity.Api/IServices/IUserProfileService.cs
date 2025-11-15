namespace CoCity.Api.IServices;

public interface IUserProfileService
{
    Task<UserProfileResponseModel> CreateUserProfileAsync(int userId, string userName);

    Task<UserProfileResponseModel?> GetCurrentUserProfileAsync(int currentUserId);

    Task<UserProfileResponseModel?> GetUserProfileByIdAsync(int currentUserId, int TargetUserId);

    Task<bool> UpdateCurrentUserProfileAsync(int currentUserId, UpdateUserProfileRequestModel updateModel);
}