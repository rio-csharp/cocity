namespace CoCity.Api.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(DbContext dbContext) : base(dbContext)
    {
    }

    public Task<User?> GetByNameAsync(string userName)
    {
        return _dbSet.FirstOrDefaultAsync(u => u.UserName == userName);
    }
}