using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TimeTracker.Core.Common;
using TimeTracker.Data.Entities;

namespace TimeTracker.Core.Services.Auth;

public class JwtTokenService : IJwtTokenService
{
    private readonly JwtSettings _jwtSettings;

    public JwtTokenService(IOptions<JwtSettings> jwtSettings)
    {
        _jwtSettings = jwtSettings.Value;
    }

    public string GenerateToken(User user, IEnumerable<string>? roles = null)
    {
        var claims = new List<Claim>
        {
            new Claim("userId", user.Id.ToString()),
            new Claim("userName", user.Name),
            new Claim("AgencyId", user.AgencyId.ToString()),
            new Claim("Email", user.Email),
            new Claim("IsActive", user.IsActive.ToString())
        };

        if (roles != null)
        {
            claims.AddRange(roles.Select(role => new Claim("role", role)));
        }

        var jwtToken = new JwtSecurityToken(
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
            claims: claims,
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(_jwtSettings.SecretKey)),
                SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(jwtToken);
    }
}