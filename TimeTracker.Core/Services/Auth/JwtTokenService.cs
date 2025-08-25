using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TimeTracker.Core.Common;
using TimeTracker.Data.Entities;

namespace TimeTracker.Core.Services.Auth;

public class JwtTokenService : IJwtTokenService
{
    private readonly JwtSettings _jwtSettings;
    private readonly JwtSecurityTokenHandler _tokenHandler;
    private readonly TokenValidationParameters _tokenValidationParameters;

    public JwtTokenService(IOptions<JwtSettings> jwtSettings)
    {
        _jwtSettings = jwtSettings.Value ?? throw new ArgumentNullException(nameof(jwtSettings));
        _tokenHandler = new JwtSecurityTokenHandler();
        
        // Настройка параметров валидации токена
        _tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey)),
            ValidateIssuer = true,
            ValidIssuer = _jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = _jwtSettings.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(_jwtSettings.ClockSkew),
            RequireExpirationTime = true
        };
    }

    public async Task<string> GenerateAccessTokenAsync(User user, IEnumerable<Role> roles)
    {
        if (user == null) throw new ArgumentNullException(nameof(user));
        
        var claims = await BuildClaimsAsync(user, roles);
        
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey)),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = _tokenHandler.CreateToken(tokenDescriptor);
        return _tokenHandler.WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public ClaimsPrincipal? GetPrincipalFromToken(string token, bool validateLifetime = true)
    {
        if (string.IsNullOrWhiteSpace(token))
            return null;

        try
        {
            var validationParameters = _tokenValidationParameters.Clone();
            validationParameters.ValidateLifetime = validateLifetime;

            var principal = _tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
            
            // Дополнительная проверка алгоритма подписи
            if (validatedToken is not JwtSecurityToken jwtToken ||
                !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            return principal;
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> ValidateTokenAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return false;

        var principal = GetPrincipalFromToken(token);
        return await Task.FromResult(principal != null);
    }

    public async Task<IEnumerable<Claim>> GetClaimsFromTokenAsync(string token)
    {
        var principal = GetPrincipalFromToken(token, validateLifetime: false);
        return await Task.FromResult(principal?.Claims ?? Enumerable.Empty<Claim>());
    }

    public async Task<long> GetUserIdFromTokenAsync(string token)
    {
        var claims = await GetClaimsFromTokenAsync(token);
        var userIdClaim = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        
        return long.TryParse(userIdClaim?.Value, out var userId) ? userId : 0;
    }

    public async Task<bool> IsTokenExpiredAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return true;

        try
        {
            var jwtToken = _tokenHandler.ReadJwtToken(token);
            return await Task.FromResult(jwtToken.ValidTo < DateTime.UtcNow);
        }
        catch
        {
            return true;
        }
    }

    #region Private Methods

    private async Task<List<Claim>> BuildClaimsAsync(User user, IEnumerable<Role> roles)
    {
        var claims = new List<Claim>
        {
            // Стандартные JWT claims
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Name),
            new(ClaimTypes.Email, user.Email),
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, 
                new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), 
                ClaimValueTypes.Integer64),
            
            // Кастомные claims для бизнес-логики
            new("user_login", user.Login),
            new("agency_id", user.AgencyId.ToString()),
            new("is_active", user.IsActive.ToString().ToLower())
        };

        // Добавляем роли
        var rolesList = roles.ToList();
        foreach (var role in rolesList)
        {
            claims.Add(new Claim(ClaimTypes.Role, role.Name));
            
            // Добавляем permissions из роли (если есть)
            if (!string.IsNullOrWhiteSpace(role.Permissions))
            {
                // Предполагаем, что permissions хранятся как JSON array строк
                try
                {
                    var permissions = System.Text.Json.JsonSerializer
                        .Deserialize<string[]>(role.Permissions);
                    
                    if (permissions != null)
                    {
                        foreach (var permission in permissions)
                        {
                            claims.Add(new Claim("permission", permission));
                        }
                    }
                }
                catch
                {
                    // Логируем ошибку парсинга permissions
                }
            }
        }

        return await Task.FromResult(claims);
    }

    #endregion
}