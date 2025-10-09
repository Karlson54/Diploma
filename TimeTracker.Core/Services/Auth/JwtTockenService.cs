using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper.Configuration.Annotations;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TimeTracker.Core.Common;
using TimeTracker.Data.Entities;

namespace TimeTracker.Core.Services.Auth;

public class JwtTokenService : IJwtTokenService
{
    private readonly JwtSettings _jwtSettings;
    // private readonly JwtBearerOptionsSetup _bearerOptionsSetup;

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

    // public ClaimsPrincipal? GetClaimsFromToken(string token)
    // {
    //     if (string.IsNullOrWhiteSpace(token))
    //         return null;
    //
    //     try
    //     {
    //         var tokenHandler = new JwtSecurityTokenHandler();
    //         var principal = tokenHandler.ValidateToken(
    //             token,
    //             _tokenValidationParameters,
    //             out var validatedToken);
    //     }
    //     catch (Exception e)
    //     {
    //         Console.WriteLine(e);
    //         throw;
    //     }
    // }
}