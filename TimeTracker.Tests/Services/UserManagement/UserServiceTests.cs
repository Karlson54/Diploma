using BCrypt.Net;
using FluentAssertions;
using MockQueryable.Moq;
using Moq;
using TimeTracker.Core.DTOs.Users;
using TimeTracker.Core.Services.UserManagement;
using TimeTracker.Data.Entities;
using TimeTracker.Data.Repositories.Users;
using TimeTracker.Tests.Services.TestBase;

namespace TimeTracker.Tests.Services.UserManagement;

public class UserServiceTests : ServiceTestBase
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly UserService _service;

    private const string TestIp = "127.0.0.1";
    private const string TestAgent = "xUnit";
    private const long TestUserId = 1;
    private const string TestUserName = "Admin User";
    private const string ValidPassword = "Test@Pass1!";

    public UserServiceTests()
    {
        _userRepoMock = new Mock<IUserRepository>();

        _service = new UserService(
            _userRepoMock.Object,
            UnitOfWorkMock.Object,
            Mapper,
            AuditServiceMock.Object,
            CreateLogger<UserService>().Object);
    }

    // ==================== HELPERS ====================

    private static User CreateUser(long id = 1, bool isActive = true) => new()
    {
        Id = id,
        Name = "John Doe",
        Email = "john@example.com",
        Login = "john.doe",
        AgencyId = 1,
        IsActive = isActive,
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(ValidPassword),
        Agency = new Agency { Id = 1, Name = "MediaCom", IsActive = true },
        UserRoles = new List<UserRole>()
    };

    private static Agency CreateAgency(long id = 1, bool isActive = true) => new()
    {
        Id = id,
        Name = "MediaCom",
        IsActive = isActive
    };

    private static Role CreateRole(long id = 1, string name = "Employee", bool isActive = true) => new()
    {
        Id = id,
        Name = name,
        IsActive = isActive
    };

    private void SetupUsersQuery(IEnumerable<User> users)
    {
        var mock = users.AsQueryable().BuildMock();
        _userRepoMock.Setup(x => x.GetQueryable()).Returns(mock);
    }

    private void SetupRolesQuery(IEnumerable<Role> roles)
    {
        var mock = roles.AsQueryable().BuildMock();
        UnitOfWorkMock.Setup(x => x.Roles.GetQueryable()).Returns(mock);
    }

    // ==================== GET TESTS ====================

    [Fact]
    public async Task GetByIdAsync_ExistingUser_ReturnsDto()
    {
        // Arrange
        var user = CreateUser();
        _userRepoMock.Setup(x => x.GetByIdWithRolesAsync(1)).ReturnsAsync(user);

        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Email.Should().Be("john@example.com");
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentUser_ReturnsNull()
    {
        // Arrange
        _userRepoMock.Setup(x => x.GetByIdWithRolesAsync(999)).ReturnsAsync((User?)null);

        // Act
        var result = await _service.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    // ==================== CREATE TESTS ====================

    [Fact]
    public async Task CreateAsync_ValidData_ReturnsCreatedUser()
    {
        // Arrange
        var dto = new CreateUserDto
        {
            Login = "new.user",
            Email = "new@example.com",
            Password = ValidPassword,
            ConfirmPassword = ValidPassword,
            Name = "New User",
            AgencyId = 1,
            RoleId = new List<long> { 1 }
        };

        var agency = CreateAgency();
        var role = CreateRole();

        // Юзер який буде збережений — Id = 0 після AddAsync в тесті
        var savedUser = new User
        {
            Id = 0,
            Login = dto.Login,
            Email = dto.Email,
            Name = dto.Name,
            AgencyId = dto.AgencyId,
            IsActive = true,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Agency = agency,
            UserRoles = new List<UserRole>()
        };

        _userRepoMock.Setup(x => x.IsEmailExistsAsync(dto.Email)).ReturnsAsync(false);
        _userRepoMock.Setup(x => x.IsLoginExistsAsync(dto.Login)).ReturnsAsync(false);
        UnitOfWorkMock.Setup(x => x.Agencies.GetByIdAsync(dto.AgencyId)).ReturnsAsync(agency);
        SetupRolesQuery(new[] { role });
        _userRepoMock.Setup(x => x.AddAsync(It.IsAny<User>())).ReturnsAsync(savedUser);
        UnitOfWorkMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        // Фінальний GetQueryable після збереження — повертаємо юзера з Id = 0
        SetupUsersQuery(new[] { savedUser });

        // Act
        var result = await _service.CreateAsync(dto, TestUserId, TestUserName, TestIp, TestAgent);

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be(dto.Email);
        result.Login.Should().Be(dto.Login);
        _userRepoMock.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Once);
        UnitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_DuplicateEmail_ThrowsInvalidOperation()
    {
        // Arrange
        var dto = new CreateUserDto
        {
            Login = "new.user",
            Email = "existing@example.com",
            Password = ValidPassword,
            ConfirmPassword = ValidPassword,
            Name = "New User",
            AgencyId = 1
        };

        _userRepoMock.Setup(x => x.IsEmailExistsAsync(dto.Email)).ReturnsAsync(true);

        // Act
        var act = () => _service.CreateAsync(dto, TestUserId, TestUserName, TestIp, TestAgent);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Email*");
    }

    [Fact]
    public async Task CreateAsync_DuplicateLogin_ThrowsInvalidOperation()
    {
        // Arrange
        var dto = new CreateUserDto
        {
            Login = "existing.login",
            Email = "new@example.com",
            Password = ValidPassword,
            ConfirmPassword = ValidPassword,
            Name = "New User",
            AgencyId = 1
        };

        _userRepoMock.Setup(x => x.IsEmailExistsAsync(dto.Email)).ReturnsAsync(false);
        _userRepoMock.Setup(x => x.IsLoginExistsAsync(dto.Login)).ReturnsAsync(true);

        // Act
        var act = () => _service.CreateAsync(dto, TestUserId, TestUserName, TestIp, TestAgent);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Login*");
    }

    [Fact]
    public async Task CreateAsync_NonExistentAgency_ThrowsKeyNotFound()
    {
        // Arrange
        var dto = new CreateUserDto
        {
            Login = "new.user",
            Email = "new@example.com",
            Password = ValidPassword,
            ConfirmPassword = ValidPassword,
            Name = "New User",
            AgencyId = 999
        };

        _userRepoMock.Setup(x => x.IsEmailExistsAsync(dto.Email)).ReturnsAsync(false);
        _userRepoMock.Setup(x => x.IsLoginExistsAsync(dto.Login)).ReturnsAsync(false);
        UnitOfWorkMock.Setup(x => x.Agencies.GetByIdAsync(999)).ReturnsAsync((Agency?)null);

        // Act
        var act = () => _service.CreateAsync(dto, TestUserId, TestUserName, TestIp, TestAgent);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*Agency*");
    }

    [Fact]
    public async Task CreateAsync_InactiveAgency_ThrowsInvalidOperation()
    {
        // Arrange
        var dto = new CreateUserDto
        {
            Login = "new.user",
            Email = "new@example.com",
            Password = ValidPassword,
            ConfirmPassword = ValidPassword,
            Name = "New User",
            AgencyId = 1
        };

        _userRepoMock.Setup(x => x.IsEmailExistsAsync(dto.Email)).ReturnsAsync(false);
        _userRepoMock.Setup(x => x.IsLoginExistsAsync(dto.Login)).ReturnsAsync(false);
        UnitOfWorkMock.Setup(x => x.Agencies.GetByIdAsync(1)).ReturnsAsync(CreateAgency(isActive: false));

        // Act
        var act = () => _service.CreateAsync(dto, TestUserId, TestUserName, TestIp, TestAgent);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*неактивного Agency*");
    }

    // ==================== UPDATE TESTS ====================

    [Fact]
    public async Task UpdateAsync_ValidData_ReturnsUpdatedUser()
    {
        // Arrange
        var existing = CreateUser();
        var dto = new UpdateUserDto
        {
            Email = "updated@example.com",
            Name = "Updated Name",
            AgencyId = 1
        };

        _userRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(existing);
        _userRepoMock.Setup(x => x.IsEmailExistsAsync(dto.Email, 1)).ReturnsAsync(false);
        UnitOfWorkMock.Setup(x => x.Agencies.GetByIdAsync(dto.AgencyId))
            .ReturnsAsync(CreateAgency());
        UnitOfWorkMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);
        SetupUsersQuery(new[] { existing });

        // Act
        var result = await _service.UpdateAsync(1, dto, TestUserId, TestUserName, TestIp, TestAgent);

        // Assert
        result.Should().NotBeNull();
        UnitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_NonExistentUser_ThrowsKeyNotFound()
    {
        // Arrange
        _userRepoMock.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((User?)null);
        var dto = new UpdateUserDto { Email = "x@x.com", Name = "X", AgencyId = 1 };

        // Act
        var act = () => _service.UpdateAsync(999, dto, TestUserId, TestUserName, TestIp, TestAgent);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*999*");
    }

    [Fact]
    public async Task UpdateAsync_DuplicateEmail_ThrowsInvalidOperation()
    {
        // Arrange
        var existing = CreateUser();
        var dto = new UpdateUserDto { Email = "taken@example.com", Name = "Name", AgencyId = 1 };

        _userRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(existing);
        _userRepoMock.Setup(x => x.IsEmailExistsAsync(dto.Email, 1)).ReturnsAsync(true);

        // Act
        var act = () => _service.UpdateAsync(1, dto, TestUserId, TestUserName, TestIp, TestAgent);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Email*");
    }

    // ==================== ACTIVATE / DEACTIVATE TESTS ====================

    [Fact]
    public async Task ActivateAsync_InactiveUser_ActivatesSuccessfully()
    {
        // Arrange
        var user = CreateUser(isActive: false);
        _userRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(user);
        UnitOfWorkMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        await _service.ActivateAsync(1, TestUserId, TestUserName, TestIp, TestAgent);

        // Assert
        user.IsActive.Should().BeTrue();
        UnitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task ActivateAsync_AlreadyActiveUser_ThrowsInvalidOperation()
    {
        // Arrange
        var user = CreateUser(isActive: true);
        _userRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(user);

        // Act
        var act = () => _service.ActivateAsync(1, TestUserId, TestUserName, TestIp, TestAgent);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*вже активний*");
    }

    [Fact]
    public async Task DeactivateAsync_ActiveUser_DeactivatesSuccessfully()
    {
        // Arrange
        var user = CreateUser(isActive: true);
        _userRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(user);
        UnitOfWorkMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        await _service.DeactivateAsync(1, TestUserId, TestUserName, TestIp, TestAgent);

        // Assert
        user.IsActive.Should().BeFalse();
        UnitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeactivateAsync_AlreadyInactiveUser_ThrowsInvalidOperation()
    {
        // Arrange
        var user = CreateUser(isActive: false);
        _userRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(user);

        // Act
        var act = () => _service.DeactivateAsync(1, TestUserId, TestUserName, TestIp, TestAgent);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*вже деактивований*");
    }

    [Fact]
    public async Task ActivateAsync_NonExistentUser_ThrowsKeyNotFound()
    {
        // Arrange
        _userRepoMock.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((User?)null);

        // Act
        var act = () => _service.ActivateAsync(999, TestUserId, TestUserName, TestIp, TestAgent);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    // ==================== CHANGE PASSWORD TESTS ====================

    [Fact]
    public async Task ChangePasswordAsync_ValidData_ChangesPassword()
    {
        // Arrange
        var user = CreateUser();
        var dto = new ChangePasswordDto
        {
            CurrentPassword = ValidPassword,
            NewPassword = "New@Pass1!",
            ConfirmPassword = "New@Pass1!"
        };

        _userRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(user);
        UnitOfWorkMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        await _service.ChangePasswordAsync(1, dto, TestUserId, TestUserName, TestIp, TestAgent);

        // Assert
        UnitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        BCrypt.Net.BCrypt.Verify("New@Pass1!", user.PasswordHash).Should().BeTrue();
    }

    [Fact]
    public async Task ChangePasswordAsync_WrongCurrentPassword_ThrowsUnauthorized()
    {
        // Arrange
        var user = CreateUser();
        var dto = new ChangePasswordDto
        {
            CurrentPassword = "Wrong@Pass1!",
            NewPassword = "New@Pass1!",
            ConfirmPassword = "New@Pass1!"
        };

        _userRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(user);

        // Act
        var act = () => _service.ChangePasswordAsync(1, dto, TestUserId, TestUserName, TestIp, TestAgent);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*невірний*");
    }

    [Fact]
    public async Task ChangePasswordAsync_SamePassword_ThrowsInvalidOperation()
    {
        // Arrange
        var user = CreateUser();
        var dto = new ChangePasswordDto
        {
            CurrentPassword = ValidPassword,
            NewPassword = ValidPassword,
            ConfirmPassword = ValidPassword
        };

        _userRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(user);

        // Act
        var act = () => _service.ChangePasswordAsync(1, dto, TestUserId, TestUserName, TestIp, TestAgent);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*співпадати*");
    }

    [Fact]
    public async Task ChangePasswordAsync_InactiveUser_ThrowsInvalidOperation()
    {
        // Arrange
        var user = CreateUser(isActive: false);
        var dto = new ChangePasswordDto
        {
            CurrentPassword = ValidPassword,
            NewPassword = "New@Pass1!",
            ConfirmPassword = "New@Pass1!"
        };

        _userRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(user);

        // Act
        var act = () => _service.ChangePasswordAsync(1, dto, TestUserId, TestUserName, TestIp, TestAgent);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*неактивного*");
    }

    // ==================== IS EXISTS TESTS ====================

    [Fact]
    public async Task IsEmailExistsAsync_ExistingEmail_ReturnsTrue()
    {
        // Arrange
        _userRepoMock.Setup(x => x.IsEmailExistsAsync("john@example.com")).ReturnsAsync(true);

        // Act
        var result = await _service.IsEmailExistsAsync("john@example.com");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsLoginExistsAsync_ExistingLogin_ReturnsTrue()
    {
        // Arrange
        _userRepoMock.Setup(x => x.IsLoginExistsAsync("john.doe")).ReturnsAsync(true);

        // Act
        var result = await _service.IsLoginExistsAsync("john.doe");

        // Assert
        result.Should().BeTrue();
    }
}