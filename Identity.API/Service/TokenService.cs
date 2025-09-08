using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Identity.API.Data;
using Identity.API.Data.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Identity.API.Service;

public class TokenService(
    IConfiguration configuration,
    UserManager<AppUser> userManager,
    ApplicationDbContext context
)
{
    public async Task<string> GenerateAccessToken(AppUser user)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.UserName ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.NameIdentifier, user.Id)
        };

        var userRoles = await userManager.GetRolesAsync(user);
        foreach (var role in userRoles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:SecretKey"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddHours(1),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string CreateRefreshToken(AppUser user)
    {
        var token = GenerateRefreshToken();
        context.RefreshTokens.Add(new RefreshToken()
        {
            UserId = user.Id,
            Token = token,
            ExpiryTime = DateTime.Now.AddDays(7),
        });

        context.SaveChanges();
        return token;
    }
    
    public string RefreshRefreshToken(string userId)
    {
        var existedToken = context.RefreshTokens.Where(t => t.UserId == userId);
        if (existedToken.Any())
        {
            context.RefreshTokens.RemoveRange(existedToken);
        }

        var token = GenerateRefreshToken();
        context.RefreshTokens.Add(new RefreshToken()
        {
            UserId = userId,
            Token = token,
            ExpiryTime = DateTime.Now.AddDays(7),
        });

        context.SaveChanges();
        return token;
    }

    private string GenerateRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    }

    public async Task<RefreshToken> RefreshToken(string token)
    {
        var refreshToken = await context.RefreshTokens.FirstOrDefaultAsync(t => t.Token == token);
        if (refreshToken is not null)
        {
            return refreshToken;
        }

        return new();
    }

    public async Task RevokeRefreshToken(string userId)
    {
        var refreshToken = await context.RefreshTokens.FirstOrDefaultAsync(t => t.UserId == userId);
        if (refreshToken is not null)
        {
            context.RefreshTokens.Remove(refreshToken);
            await context.SaveChangesAsync();
        }
    }
}