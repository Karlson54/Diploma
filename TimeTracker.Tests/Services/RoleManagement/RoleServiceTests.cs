using FluentAssertions;
using Moq;
using TimeTracker.Core.DTOs.Roles;
using TimeTracker.Core.Services.RoleManagement;
using TimeTracker.Data.Entities;
using TimeTracker.Data.Repositories.Roles;
using TimeTracker.Data.Repositories.Users;
using TimeTracker.Tests.Services.TestBase;

namespace TimeTracker.Tests.Services.RoleManagement;

public class RoleServiceTests : ServiceTestBase
{
    private readonly Mock<IRoleRepository> _roleRepoMock;
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly RoleService _service;

    private const string TestIp = "127.0.0.1";
    private const string TestAgent = "xUnit";
    private const long TestUserId = 1;

    public RoleServiceTests()
    {
        _roleRepoMock = new Mock<IRoleRepository>();
        _userRepoMock = new Mock<IUserRepository>();

        _service = new RoleService(
            _roleRepoMock.Object,
            _userRepoMock.Object,
            UnitOfWorkMock.Object,
            Mapper,
            CreateLogger<RoleService>().Object,
            AuditServiceMock.Object);
    }

    // ==================== HELPERS ====================

    private static User CreateUser(long id = 1) => new()
    {
        Id = id,
        Name = "Admin User",
        Email = "admin@example.com",
        Login = "admin",
        AgencyId = 1,
        IsActive = true,
        UserRoles = new List<UserRole>()
    };

    private static Role CreateRole(long id = 1, string name = "CustomRole", bool isActive = true) => new()
    {
        Id = id,
        Name = name,
        Description = "Test role",
        IsActive = isActive
    };

    // ==================== GET TESTS ====================

