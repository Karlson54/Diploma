using System.IdentityModel.Tokens.Jwt;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TimeTracker.Core.Common;
using TimeTracker.Core.Services.Auth;
using TimeTracker.Data.Entities;

namespace TimeTracker.Tests.Services.Auth;

public class JwtTokenServiceTests
{
    private readonly JwtTokenService _service;
    private readonly JwtSettings _jwtSettings;

    public JwtTokenServiceTests()
    {
        _jwtSettings = new JwtSettings
        {
            SecretKey = "test-super-secret-key-that-is-long-enough-for-hmac256",
            ExpirationMinutes = 60,
            ClockSkew = 5
        };

        _service = new JwtTokenService(Options.Create(_jwtSettings));
    }

    private static User CreateTestUser() => new()
    {
        Id = 1,
        Name = "John Doe",
        Email = "john@example.com",
        Login = "john.doe",
        AgencyId = 1,
        IsActive = true
    };

    [Fact]
    public void GenerateToken_ValidUser_ReturnsNonEmptyToken()
    {
        // Arrange
        var user = CreateTestUser();

        // Act
        var token = _service.GenerateToken(user);

        // Assert
        token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GenerateToken_ValidUser_TokenContainsCorrectClaims()
    {
        // Arrange
        var user = CreateTestUser();

        // Act
        var token = _service.GenerateToken(user);
        var parsed = ParseToken(token);

        // Assert
        parsed.Claims.First(c => c.Type == "userId").Value.Should().Be(user.Id.ToString());
        parsed.Claims.First(c => c.Type == "userName").Value.Should().Be(user.Name);
        parsed.Claims.First(c => c.Type == "Email").Value.Should().Be(user.Email);
        parsed.Claims.First(c => c.Type == "AgencyId").Value.Should().Be(user.AgencyId.ToString());
        parsed.Claims.First(c => c.Type == "IsActive").Value.Should().Be(user.IsActive.ToString());
    }

    [Fact]
    public void GenerateToken_WithRoles_TokenContainsRoleClaims()
    {
        // Arrange
        var user = CreateTestUser();
        var roles = new[] { "Admin", "Manager" };

        // Act
        var token = _service.GenerateToken(user, roles);
        var parsed = ParseToken(token);

        // Assert
        var roleClaims = parsed.Claims
            .Where(c => c.Type == "role")
            .Select(c => c.Value)
            .ToList();

        roleClaims.Should().BeEquivalentTo(roles);
    }

    [Fact]
    public void GenerateToken_WithoutRoles_TokenHasNoRoleClaims()
    {
        // Arrange
        var user = CreateTestUser();

        // Act
        var token = _service.GenerateToken(user);
        var parsed = ParseToken(token);

        // Assert
        parsed.Claims.Where(c => c.Type == "role").Should().BeEmpty();
    }

    [Fact]
    public void GenerateToken_TokenExpiration_MatchesSettings()
    {
        // Arrange
        var user = CreateTestUser();
        var before = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes - 1);
        var after = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes + 1);

        // Act
        var token = _service.GenerateToken(user);
        var parsed = ParseToken(token);

        // Assert
        parsed.ValidTo.Should().BeAfter(before).And.BeBefore(after);
    }

    [Fact]
    public void GenerateToken_TokenIsSignedWithCorrectKey()
    {
        // Arrange
        var user = CreateTestUser();
        var validationParams = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_jwtSettings.SecretKey)),
            ClockSkew = TimeSpan.Zero
        };

        // Act
        var token = _service.GenerateToken(user);
        var handler = new JwtSecurityTokenHandler();
        var act = () => handler.ValidateToken(token, validationParams, out _);

        // Assert
        act.Should().NotThrow();
    }

    // ==================== HELPERS ====================

    private static JwtSecurityToken ParseToken(string token)
    {
        return new JwtSecurityTokenHandler().ReadJwtToken(token);
    }
}