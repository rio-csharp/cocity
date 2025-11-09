using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CoCity.Api.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly ITokenRepository _tokenRepository;
    public TokenService(IConfiguration configuration, ITokenRepository tokenRepository)
    {
        _configuration = configuration;
        _secretKey = _configuration["Jwt:SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
        _issuer = _configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("JWT Issuer not configured");
        _audience = _configuration["Jwt:Audience"] ?? throw new InvalidOperationException("JWT Audience not configured");
        _tokenRepository = tokenRepository;
    }

    public string GenerateAccessToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim("username", user.UserName),
            // new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        return Convert.ToBase64String(Guid.NewGuid().ToByteArray());
    }

    public bool ValidateAccessToken(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_secretKey);

            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = _audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task SaveRefreshTokenAsync(RefreshToken token)
    {
        await _tokenRepository.AddAsync(token);
    }

    public async Task SaveRefreshTokenAsync(string token, User user)
    {
        var refreshToken = new RefreshToken
        {
            Token = token,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        };
        await SaveRefreshTokenAsync(refreshToken);
    }

    public async Task RevokeRefreshTokenAsync(RefreshToken token)
    {
        await RevokeRefreshTokenAsync(token.Token);
    }

    public async Task RevokeRefreshTokenAsync(string token)
    {
        var refreshToken = await _tokenRepository.GetByTokenAsync(token);
        if (refreshToken is not null)
        {
            refreshToken.IsRevoked = true;
            await _tokenRepository.UpdateAsync(refreshToken);
        }
    }

    public async Task RevokeRefreshTokenAsync(int userId)
    {
        await _tokenRepository.RevokeAllTokenAsync(userId);
    }

    public async Task<RefreshToken> RotateRefreshTokenAsync(string token, int userId)
    {
        await RevokeRefreshTokenAsync(token);
        var newTokenString = GenerateRefreshToken();
        var newRefreshToken = new RefreshToken
        {
            Token = newTokenString,
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        };
        await _tokenRepository.AddAsync(newRefreshToken);
        return newRefreshToken;
    }

    public async Task<bool> ValidateTokenUserAsync(string token, int userId)
    {
        var refreshToken = await _tokenRepository.GetByTokenAsync(token);
        return refreshToken?.UserId == userId
            && refreshToken.ExpiresAt > DateTime.UtcNow
            && !refreshToken.IsRevoked;
    }
}