    [Fact]
    public async Task GetByIdAsync_ExistingRole_ReturnsDto()
    {
        // Arrange
        var role = CreateRole();
        _roleRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(role);
        _roleRepoMock.Setup(x => x.GetUsersInRoleAsync(role.Name))
            .ReturnsAsync(Array.Empty<User>());

        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Name.Should().Be("CustomRole");
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentRole_ReturnsNull()
    {
        // Arrange
        _roleRepoMock.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((Role?)null);

        // Act
        var result = await _service.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    // ==================== CREATE TESTS ====================

    [Fact]
    public async Task CreateAsync_ValidData_ReturnsCreatedRole()
    {
        // Arrange
        var dto = new CreateRoleDto { Name = "NewRole", Description = "New role desc" };
        var requestingUser = CreateUser();
        var createdRole = CreateRole(1, dto.Name);

        _userRepoMock.Setup(x => x.GetByIdAsync(TestUserId)).ReturnsAsync(requestingUser);
        _roleRepoMock.Setup(x => x.IsRoleNameExistsAsync(dto.Name)).ReturnsAsync(false);
        _roleRepoMock.Setup(x => x.AddAsync(It.IsAny<Role>())).ReturnsAsync(createdRole);
        UnitOfWorkMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _service.CreateAsync(dto, TestUserId, TestIp, TestAgent);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(dto.Name);
        _roleRepoMock.Verify(x => x.AddAsync(It.IsAny<Role>()), Times.Once);
        UnitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_DuplicateName_ThrowsInvalidOperation()
    {
        // Arrange
        var dto = new CreateRoleDto { Name = "ExistingRole" };
        _userRepoMock.Setup(x => x.GetByIdAsync(TestUserId)).ReturnsAsync(CreateUser());
        _roleRepoMock.Setup(x => x.IsRoleNameExistsAsync(dto.Name)).ReturnsAsync(true);

        // Act
        var act = () => _service.CreateAsync(dto, TestUserId, TestIp, TestAgent);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*існує*");
    }

    [Fact]
    public async Task CreateAsync_InvalidRequestingUser_ThrowsUnauthorized()
    {
        // Arrange
        var dto = new CreateRoleDto { Name = "NewRole" };
        _userRepoMock.Setup(x => x.GetByIdAsync(TestUserId)).ReturnsAsync((User?)null);

        // Act
        var act = () => _service.CreateAsync(dto, TestUserId, TestIp, TestAgent);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    // ==================== UPDATE TESTS ====================

    [Fact]
    public async Task UpdateAsync_ValidData_ReturnsUpdatedRole()
    {
        // Arrange
        var existing = CreateRole(1, "CustomRole");
        var dto = new UpdateRoleDto { Name = "UpdatedRole", Description = "Updated" };
        var requestingUser = CreateUser();

        _userRepoMock.Setup(x => x.GetByIdAsync(TestUserId)).ReturnsAsync(requestingUser);
        _roleRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(existing);
        _roleRepoMock.Setup(x => x.IsRoleNameExistsAsync(dto.Name, 1)).ReturnsAsync(false);
        UnitOfWorkMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _service.UpdateAsync(1, dto, TestUserId, TestIp, TestAgent);

        // Assert
        result.Should().NotBeNull();
        UnitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_NonExistentRole_ThrowsKeyNotFound()
    {
        // Arrange
        _userRepoMock.Setup(x => x.GetByIdAsync(TestUserId)).ReturnsAsync(CreateUser());
        _roleRepoMock.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((Role?)null);
        var dto = new UpdateRoleDto { Name = "SomeName" };

        // Act
        var act = () => _service.UpdateAsync(999, dto, TestUserId, TestIp, TestAgent);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*999*");
    }

    [Fact]
    public async Task UpdateAsync_SystemRoleNameChange_ThrowsInvalidOperation()
    {
        // Arrange
        var systemRole = CreateRole(1, "Admin"); // системна роль
        var dto = new UpdateRoleDto { Name = "NotAdmin" };
        _userRepoMock.Setup(x => x.GetByIdAsync(TestUserId)).ReturnsAsync(CreateUser());
        _roleRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(systemRole);

        // Act
        var act = () => _service.UpdateAsync(1, dto, TestUserId, TestIp, TestAgent);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*системн*");
    }

    [Fact]
    public async Task UpdateAsync_DuplicateName_ThrowsInvalidOperation()
    {
        // Arrange
        var existing = CreateRole(1, "CustomRole");
        var dto = new UpdateRoleDto { Name = "TakenName" };
        _userRepoMock.Setup(x => x.GetByIdAsync(TestUserId)).ReturnsAsync(CreateUser());
        _roleRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(existing);
        _roleRepoMock.Setup(x => x.IsRoleNameExistsAsync(dto.Name, 1)).ReturnsAsync(true);

        // Act
        var act = () => _service.UpdateAsync(1, dto, TestUserId, TestIp, TestAgent);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*існує*");
    }

    // ==================== DELETE TESTS ====================

    [Fact]
    public async Task DeleteAsync_CustomRoleWithNoUsers_DeletesSuccessfully()
    {
        // Arrange
        var role = CreateRole(1, "CustomRole");
        _userRepoMock.Setup(x => x.GetByIdAsync(TestUserId)).ReturnsAsync(CreateUser());
        _roleRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(role);
        _roleRepoMock.Setup(x => x.GetUsersInRoleAsync(role.Name))
            .ReturnsAsync(Array.Empty<User>());
        UnitOfWorkMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        await _service.DeleteAsync(1, TestUserId, TestIp, TestAgent);

        // Assert
        _roleRepoMock.Verify(x => x.Delete(role), Times.Once);
        UnitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_SystemRole_ThrowsInvalidOperation()
    {
        // Arrange
        var systemRole = CreateRole(1, "Admin");
        _userRepoMock.Setup(x => x.GetByIdAsync(TestUserId)).ReturnsAsync(CreateUser());
        _roleRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(systemRole);
        _roleRepoMock.Setup(x => x.GetUsersInRoleAsync(systemRole.Name))
            .ReturnsAsync(Array.Empty<User>());

        // Act
        var act = () => _service.DeleteAsync(1, TestUserId, TestIp, TestAgent);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*системн*");
    }

    [Fact]
    public async Task DeleteAsync_RoleWithUsers_ThrowsInvalidOperation()
    {
        // Arrange
        var role = CreateRole(1, "CustomRole");
        _userRepoMock.Setup(x => x.GetByIdAsync(TestUserId)).ReturnsAsync(CreateUser());
        _roleRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(role);
        _roleRepoMock.Setup(x => x.GetUsersInRoleAsync(role.Name))
            .ReturnsAsync(new[] { CreateUser(2) });

        // Act
        var act = () => _service.DeleteAsync(1, TestUserId, TestIp, TestAgent);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*призначена*");
    }

    // ==================== ACTIVATE / DEACTIVATE TESTS ====================

    [Fact]
    public async Task ActivateAsync_InactiveRole_ActivatesSuccessfully()
    {
        // Arrange
        var role = CreateRole(1, "CustomRole", isActive: false);
        _userRepoMock.Setup(x => x.GetByIdAsync(TestUserId)).ReturnsAsync(CreateUser());
        _roleRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(role);
        UnitOfWorkMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        await _service.ActivateAsync(1, TestUserId, TestIp, TestAgent);

        // Assert
        role.IsActive.Should().BeTrue();
        UnitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task ActivateAsync_AlreadyActiveRole_ThrowsInvalidOperation()
    {
        // Arrange
        var role = CreateRole(1, "CustomRole", isActive: true);
        _userRepoMock.Setup(x => x.GetByIdAsync(TestUserId)).ReturnsAsync(CreateUser());
        _roleRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(role);

        // Act
        var act = () => _service.ActivateAsync(1, TestUserId, TestIp, TestAgent);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*вже активна*");
    }

    [Fact]
    public async Task DeactivateAsync_ActiveRole_DeactivatesSuccessfully()
    {
        // Arrange
        var role = CreateRole(1, "CustomRole", isActive: true);
        _userRepoMock.Setup(x => x.GetByIdAsync(TestUserId)).ReturnsAsync(CreateUser());
        _roleRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(role);
        _roleRepoMock.Setup(x => x.GetUsersInRoleAsync(role.Name))
            .ReturnsAsync(Array.Empty<User>());
        UnitOfWorkMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        await _service.DeactivateAsync(1, TestUserId, TestIp, TestAgent);

        // Assert
        role.IsActive.Should().BeFalse();
        UnitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeactivateAsync_RoleWithActiveUsers_ThrowsInvalidOperation()
    {
        // Arrange
        var role = CreateRole(1, "CustomRole", isActive: true);
        _userRepoMock.Setup(x => x.GetByIdAsync(TestUserId)).ReturnsAsync(CreateUser());
        _roleRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(role);
        _roleRepoMock.Setup(x => x.GetUsersInRoleAsync(role.Name))
            .ReturnsAsync(new[] { CreateUser(2) });

        // Act
        var act = () => _service.DeactivateAsync(1, TestUserId, TestIp, TestAgent);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    // ==================== ASSIGN / REMOVE ROLE TESTS ====================

    [Fact]
    public async Task AssignRoleToUserAsync_ValidData_AssignsSuccessfully()
    {
        // Arrange
        var user = CreateUser(2);
        var role = CreateRole(1, "CustomRole");
        _userRepoMock.Setup(x => x.GetByIdAsync(TestUserId)).ReturnsAsync(CreateUser());
        _userRepoMock.Setup(x => x.GetByIdAsync(2)).ReturnsAsync(user);
        _roleRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(role);
        _roleRepoMock.Setup(x => x.UserHasRoleAsync(2, role.Name)).ReturnsAsync(false);
        _roleRepoMock.Setup(x => x.AssignRoleAsync(2, 1)).Returns(Task.CompletedTask);
        UnitOfWorkMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        await _service.AssignRoleToUserAsync(2, 1, TestUserId, TestIp, TestAgent);

        // Assert
        _roleRepoMock.Verify(x => x.AssignRoleAsync(2, 1), Times.Once);
        UnitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task AssignRoleToUserAsync_AlreadyAssigned_ThrowsInvalidOperation()
    {
        // Arrange
        var user = CreateUser(2);
        var role = CreateRole(1, "CustomRole");
        _userRepoMock.Setup(x => x.GetByIdAsync(TestUserId)).ReturnsAsync(CreateUser());
        _userRepoMock.Setup(x => x.GetByIdAsync(2)).ReturnsAsync(user);
        _roleRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(role);
        _roleRepoMock.Setup(x => x.UserHasRoleAsync(2, role.Name)).ReturnsAsync(true);

        // Act
        var act = () => _service.AssignRoleToUserAsync(2, 1, TestUserId, TestIp, TestAgent);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*вже призначена*");
    }

    [Fact]
    public async Task RemoveRoleFromUserAsync_ValidData_RemovesSuccessfully()
    {
        // Arrange
        var user = CreateUser(2);
        var role = CreateRole(1, "CustomRole");
        _userRepoMock.Setup(x => x.GetByIdAsync(TestUserId)).ReturnsAsync(CreateUser());
        _userRepoMock.Setup(x => x.GetByIdAsync(2)).ReturnsAsync(user);
        _roleRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(role);
        _roleRepoMock.Setup(x => x.UserHasRoleAsync(2, role.Name)).ReturnsAsync(true);
        _roleRepoMock.Setup(x => x.RemoveRoleAsync(2, 1)).Returns(Task.CompletedTask);
        UnitOfWorkMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        await _service.RemoveRoleFromUserAsync(2, 1, TestUserId, TestIp, TestAgent);

        // Assert
        _roleRepoMock.Verify(x => x.RemoveRoleAsync(2, 1), Times.Once);
        UnitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    // ==================== CAN DELETE TESTS ====================

    [Fact]
    public async Task CanDeleteRoleAsync_SystemRole_ReturnsFalse()
    {
        // Arrange
        var systemRole = CreateRole(1, "Admin");
        _roleRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(systemRole);

        // Act
        var result = await _service.CanDeleteRoleAsync(1);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CanDeleteRoleAsync_CustomRoleWithNoUsers_ReturnsTrue()
    {
        // Arrange
        var role = CreateRole(1, "CustomRole");
        _roleRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(role);
        _roleRepoMock.Setup(x => x.GetUsersInRoleAsync(role.Name))
            .ReturnsAsync(Array.Empty<User>());

        // Act
        var result = await _service.CanDeleteRoleAsync(1);

        // Assert
        result.Should().BeTrue();
    }
}