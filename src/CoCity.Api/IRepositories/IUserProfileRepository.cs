namespace CoCity.Api.IRepositories;

public interface IUserProfileRepository : IRepository<UserProfile>
{
    Task<UserProfile?> GetByUserIdAsync(int userId);
}