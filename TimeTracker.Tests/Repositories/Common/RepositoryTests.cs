using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Data.Entities;
using TimeTracker.Data.Repositories.Common;
using TimeTracker.Tests.Repositories.TestBase;
using Xunit;

namespace TimeTracker.Tests.Repositories.Common;

public class RepositoryTests : RepositoryTestBase
{
    private readonly Repository<User> _repository;

    public RepositoryTests()
    {
        _repository = new Repository<User>(Context);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnCorrectEntity()
    {
        // Act
        var result = await _repository.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Name.Should().Be("John Doe");
    }

    [Fact]
    public async Task GetByIdAsync_ForNonExistentId_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllEntities()
    {
        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(3); // John, Jane, Bob
    }

    [Fact]
    public async Task GetActiveAsync_ForDictionaryEntity_ShouldReturnOnlyActive()
    {
        // Arrange
        var clientRepository = new Repository<Client>(Context);

        // Act
        var result = await clientRepository.GetActiveAsync();

        // Assert
        result.Should().HaveCount(2); // Только активные клиенты
        result.All(c => ((Client)c).IsActive).Should().BeTrue();
    }

    [Fact]
    public async Task AddAsync_ShouldAddEntity()
    {
        // Arrange
        var newUser = new User
        {
            Name = "Test User",
            Email = "test@example.com",
            Login = "test.user",
            PasswordHash = "hash",
            AgencyId = 1,
            IsActive = true
        };

        // Act
        var result = await _repository.AddAsync(newUser);
        await Context.SaveChangesAsync();

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));

        var savedUser = await _repository.GetByIdAsync(result.Id);
        savedUser.Should().NotBeNull();
        savedUser!.Name.Should().Be("Test User");
    }

    [Fact]
    public async Task Update_ShouldUpdateEntity()
    {
        // Arrange
        var user = await _repository.GetByIdAsync(1);
        user!.Name = "Updated Name";

        // Act
        _repository.Update(user);
        await Context.SaveChangesAsync();

        // Assert
        var updatedUser = await _repository.GetByIdAsync(1);
        updatedUser!.Name.Should().Be("Updated Name");
        updatedUser.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteEntity()
    {
        // Arrange
        const long userId = 3; // Bob

        // Act
        await _repository.DeleteAsync(userId);
        await Context.SaveChangesAsync();

        // Assert
        var deletedUser = await _repository.GetByIdAsync(userId);
        deletedUser.Should().BeNull();
    }

    [Fact]
    public async Task ExistsAsync_WithPredicate_ShouldReturnCorrectResult()
    {
        // Act & Assert
        var existsJohn = await _repository.ExistsAsync(u => u.Name == "John Doe");
        existsJohn.Should().BeTrue();

        var existsNonExistent = await _repository.ExistsAsync(u => u.Name == "Non Existent");
        existsNonExistent.Should().BeFalse();
    }

    [Fact]
    public async Task CountAsync_ShouldReturnCorrectCount()
    {
        // Act
        var totalCount = await _repository.CountAsync();
        var activeCount = await _repository.CountAsync(u => u.IsActive);

        // Assert
        totalCount.Should().Be(3);
        activeCount.Should().Be(2); // John и Jane
    }

    [Fact]
    public async Task GetPagedAsync_ShouldReturnPagedResults()
    {
        // Act
        var result = await _repository.GetPagedAsync(1, 2); // Первая страница, размер 2

        // Assert
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(3);
    }

    [Fact]
    public async Task GetPagedAsync_WithFilter_ShouldApplyFilter()
    {
        // Act
        var result = await _repository.GetPagedAsync(
            1, 10, 
            filter: u => u.IsActive,
            orderBy: u => u.Name);

        // Assert
        result.Items.Should().HaveCount(2); // Только активные
        result.TotalCount.Should().Be(2);
        result.Items.All(u => u.IsActive).Should().BeTrue();
        result.Items.Should().BeInAscendingOrder(u => u.Name);
    }

    [Fact]
    public async Task GetProjectionAsync_ShouldReturnProjection()
    {
        // Act
        var result = await _repository.GetProjectionAsync(1, u => u.Name);

        // Assert
        result.Should().Be("John Doe");
    }

    [Fact]
    public async Task GetProjectionsAsync_WithFilter_ShouldReturnFilteredProjections()
    {
        // Act
        var result = await _repository.GetProjectionsAsync(
            u => u.IsActive,
            u => new { u.Id, u.Name });

        // Assert
        result.Should().HaveCount(2); // John и Jane
        result.All(r => r.Id > 0 && !string.IsNullOrEmpty(r.Name)).Should().BeTrue();
    }

    [Fact]
    public void GetQueryable_ShouldReturnQueryable()
    {
        // Act
        var queryable = _repository.GetQueryable();

        // Assert
        queryable.Should().NotBeNull();
        queryable.Should().BeAssignableTo<IQueryable<User>>();
    }

    [Fact]
    public async Task LoadReferenceAsync_ShouldLoadReference()
    {
        // Arrange
        var user = await _repository.GetByIdAsync(1);

        // Act
        await _repository.LoadReferenceAsync(user!, u => u.Agency);

        // Assert
        user!.Agency.Should().NotBeNull();
        user.Agency.Name.Should().Be("MediaCom Ukraine");
    }

    [Fact]
    public async Task LoadCollectionAsync_ShouldLoadCollection()
    {
        // Arrange
        var user = await _repository.GetByIdAsync(1);

        // Act
        await _repository.LoadCollectionAsync(user!, u => u.UserRoles);

        // Assert
        user!.UserRoles.Should().NotBeEmpty();
        user.UserRoles.Should().HaveCount(2); // Admin + Manager
    }

    [Fact]
    public async Task AddRangeAsync_ShouldAddMultipleEntities()
    {
        // Arrange
        var users = new[]
        {
            new User { Name = "User 1", Email = "user1@test.com", Login = "user1", PasswordHash = "hash1", AgencyId = 1 },
            new User { Name = "User 2", Email = "user2@test.com", Login = "user2", PasswordHash = "hash2", AgencyId = 1 }
        };

        // Act
        await _repository.AddRangeAsync(users);
        await Context.SaveChangesAsync();

        // Assert
        var allUsers = await _repository.GetAllAsync();
        allUsers.Should().HaveCount(5); // 3 изначальных + 2 новых

        foreach (var user in users)
        {
            user.Id.Should().BeGreaterThan(0);
            user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }
    }
}