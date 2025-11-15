namespace CoCity.Api.Repositories;

public class UserProfileRepository : Repository<UserProfile>, IUserProfileRepository
{
    public UserProfileRepository(DbContext dbContext) : base(dbContext)
    {
    }

    public Task<UserProfile?> GetByUserIdAsync(int userId)
    {
        return _dbSet.FirstOrDefaultAsync(up => up.UserId == userId);
    }
}