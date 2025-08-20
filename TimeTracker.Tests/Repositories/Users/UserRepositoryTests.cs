using FluentAssertions;
using TimeTracker.Data.Repositories.Users;
using TimeTracker.Tests.Repositories.TestBase;
using Xunit;

namespace TimeTracker.Tests.Repositories.Users;

public class UserRepositoryTests : RepositoryTestBase
{
    private readonly UserRepository _repository;

    public UserRepositoryTests()
    {
        _repository = new UserRepository(Context);
    }

    [Theory]
    [InlineData("john@example.com", 1)]
    [InlineData("JANE@EXAMPLE.COM", 2)] // Проверка регистронезависимости
    [InlineData("nonexistent@example.com", null)]
    public async Task GetByEmailAsync_ShouldReturnCorrectUser(string email, long? expectedId)
    {
        // Act
        var result = await _repository.GetByEmailAsync(email);

        // Assert
        if (expectedId.HasValue)
        {
            result.Should().NotBeNull();
            result!.Id.Should().Be(expectedId.Value);
            result.Email.ToLower().Should().Be(email.ToLower());
        }
        else
        {
            result.Should().BeNull();
        }
    }

    [Theory]
    [InlineData("john.doe", 1)]
    [InlineData("JANE.SMITH", 2)] // Проверка регистронезависимости
    [InlineData("nonexistent", null)]
    public async Task GetByLoginAsync_ShouldReturnCorrectUser(string login, long? expectedId)
    {
        // Act
        var result = await _repository.GetByLoginAsync(login);

        // Assert
        if (expectedId.HasValue)
        {
            result.Should().NotBeNull();
            result!.Id.Should().Be(expectedId.Value);
            result.Login.ToLower().Should().Be(login.ToLower());
        }
        else
        {
            result.Should().BeNull();
        }
    }

    [Fact]
    public async Task GetByEmailWithRolesAsync_ShouldIncludeRoles()
    {
        // Act
        var result = await _repository.GetByEmailWithRolesAsync("john@example.com");

        // Assert
        result.Should().NotBeNull();
        result!.UserRoles.Should().NotBeEmpty();
        result.UserRoles.Should().HaveCount(2); // Admin и Manager
        result.UserRoles.All(ur => ur.Role != null).Should().BeTrue();
    }

    [Fact]
    public async Task GetActiveUsersAsync_ShouldReturnOnlyActiveUsers()
    {
        // Act
        var result = await _repository.GetActiveUsersAsync();

        // Assert
        result.Should().NotBeEmpty();
        result.Should().HaveCount(2); // John и Jane активны, Bob неактивен
        result.All(u => u.IsActive).Should().BeTrue();
        result.Should().BeInAscendingOrder(u => u.Name);
    }

    [Theory]
    [InlineData(1, 2)] // MediaCom Ukraine - John и Jane
    [InlineData(2, 1)] // Mindshare - только Bob, но он неактивен, поэтому 0
    [InlineData(999, 0)] // Несуществующее агентство
    public async Task GetUsersByAgencyAsync_ShouldReturnCorrectCount(long agencyId, int expectedCount)
    {
        // Act
        var result = await _repository.GetUsersByAgencyAsync(agencyId);

        // Assert
        result.Should().HaveCount(expectedCount);
        result.All(u => u.AgencyId == agencyId && u.IsActive).Should().BeTrue();
    }

    [Theory]
    [InlineData("Admin", 1)] // Только John
    [InlineData("Employee", 1)] // Jane активна, Bob неактивен
    [InlineData("Manager", 1)] // Только John
    [InlineData("NonExistentRole", 0)]
    public async Task GetUsersWithRoleAsync_ShouldReturnCorrectUsers(string roleName, int expectedCount)
    {
        // Act
        var result = await _repository.GetUsersWithRoleAsync(roleName);

        // Assert
        result.Should().HaveCount(expectedCount);
    }

    [Theory]
    [InlineData("john@example.com", true)]
    [InlineData("JOHN@EXAMPLE.COM", true)] // Регистронезависимость
    [InlineData("newuser@example.com", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public async Task IsEmailExistsAsync_ShouldReturnCorrectResult(string email, bool expected)
    {
        // Act
        var result = await _repository.IsEmailExistsAsync(email);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("john.doe", true)]
    [InlineData("JOHN.DOE", true)] // Регистронезависимость  
    [InlineData("newlogin", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public async Task IsLoginExistsAsync_ShouldReturnCorrectResult(string login, bool expected)
    {
        // Act
        var result = await _repository.IsLoginExistsAsync(login);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("john@example.com", 2, false)] // Email другого пользователя, исключаем не того
    [InlineData("john@example.com", 1, true)]  // Email того же пользователя, исключаем его
    [InlineData("newemail@example.com", 1, false)] // Новый email
    public async Task IsEmailExistsAsync_WithExclude_ShouldReturnCorrectResult(string email, long excludeUserId, bool expected)
    {
        // Act
        var result = await _repository.IsEmailExistsAsync(email, excludeUserId);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public async Task GetActiveUsersCountAsync_ShouldReturnCorrectCount()
    {
        // Act
        var result = await _repository.GetActiveUsersCountAsync();

        // Assert
        result.Should().Be(2); // John и Jane
    }

    [Fact]
    public async Task GetUsersPagedAsync_ShouldReturnPagedResults()
    {
        // Act
        var result = await _repository.GetUsersPagedAsync(1, 1); // Первая страница, размер 1

        // Assert
        result.Users.Should().HaveCount(1);
        result.TotalCount.Should().Be(3); // Всего 3 пользователя
        result.Users.First().Name.Should().Be("Bob Johnson"); // Первый по алфавиту
    }

    [Fact]
    public async Task GetUsersPagedAsync_WithSearch_ShouldFilterResults()
    {
        // Act
        var result = await _repository.GetUsersPagedAsync(1, 10, searchTerm: "john");

        // Assert
        result.Users.Should().HaveCount(2); // John Doe и Bob Johnson
        result.TotalCount.Should().Be(2);
        result.Users.All(u => u.Name.ToLower().Contains("john")).Should().BeTrue();
    }

    [Fact]
    public async Task GetUsersPagedAsync_WithAgencyFilter_ShouldFilterResults()
    {
        // Act
        var result = await _repository.GetUsersPagedAsync(1, 10, agencyId: 1);

        // Assert
        result.Users.Should().HaveCount(2); // John и Jane в агентстве 1
        result.TotalCount.Should().Be(2);
        result.Users.All(u => u.AgencyId == 1).Should().BeTrue();
    }

    [Fact]
    public async Task GetUsersPagedAsync_WithActiveFilter_ShouldFilterResults()
    {
        // Act
        var result = await _repository.GetUsersPagedAsync(1, 10, isActive: true);

        // Assert
        result.Users.Should().HaveCount(2); // Только активные John и Jane
        result.TotalCount.Should().Be(2);
        result.Users.All(u => u.IsActive).Should().BeTrue();
    }
}