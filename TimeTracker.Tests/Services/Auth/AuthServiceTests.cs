using BCrypt.Net;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using TimeTracker.Core.Common;
using TimeTracker.Core.DTOs.Auth;
using TimeTracker.Core.Services.Audit;
using TimeTracker.Core.Services.Auth;
using TimeTracker.Data.Entities;
using TimeTracker.Data.Repositories.Roles;
using TimeTracker.Data.Repositories.Users;
using TimeTracker.Data.UnitOfWork;
using TimeTracker.Tests.Services.TestBase;

namespace TimeTracker.Tests.Services.Auth;

public class AuthServiceTests : ServiceTestBase
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IRoleRepository> _roleRepoMock;
    private readonly Mock<IJwtTokenService> _jwtServiceMock;
    private readonly AuthService _service;

    // Валидный пароль соответствующий всем требованиям AuthService
    private const string ValidPassword = "Test@Pass1!";
    private const string TestIp = "127.0.0.1";
    private const string TestAgent = "xUnit";

    public AuthServiceTests()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _roleRepoMock = new Mock<IRoleRepository>();
        _jwtServiceMock = new Mock<IJwtTokenService>();

        _jwtServiceMock
            .Setup(x => x.GenerateToken(It.IsAny<User>(), It.IsAny<IEnumerable<string>?>()))
            .Returns("mocked-jwt-token");

        var jwtSettings = Options.Create(new JwtSettings
        {
            SecretKey = "test-secret-key-long-enough-for-tests",
            ExpirationMinutes = 60,
            ClockSkew = 5
        });

        _service = new AuthService(
            _userRepoMock.Object,
            _roleRepoMock.Object,
            UnitOfWorkMock.Object,
            _jwtServiceMock.Object,
            AuditServiceMock.Object,
            Mapper,
            CreateLogger<AuthService>().Object,
            jwtSettings);
    }

    // ==================== HELPERS ====================

    private static User CreateActiveUser(long id = 1, string? passwordHash = null) => new()
    {
        Id = id,
        Name = "John Doe",
        Email = "john@example.com",
        Login = "john.doe",
        AgencyId = 1,
        IsActive = true,
        PasswordHash = passwordHash ?? BCrypt.Net.BCrypt.HashPassword(ValidPassword),
        Agency = new Agency { Id = 1, Name = "MediaCom", IsActive = true },
        UserRoles = new List<UserRole>
        {
            new() { RoleId = 1, Role = new Role { Id = 1, Name = "Employee", IsActive = true } }
        }
    };

    private static User CreateInactiveUser() => new()
    {
        Id = 2,
        Name = "Inactive User",
        Email = "inactive@example.com",
        Login = "inactive",
        AgencyId = 1,
        IsActive = false,
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(ValidPassword)
    };

    // ==================== LOGIN TESTS ====================

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsAuthResponse()
    {
        // Arrange
        var user = CreateActiveUser();
        _userRepoMock.Setup(x => x.GetByEmailAsync(user.Email)).ReturnsAsync(user);
        _userRepoMock.Setup(x => x.GetByIdWithRolesAsync(user.Id)).ReturnsAsync(user);

        var dto = new LoginDto { LoginOrEmail = user.Email, Password = ValidPassword };

        // Act
        var result = await _service.LoginAsync(dto, TestIp, TestAgent);

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be(user.Email);
        result.Token.Should().Be("mocked-jwt-token");
        result.Roles.Should().Contain("Employee");
    }

    [Fact]
    public async Task LoginAsync_NonExistentUser_ThrowsUnauthorized()
    {
        // Arrange
        _userRepoMock.Setup(x => x.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
        _userRepoMock.Setup(x => x.GetByLoginAsync(It.IsAny<string>())).ReturnsAsync((User?)null);

        var dto = new LoginDto { LoginOrEmail = "nobody@example.com", Password = ValidPassword };

        // Act
        var act = () => _service.LoginAsync(dto, TestIp, TestAgent);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*логін/email або пароль*");
    }

    [Fact]
    public async Task LoginAsync_InactiveUser_ThrowsUnauthorized()
    {
        // Arrange
        var user = CreateInactiveUser();
        _userRepoMock.Setup(x => x.GetByEmailAsync(user.Email)).ReturnsAsync(user);

        var dto = new LoginDto { LoginOrEmail = user.Email, Password = ValidPassword };

        // Act
        var act = () => _service.LoginAsync(dto, TestIp, TestAgent);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*деактивовано*");
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ThrowsUnauthorized()
    {
        // Arrange
        var user = CreateActiveUser();
        _userRepoMock.Setup(x => x.GetByEmailAsync(user.Email)).ReturnsAsync(user);

        var dto = new LoginDto { LoginOrEmail = user.Email, Password = "WrongPass@1!" };

        // Act
        var act = () => _service.LoginAsync(dto, TestIp, TestAgent);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*логін/email або пароль*");
    }

    [Fact]
    public async Task LoginAsync_ByLogin_ReturnsAuthResponse()
    {
        // Arrange
        var user = CreateActiveUser();
        _userRepoMock.Setup(x => x.GetByEmailAsync(user.Login)).ReturnsAsync((User?)null);
        _userRepoMock.Setup(x => x.GetByLoginAsync(user.Login)).ReturnsAsync(user);
        _userRepoMock.Setup(x => x.GetByIdWithRolesAsync(user.Id)).ReturnsAsync(user);

        var dto = new LoginDto { LoginOrEmail = user.Login, Password = ValidPassword };

        // Act
        var result = await _service.LoginAsync(dto, TestIp, TestAgent);

        // Assert
        result.Should().NotBeNull();
        result.Login.Should().Be(user.Login);
    }

    [Fact]
    public async Task LoginAsync_UserWithNoActiveRoles_ThrowsUnauthorized()
    {
        // Arrange
        var user = CreateActiveUser();
        user.UserRoles = new List<UserRole>
        {
            new() { RoleId = 1, Role = new Role { Id = 1, Name = "Employee", IsActive = false } }
        };
        _userRepoMock.Setup(x => x.GetByEmailAsync(user.Email)).ReturnsAsync(user);
        _userRepoMock.Setup(x => x.GetByIdWithRolesAsync(user.Id)).ReturnsAsync(user);

        var dto = new LoginDto { LoginOrEmail = user.Email, Password = ValidPassword };

        // Act
        var act = () => _service.LoginAsync(dto, TestIp, TestAgent);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*активних ролей*");
    }

    // ==================== VALIDATE PASSWORD TESTS ====================

    [Fact]
    public async Task ValidatePasswordAsync_CorrectPassword_ReturnsTrue()
    {
        // Arrange
        var user = CreateActiveUser();
        _userRepoMock.Setup(x => x.GetByIdAsync(user.Id)).ReturnsAsync(user);

        // Act
        var result = await _service.ValidatePasswordAsync(user.Id, ValidPassword);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ValidatePasswordAsync_WrongPassword_ReturnsFalse()
    {
        // Arrange
        var user = CreateActiveUser();
        _userRepoMock.Setup(x => x.GetByIdAsync(user.Id)).ReturnsAsync(user);

        // Act
        var result = await _service.ValidatePasswordAsync(user.Id, "WrongPass@1!");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ValidatePasswordAsync_NonExistentUser_ReturnsFalse()
    {
        // Arrange
        _userRepoMock.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((User?)null);

        // Act
        var result = await _service.ValidatePasswordAsync(999, ValidPassword);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ValidatePasswordAsync_InactiveUser_ReturnsFalse()
    {
        // Arrange
        var user = CreateInactiveUser();
        _userRepoMock.Setup(x => x.GetByIdAsync(user.Id)).ReturnsAsync(user);

        // Act
        var result = await _service.ValidatePasswordAsync(user.Id, ValidPassword);

        // Assert
        result.Should().BeFalse();
    }
}