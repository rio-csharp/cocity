namespace CoCity.Api.IRepositories;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByNameAsync(string userName);
}
