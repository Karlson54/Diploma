using FluentAssertions;
using TimeTracker.Data.Entities;
using TimeTracker.Data.Repositories.Roles;
using TimeTracker.Tests.Repositories.TestBase;
using Xunit;

namespace TimeTracker.Tests.Repositories.Roles;

public class RoleRepositoryTests : RepositoryTestBase
{
    private readonly RoleRepository _repository;

    public RoleRepositoryTests()
    {
        _repository = new RoleRepository(Context);
    }

    [Theory]
    [InlineData("Admin", 1)]
    [InlineData("MANAGER", 2)] // Регистронезависимость
    [InlineData("NonExistentRole", null)]
    public async Task GetByNameAsync_ShouldReturnCorrectRole(string name, long? expectedId)
    {
        // Act
        var result = await _repository.GetByNameAsync(name);

        // Assert
        if (expectedId.HasValue)
        {
            result.Should().NotBeNull();
            result!.Id.Should().Be(expectedId.Value);
            result.Name.ToLower().Should().Be(name.ToLower());
        }
        else
        {
            result.Should().BeNull();
        }
    }

    [Fact]
    public async Task GetActiveRolesAsync_ShouldReturnOnlyActiveRoles()
    {
        // Act
        var result = await _repository.GetActiveRolesAsync();

        // Assert
        result.Should().HaveCount(3); // Admin, Manager, Employee (Inactive Role исключена)
        result.All(r => r.IsActive).Should().BeTrue();
        result.Should().BeInAscendingOrder(r => r.Name);
    }

    [Fact]
    public async Task GetUserRolesAsync_ShouldReturnUserRoles()
    {
        // Act - получаем роли John'а (у него Admin + Manager)
        var result = await _repository.GetUserRolesAsync(1);

        // Assert
        result.Should().HaveCount(2);
        result.Select(r => r.Name).Should().Contain(new[] { "Admin", "Manager" });
        result.All(r => r.IsActive).Should().BeTrue();
    }

    [Fact]
    public async Task GetUsersInRoleAsync_ShouldReturnUsersWithRole()
    {
        // Act
        var result = await _repository.GetUsersInRoleAsync("Employee");

        // Assert
        result.Should().HaveCount(1); // Только Jane активна и имеет роль Employee
        result.First().Name.Should().Be("Jane Smith");
    }

    [Theory]
    [InlineData(1, "Admin", true)]  // John имеет роль Admin
    [InlineData(1, "Employee", false)] // John не имеет роль Employee
    [InlineData(2, "Employee", true)]  // Jane имеет роль Employee
    [InlineData(999, "Admin", false)] // Несуществующий пользователь
    public async Task UserHasRoleAsync_ShouldReturnCorrectResult(long userId, string roleName, bool expected)
    {
        // Act
        var result = await _repository.UserHasRoleAsync(userId, roleName);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public async Task AssignRoleAsync_ShouldAssignNewRole()
    {
        // Arrange
        const long userId = 2; // Jane
        const long roleId = 2;  // Manager

        // Проверяем, что роли еще нет
        var hasRoleBefore = await _repository.UserHasRoleAsync(userId, "Manager");
        hasRoleBefore.Should().BeFalse();

        // Act
        await _repository.AssignRoleAsync(userId, roleId);
        await Context.SaveChangesAsync();

        // Assert
        var hasRoleAfter = await _repository.UserHasRoleAsync(userId, "Manager");
        hasRoleAfter.Should().BeTrue();
    }

    [Fact]
    public async Task AssignRoleAsync_ShouldNotDuplicateExistingRole()
    {
        // Arrange - John уже имеет роль Admin
        const long userId = 1;
        const long roleId = 1;

        var userRolesCountBefore = await _repository.GetUserRolesAsync(userId);

        // Act - пытаемся назначить роль повторно
        await _repository.AssignRoleAsync(userId, roleId);
        await Context.SaveChangesAsync();

        // Assert - количество ролей не должно измениться
        var userRolesCountAfter = await _repository.GetUserRolesAsync(userId);
        userRolesCountAfter.Should().HaveCount(userRolesCountBefore.Count());
    }

    [Fact]
    public async Task RemoveRoleAsync_ShouldRemoveRole()
    {
        // Arrange - John имеет роль Admin (roleId = 1)
        const long userId = 1;
        const long roleId = 1;

        var hasRoleBefore = await _repository.UserHasRoleAsync(userId, "Admin");
        hasRoleBefore.Should().BeTrue();

        // Act
        await _repository.RemoveRoleAsync(userId, roleId);
        await Context.SaveChangesAsync();

        // Assert
        var hasRoleAfter = await _repository.UserHasRoleAsync(userId, "Admin");
        hasRoleAfter.Should().BeFalse();
    }

    [Fact]
    public async Task ReplaceUserRolesAsync_ShouldReplaceAllRoles()
    {
        // Arrange
        const long userId = 1; // John имеет роли Admin + Manager
        var newRoleIds = new[] { 3L }; // Только Employee

        var rolesBefore = await _repository.GetUserRolesAsync(userId);
        rolesBefore.Should().HaveCount(2);

        // Act
        await _repository.ReplaceUserRolesAsync(userId, newRoleIds);
        await Context.SaveChangesAsync();

        // Assert
        var rolesAfter = await _repository.GetUserRolesAsync(userId);
        rolesAfter.Should().HaveCount(1);
        rolesAfter.First().Name.Should().Be("Employee");
    }

    [Theory]
    [InlineData("Admin", true)]
    [InlineData("ADMIN", true)] // Регистронезависимость
    [InlineData("NewRole", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public async Task IsRoleNameExistsAsync_ShouldReturnCorrectResult(string name, bool expected)
    {
        // Act
        var result = await _repository.IsRoleNameExistsAsync(name);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("Admin", 2, false)] // Имя другой роли, исключаем не ту
    [InlineData("Admin", 1, true)]  // Имя той же роли, исключаем ее
    [InlineData("NewRole", 1, false)] // Новое имя
    public async Task IsRoleNameExistsAsync_WithExclude_ShouldReturnCorrectResult(string name, long excludeRoleId, bool expected)
    {
        // Act
        var result = await _repository.IsRoleNameExistsAsync(name, excludeRoleId);

        // Assert
        result.Should().Be(expected);
    }
}