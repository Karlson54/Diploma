using FluentAssertions;
using MockQueryable.Moq;
using Moq;
using TimeTracker.Core.DTOs.Dictionaries.Clients;
using TimeTracker.Core.Services.Dictionaries.Clients;
using TimeTracker.Data.Entities;
using TimeTracker.Data.Repositories.Common;
using TimeTracker.Data.Repositories.Dictionaries;
using TimeTracker.Tests.Services.TestBase;

namespace TimeTracker.Tests.Services.Dictionaries;

public class ClientServiceTests : ServiceTestBase
{
    private readonly Mock<IDictionaryRepository<Client>> _repoMock;
    private readonly Mock<IRepository<TimeEntry>> _timeEntriesRepoMock;
    private readonly ClientService _service;

    private const string TestIp = "127.0.0.1";
    private const string TestAgent = "xUnit";
    private const long TestUserId = 1;
    private const string TestUserName = "Test User";

    public ClientServiceTests()
    {
        _repoMock = new Mock<IDictionaryRepository<Client>>();
        _timeEntriesRepoMock = new Mock<IRepository<TimeEntry>>();

        UnitOfWorkMock
            .Setup(x => x.TimeEntries)
            .Returns(_timeEntriesRepoMock.Object);

        _service = new ClientService(
            _repoMock.Object,
            UnitOfWorkMock.Object,
            Mapper,
            CreateLogger<ClientService>().Object,
            AuditServiceMock.Object);
    }

    // ==================== HELPERS ====================

    private static Client CreateClient(long id = 1, string name = "Test Client",
        string email = "test@client.com", bool isActive = true) => new()
    {
        Id = id,
        Name = name,
        Email = email,
        IsActive = isActive,
        CreatedAt = DateTime.UtcNow
    };

    private void SetupTimeEntriesQuery(IEnumerable<TimeEntry> entries)
    {
        var mock = entries.AsQueryable().BuildMock();
        _timeEntriesRepoMock.Setup(x => x.GetQueryable()).Returns(mock);
    }

    private void SetupClientsQuery(IEnumerable<Client> clients)
    {
        var mock = clients.AsQueryable().BuildMock();
        _repoMock.Setup(x => x.GetQueryable()).Returns(mock);
    }

    // ==================== GET TESTS ====================

