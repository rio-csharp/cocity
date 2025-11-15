namespace CoCity.Api.Services;

public class UserProfileService : IUserProfileService
{
    private readonly IUserProfileRepository _repository;

    public UserProfileService(IUserProfileRepository repository)
    {
        _repository = repository;
    }

    private static UserProfileResponseModel? MapToResponseModel(UserProfile? entity)
    {
        if (entity == null) return null;

        return new UserProfileResponseModel(
            userId: entity.UserId.ToString(),
            username: entity.UserName,
            nickName: entity.NickName,
            avatarUrl: entity.AvatarUrl,
            bio: entity.Bio,
            gender: entity.Gender,
            birthday: entity.Birthday?.ToString("yyyy-MM-dd")
        );
    }

    public async Task<UserProfileResponseModel> CreateUserProfileAsync(int userId, string userName)
    {
        var profile = new UserProfile(userId, userName, userName);

        await _repository.AddAsync(profile);
        return new UserProfileResponseModel(
            userId.ToString(),
            userName,
            profile.NickName ?? "",
            profile.AvatarUrl,
            profile.Bio,
            profile.Gender,
            profile.Birthday?.ToString("yyyy-MM-dd")
        );
    }

    public async Task<UserProfileResponseModel?> GetCurrentUserProfileAsync(int currentUserId)
    {
        var entity = await _repository.GetByUserIdAsync(currentUserId);
        return MapToResponseModel(entity);
    }

    public async Task<UserProfileResponseModel?> GetUserProfileByIdAsync(int currentUserId, int userId)
    {
        var entity = await _repository.GetByUserIdAsync(userId);
        return MapToResponseModel(entity);
    }

    public async Task<bool> UpdateCurrentUserProfileAsync(int currentUserId, UpdateUserProfileRequestModel updateModel)
    {
        var entity = await _repository.GetByUserIdAsync(currentUserId);
        if (entity == null) return false;

        if (updateModel.NickName != null)
            entity.NickName = updateModel.NickName;
        if (updateModel.AvatarUrl != null)
            entity.AvatarUrl = updateModel.AvatarUrl;
        if (updateModel.Bio != null)
            entity.Bio = updateModel.Bio;
        if (updateModel.Gender != null)
            entity.Gender = updateModel.Gender;

        if (updateModel.Birthday != null && DateOnly.TryParse(updateModel.Birthday, out var birthday))
            entity.Birthday = birthday;

        await _repository.UpdateAsync(entity);
        return true;
    }
}