using FluentAssertions;
using TimeTracker.Data.Entities;
using TimeTracker.Data.Repositories.Dictionaries;
using TimeTracker.Tests.Repositories.TestBase;
using Xunit;

namespace TimeTracker.Tests.Repositories.Dictionaries;

public class DictionaryRepositoryTests : RepositoryTestBase
{
    private readonly DictionaryRepository<Client> _clientRepository;
    private readonly DictionaryRepository<ProjectBrand> _projectRepository;

    public DictionaryRepositoryTests()
    {
        _clientRepository = new DictionaryRepository<Client>(Context);
        _projectRepository = new DictionaryRepository<ProjectBrand>(Context);
    }

    [Theory]
    [InlineData("Procter & Gamble", 1)]
    [InlineData("NESTLÉ", 2)] // Регистронезависимость
    [InlineData("NonExistent", null)]
    public async Task GetByNameAsync_ShouldReturnCorrectEntity(string name, long? expectedId)
    {
        // Act
        var result = await _clientRepository.GetByNameAsync(name);

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
    public async Task GetActiveAsync_ShouldReturnOnlyActiveEntities()
    {
        // Act
        var result = await _clientRepository.GetActiveAsync();

        // Assert
        result.Should().HaveCount(2); // P&G и Nestlé активны, Inactive Client неактивен
        result.All(c => c.IsActive).Should().BeTrue();
        result.Should().BeInAscendingOrder(c => c.Name);
    }

    [Theory]
    [InlineData("Procter & Gamble", true)]
    [InlineData("PROCTER & GAMBLE", true)] // Регистронезависимость
    [InlineData("New Client", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public async Task IsNameExistsAsync_ShouldReturnCorrectResult(string name, bool expected)
    {
        // Act
        var result = await _clientRepository.IsNameExistsAsync(name);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("Procter & Gamble", 2, false)] // Имя другого клиента, исключаем не того
    [InlineData("Procter & Gamble", 1, true)]  // Имя того же клиента, исключаем его
    [InlineData("New Client", 1, false)] // Новое имя
    public async Task IsNameExistsAsync_WithExclude_ShouldReturnCorrectResult(string name, long excludeId, bool expected)
    {
        // Act
        var result = await _clientRepository.IsNameExistsAsync(name, excludeId);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public async Task GetActiveCountAsync_ShouldReturnCorrectCount()
    {
        // Act
        var result = await _clientRepository.GetActiveCountAsync();

        // Assert
        result.Should().Be(2); // P&G и Nestlé
    }

    [Fact]
    public async Task DeactivateAsync_ShouldDeactivateEntity()
    {
        // Arrange
        const long clientId = 1; // P&G

        var clientBefore = await _clientRepository.GetByIdAsync(clientId);
        clientBefore!.IsActive.Should().BeTrue();

        // Act
        await _clientRepository.DeactivateAsync(clientId);
        await Context.SaveChangesAsync();

        // Assert
        var clientAfter = await _clientRepository.GetByIdAsync(clientId);
        clientAfter!.IsActive.Should().BeFalse();
        clientAfter.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task ActivateAsync_ShouldActivateEntity()
    {
        // Arrange
        const long clientId = 3; // Inactive Client

        var clientBefore = await _clientRepository.GetByIdAsync(clientId);
        clientBefore!.IsActive.Should().BeFalse();

        // Act
        await _clientRepository.ActivateAsync(clientId);
        await Context.SaveChangesAsync();

        // Assert
        var clientAfter = await _clientRepository.GetByIdAsync(clientId);
        clientAfter!.IsActive.Should().BeTrue();
        clientAfter.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task CanBeDeactivatedAsync_ShouldReturnTrue()
    {
        // Act - базовая реализация всегда возвращает true
        var result = await _clientRepository.CanBeDeactivatedAsync(1);

        // Assert
        result.Should().BeTrue();
    }
}