    [Fact]
    public async Task GetByIdAsync_ExistingClient_ReturnsDto()
    {
        // Arrange
        var client = CreateClient();
        _repoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(client);

        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.Name.Should().Be("Test Client");
        result.Email.Should().Be("test@client.com");
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentClient_ReturnsNull()
    {
        // Arrange
        _repoMock.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((Client?)null);

        // Act
        var result = await _service.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllClients()
    {
        // Arrange
        var clients = new[]
        {
            CreateClient(1, "Client A"),
            CreateClient(2, "Client B"),
            CreateClient(3, "Client C", isActive: false)
        };
        _repoMock.Setup(x => x.GetAllAsync()).ReturnsAsync(clients);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetActiveAsync_ReturnsOnlyActiveClients()
    {
        // Arrange
        var activeClients = new[]
        {
            CreateClient(1, "Client A"),
            CreateClient(2, "Client B")
        };
        _repoMock.Setup(x => x.GetActiveAsync()).ReturnsAsync(activeClients);

        // Act
        var result = await _service.GetActiveAsync();

        // Assert
        result.Should().HaveCount(2);
        result.All(c => c.IsActive).Should().BeTrue();
    }

    // ==================== CREATE TESTS ====================

    [Fact]
    public async Task CreateAsync_ValidData_ReturnsCreatedClient()
    {
        // Arrange
        var dto = new CreateClientDto { Name = "New Client", Email = "new@client.com" };
        var createdClient = CreateClient(1, dto.Name, dto.Email);

        SetupClientsQuery(Array.Empty<Client>());
        _repoMock.Setup(x => x.IsNameExistsAsync(dto.Name)).ReturnsAsync(false);
        // AddAsync возвращает Task<T> — используем ReturnsAsync
        _repoMock.Setup(x => x.AddAsync(It.IsAny<Client>())).ReturnsAsync(createdClient);
        UnitOfWorkMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);
        _repoMock.Setup(x => x.GetByIdAsync(It.IsAny<long>())).ReturnsAsync(createdClient);

        // Act
        var result = await _service.CreateAsync(dto, TestUserId, TestUserName, TestIp, TestAgent);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(dto.Name);
        _repoMock.Verify(x => x.AddAsync(It.IsAny<Client>()), Times.Once);
        UnitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_DuplicateEmail_ThrowsInvalidOperation()
    {
        // Arrange
        var dto = new CreateClientDto { Name = "New Client", Email = "existing@client.com" };
        var existingClients = new[] { CreateClient(1, "Existing", "existing@client.com") };
        SetupClientsQuery(existingClients);

        // Act
        var act = () => _service.CreateAsync(dto, TestUserId, TestUserName, TestIp, TestAgent);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*email*");
    }

    [Fact]
    public async Task CreateAsync_DuplicateName_ThrowsInvalidOperation()
    {
        // Arrange
        var dto = new CreateClientDto { Name = "Duplicate Name" };
        SetupClientsQuery(Array.Empty<Client>());
        _repoMock.Setup(x => x.IsNameExistsAsync(dto.Name)).ReturnsAsync(true);

        // Act
        var act = () => _service.CreateAsync(dto, TestUserId, TestUserName, TestIp, TestAgent);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*назвою*");
    }

    // ==================== UPDATE TESTS ====================

    [Fact]
    public async Task UpdateAsync_ValidData_ReturnsUpdatedClient()
    {
        // Arrange
        var existing = CreateClient(1, "Old Name", "old@client.com");
        var dto = new UpdateClientDto { Name = "New Name", Email = "new@client.com" };
        var updatedClient = CreateClient(1, dto.Name, dto.Email);

        _repoMock.SetupSequence(x => x.GetByIdAsync(1))
            .ReturnsAsync(existing)     // первый вызов — получение для проверки email
            .ReturnsAsync(updatedClient); // второй вызов — после сохранения
        SetupClientsQuery(Array.Empty<Client>());
        _repoMock.Setup(x => x.IsNameExistsAsync(dto.Name, 1)).ReturnsAsync(false);
        // Update — void метод, мокать не нужно
        UnitOfWorkMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _service.UpdateAsync(1, dto, TestUserId, TestUserName, TestIp, TestAgent);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be(dto.Name);
    }

    [Fact]
    public async Task UpdateAsync_NonExistentClient_ThrowsKeyNotFound()
    {
        // Arrange
        _repoMock.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((Client?)null);
        var dto = new UpdateClientDto { Name = "New Name" };

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
        var existing = CreateClient(1, "Client", "old@client.com");
        var dto = new UpdateClientDto { Name = "Client", Email = "taken@client.com" };
        var otherClient = CreateClient(2, "Other", "taken@client.com");

        _repoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(existing);
        SetupClientsQuery(new[] { otherClient });

        // Act
        var act = () => _service.UpdateAsync(1, dto, TestUserId, TestUserName, TestIp, TestAgent);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*email*");
    }

    // ==================== DELETE TESTS ====================

    [Fact]
    public async Task DeleteAsync_ClientWithNoTimeEntries_DeletesSuccessfully()
    {
        // Arrange
        var client = CreateClient();
        _repoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(client);
        SetupTimeEntriesQuery(Array.Empty<TimeEntry>());
        // DeleteAsync(long id) возвращает Task — используем Returns(Task.CompletedTask)
        _repoMock.Setup(x => x.DeleteAsync(1)).Returns(Task.CompletedTask);
        UnitOfWorkMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        await _service.DeleteAsync(1, TestUserId, TestUserName, TestIp, TestAgent);

        // Assert
        _repoMock.Verify(x => x.DeleteAsync(1), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ClientWithTimeEntries_ThrowsInvalidOperation()
    {
        // Arrange
        var client = CreateClient();
        var timeEntries = new[] { new TimeEntry { Id = 1, ClientId = 1 } };
        _repoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(client);
        SetupTimeEntriesQuery(timeEntries);

        // Act
        var act = () => _service.DeleteAsync(1, TestUserId, TestUserName, TestIp, TestAgent);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*записів часу*");
    }

    [Fact]
    public async Task DeleteAsync_NonExistentClient_ThrowsKeyNotFound()
    {
        // Arrange
        _repoMock.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((Client?)null);

        // Act
        var act = () => _service.DeleteAsync(999, TestUserId, TestUserName, TestIp, TestAgent);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    // ==================== EMAIL TESTS ====================

    [Fact]
    public async Task GetByEmailAsync_ExistingEmail_ReturnsClient()
    {
        // Arrange
        var client = CreateClient(1, "Test", "find@me.com");
        SetupClientsQuery(new[] { client });

        // Act
        var result = await _service.GetByEmailAsync("find@me.com");

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be("find@me.com");
    }

    [Fact]
    public async Task GetByEmailAsync_EmptyEmail_ReturnsNull()
    {
        // Act
        var result = await _service.GetByEmailAsync("");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task IsEmailExistsAsync_ExistingEmail_ReturnsTrue()
    {
        // Arrange
        var client = CreateClient(1, "Test", "exists@client.com");
        SetupClientsQuery(new[] { client });

        // Act
        var result = await _service.IsEmailExistsAsync("exists@client.com");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsEmailExistsAsync_WithExcludeId_ExcludesCurrentClient()
    {
        // Arrange
        var client = CreateClient(1, "Test", "mine@client.com");
        SetupClientsQuery(new[] { client });

        // Act — исключаем ID 1, email принадлежит ему же
        var result = await _service.IsEmailExistsAsync("mine@client.com", excludeId: 1);

        // Assert
        result.Should().BeFalse();
    }

    // ==================== DEACTIVATE TESTS ====================

    [Fact]
    public async Task CanBeDeactivatedAsync_ClientWithTimeEntries_ReturnsFalse()
    {
        // Arrange
        var timeEntries = new[] { new TimeEntry { Id = 1, ClientId = 1 } };
        SetupTimeEntriesQuery(timeEntries);

        // Act
        var result = await _service.CanBeDeactivatedAsync(1);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CanBeDeactivatedAsync_ClientWithNoTimeEntries_ReturnsTrue()
    {
        // Arrange
        SetupTimeEntriesQuery(Array.Empty<TimeEntry>());

        // Act
        var result = await _service.CanBeDeactivatedAsync(1);

        // Assert
        result.Should().BeTrue();
    }
}