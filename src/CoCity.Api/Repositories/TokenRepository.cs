
namespace CoCity.Api.Repositories;

public class TokenRepository : Repository<RefreshToken>, ITokenRepository
{
    public TokenRepository(DbContext dbContext) : base(dbContext)
    {
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token)
    {
        return await _dbSet.FirstOrDefaultAsync(rt => rt.Token == token && rt.IsRevoked == false);
    }

    public async Task RevokeAllTokenAsync(int userId)
    {
        var tokens = await _dbSet.Where(rt => rt.UserId == userId && rt.IsRevoked == false).ToListAsync();
        foreach (var token in tokens)
        {
            token.IsRevoked = true;
        }
        await _dbContext.SaveChangesAsync();
    }
}
