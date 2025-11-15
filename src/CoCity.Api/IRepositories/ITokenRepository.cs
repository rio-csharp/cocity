namespace CoCity.Api.IRepositories;

public interface ITokenRepository : IRepository<RefreshToken>
{
    Task<RefreshToken?> GetByTokenAsync(string token);

    Task RevokeAllTokenAsync(int userId);
}