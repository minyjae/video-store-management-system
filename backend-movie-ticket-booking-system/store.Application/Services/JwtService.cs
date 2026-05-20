using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using store.Application.Interfaces;
using store.Domain.Enums;

namespace store.Application.Services;

public class JwtService : IJwtService
{
    public string GenerateToken(Guid userId, string username, UserRole role)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_SECRET")!));

        var token = new JwtSecurityToken(
            claims: [
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Name, username),
                new Claim("role", role.ToString())
            ],
            expires:  TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Asia/Bangkok").AddDays(7),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}