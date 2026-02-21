using FluentAssertions;
using MockQueryable.Moq;
using Moq;
using TimeTracker.Core.DTOs.TimeEntries;
using TimeTracker.Core.Services.TimeTracking;
using TimeTracker.Data.Entities;
using TimeTracker.Data.Repositories.TimeEntries;
using TimeTracker.Data.Repositories.Users;
using TimeTracker.Tests.Services.TestBase;

namespace TimeTracker.Tests.Services.TimeTracking;

public class TimeEntryServiceTests : ServiceTestBase
{
    private readonly Mock<ITimeEntryRepository> _timeEntryRepoMock;
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<ITimeValidationService> _validationServiceMock;
    private readonly TimeEntryService _service;

    private const string TestIp = "127.0.0.1";
    private const string TestAgent = "xUnit";
    private const long TestUserId = 1;
    private const long HourInMs = 3_600_000;

    public TimeEntryServiceTests()
    {
        _timeEntryRepoMock = new Mock<ITimeEntryRepository>();
        _userRepoMock = new Mock<IUserRepository>();
        _validationServiceMock = new Mock<ITimeValidationService>();

        // По умолчанию все валидации проходят успешно
        _validationServiceMock
            .Setup(x => x.ValidateUserPermissionsAsync(It.IsAny<long>(), It.IsAny<long?>()))
            .ReturnsAsync(ValidationResult.Success());

        _validationServiceMock
            .Setup(x => x.ValidateCreateAsync(It.IsAny<long>(), It.IsAny<DateTime>(), It.IsAny<long>()))
            .ReturnsAsync(ValidationResult.Success());

        _validationServiceMock
            .Setup(x => x.ValidateUpdateAsync(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<DateTime>(), It.IsAny<long>()))
            .ReturnsAsync(ValidationResult.Success());

        _validationServiceMock
            .Setup(x => x.ValidateReferencesAsync(
                It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>(),
                It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>()))
            .ReturnsAsync(ValidationResult.Success());

        _service = new TimeEntryService(
            _timeEntryRepoMock.Object,
            _userRepoMock.Object,
            _validationServiceMock.Object,
            UnitOfWorkMock.Object,
            Mapper,
            CreateLogger<TimeEntryService>().Object,
            AuditServiceMock.Object);
    }

    // ==================== HELPERS ====================

    private static User CreateUser(long id = 1) => new()
    {
        Id = id,
        Name = "John Doe",
        Email = "john@example.com",
        Login = "john.doe",
        AgencyId = 1,
        IsActive = true
    };

    private static TimeEntry CreateTimeEntry(long id = 1, long userId = 1) => new()
    {
        Id = id,
        UserId = userId,
        EntryDate = DateTime.Today,
        HoursMilliseconds = 8 * HourInMs,
        AgencyId = 1,
        MarketId = 1,
        ContractingAgencyId = 1,
        ClientId = 1,
        ProjectBrandId = 1,
        MediaId = 1,
        JobTypeId = 1,
        Comments = "Test work",
        User = CreateUser(userId),
        Agency = new Agency { Id = 1, Name = "MediaCom" },
        Market = new Market { Id = 1, Name = "Ukraine" },
        ContractingAgency = new ContractingAgency { Id = 1, Name = "GroupM" },
        Client = new Client { Id = 1, Name = "P&G" },
        ProjectBrand = new ProjectBrand { Id = 1, Name = "Pampers" },
        Media = new Media { Id = 1, Name = "Digital" },
        JobType = new JobType { Id = 1, Name = "Strategy" }
    };

    private static CreateTimeEntryDto CreateDto(long userId = 1) => new()
    {
        UserId = userId,
        EntryDate = DateTime.Today,
        HoursMilliseconds = 8 * HourInMs,
        AgencyId = 1,
        MarketId = 1,
        ContractingAgencyId = 1,
        ClientId = 1,
        ProjectBrandId = 1,
        MediaId = 1,
        JobTypeId = 1,
        Comments = "Test work"
    };

    private void SetupTimeEntryQuery(IEnumerable<TimeEntry> entries)
    {
        var mock = entries.AsQueryable().BuildMock();
        _timeEntryRepoMock.Setup(x => x.GetQueryable()).Returns(mock);
    }

    // ==================== GET TESTS ====================

    [Fact]
    public async Task GetByIdAsync_ExistingEntry_OwnedByUser_ReturnsDto()
    {
        // Arrange
        var entry = CreateTimeEntry(id: 1, userId: TestUserId);
        SetupTimeEntryQuery(new[] { entry });
        _userRepoMock.Setup(x => x.GetByIdAsync(TestUserId)).ReturnsAsync(CreateUser());

        // Act
        var result = await _service.GetByIdAsync(1, TestUserId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistentEntry_ReturnsNull()
    {
        // Arrange
        SetupTimeEntryQuery(Array.Empty<TimeEntry>());

        // Act
        var result = await _service.GetByIdAsync(999, TestUserId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_EntryOwnedByOtherUser_ThrowsUnauthorized()
    {
        // Arrange — запись принадлежит userId=2, запрашивает userId=1
        var entry = CreateTimeEntry(id: 1, userId: 2);
        SetupTimeEntryQuery(new[] { entry });
        _userRepoMock.Setup(x => x.GetByIdAsync(TestUserId))
            .ReturnsAsync(CreateUser(TestUserId)); // обычный юзер без роли Admin/Manager

        // Act
        var act = () => _service.GetByIdAsync(1, TestUserId);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task GetUserEntriesAsync_ReturnsUserEntries()
    {
        // Arrange
        var entries = new[] { CreateTimeEntry(1, TestUserId), CreateTimeEntry(2, TestUserId) };
        _timeEntryRepoMock
            .Setup(x => x.GetUserTimeEntriesAsync(TestUserId, null, null))
            .ReturnsAsync(entries);
        SetupTimeEntryQuery(entries);

        // Act
        var result = await _service.GetUserEntriesAsync(TestUserId);

        // Assert
        result.Should().HaveCount(2);
        result.All(e => e.UserId == TestUserId).Should().BeTrue();
    }

    // ==================== CREATE TESTS ====================

    [Fact]
    public async Task CreateAsync_ValidData_ReturnsCreatedEntry()
    {
        // Arrange
        var dto = CreateDto();
        var savedEntry = CreateTimeEntry();

        _timeEntryRepoMock.Setup(x => x.AddAsync(It.IsAny<TimeEntry>())).ReturnsAsync(savedEntry);
        UnitOfWorkMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);
        SetupTimeEntryQuery(new[] { savedEntry });

        // Act
        var result = await _service.CreateAsync(dto, TestUserId, TestIp, TestAgent);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(TestUserId);
        _timeEntryRepoMock.Verify(x => x.AddAsync(It.IsAny<TimeEntry>()), Times.Once);
        UnitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ValidationFails_ThrowsInvalidOperation()
    {
        // Arrange
        var dto = CreateDto();
        _validationServiceMock
            .Setup(x => x.ValidateCreateAsync(It.IsAny<long>(), It.IsAny<DateTime>(), It.IsAny<long>()))
            .ReturnsAsync(ValidationResult.Failure("Перевищено ліміт часу за день"));

        // Act
        var act = () => _service.CreateAsync(dto, TestUserId, TestIp, TestAgent);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*ліміт*");
    }

    [Fact]
    public async Task CreateAsync_PermissionFails_ThrowsUnauthorized()
    {
        // Arrange
        var dto = CreateDto(userId: 99); // пытается создать для другого юзера
        _validationServiceMock
            .Setup(x => x.ValidateUserPermissionsAsync(TestUserId, 99L))
            .ReturnsAsync(ValidationResult.Failure("Недостатньо прав"));

        // Act
        var act = () => _service.CreateAsync(dto, TestUserId, TestIp, TestAgent);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*прав*");
    }

    [Fact]
    public async Task CreateAsync_ReferenceValidationFails_ThrowsInvalidOperation()
    {
        // Arrange
        var dto = CreateDto();
        _validationServiceMock
            .Setup(x => x.ValidateReferencesAsync(
                It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>(),
                It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<long>()))
            .ReturnsAsync(ValidationResult.Failure("Agency не знайдено"));

        // Act
        var act = () => _service.CreateAsync(dto, TestUserId, TestIp, TestAgent);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Agency*");
    }

    // ==================== UPDATE TESTS ====================

    [Fact]
    public async Task UpdateAsync_ValidData_ReturnsUpdatedEntry()
    {
        // Arrange
        var existing = CreateTimeEntry(id: 1, userId: TestUserId);
        var updateDto = new UpdateTimeEntryDto
        {
            EntryDate = DateTime.Today,
            HoursMilliseconds = 6 * HourInMs,
            AgencyId = 1, MarketId = 1, ContractingAgencyId = 1,
            ClientId = 1, ProjectBrandId = 1, MediaId = 1, JobTypeId = 1
        };

        SetupTimeEntryQuery(new[] { existing });
        UnitOfWorkMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _service.UpdateAsync(1, updateDto, TestUserId, TestIp, TestAgent);

        // Assert
        result.Should().NotBeNull();
        UnitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_NonExistentEntry_ThrowsKeyNotFound()
    {
        // Arrange
        SetupTimeEntryQuery(Array.Empty<TimeEntry>());
        var updateDto = new UpdateTimeEntryDto { EntryDate = DateTime.Today, HoursMilliseconds = HourInMs };

        // Act
        var act = () => _service.UpdateAsync(999, updateDto, TestUserId, TestIp, TestAgent);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("*999*");
    }

    [Fact]
    public async Task UpdateAsync_PermissionFails_ThrowsUnauthorized()
    {
        // Arrange
        var existing = CreateTimeEntry(id: 1, userId: 2); // чужая запись
        SetupTimeEntryQuery(new[] { existing });
        _validationServiceMock
            .Setup(x => x.ValidateUserPermissionsAsync(TestUserId, 2L))
            .ReturnsAsync(ValidationResult.Failure("Недостатньо прав"));

        var updateDto = new UpdateTimeEntryDto { EntryDate = DateTime.Today, HoursMilliseconds = HourInMs };

        // Act
        var act = () => _service.UpdateAsync(1, updateDto, TestUserId, TestIp, TestAgent);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    // ==================== DELETE TESTS ====================

    [Fact]
    public async Task DeleteAsync_OwnEntry_DeletesSuccessfully()
    {
        // Arrange
        var entry = CreateTimeEntry(id: 1, userId: TestUserId);
        SetupTimeEntryQuery(new[] { entry });
        _timeEntryRepoMock.Setup(x => x.DeleteAsync(1)).Returns(Task.CompletedTask);
        UnitOfWorkMock.Setup(x => x.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        await _service.DeleteAsync(1, TestUserId, TestIp, TestAgent);

        // Assert
        _timeEntryRepoMock.Verify(x => x.DeleteAsync(1), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_NonExistentEntry_ThrowsKeyNotFound()
    {
        // Arrange
        SetupTimeEntryQuery(Array.Empty<TimeEntry>());

        // Act
        var act = () => _service.DeleteAsync(999, TestUserId, TestIp, TestAgent);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task DeleteAsync_OtherUsersEntry_ThrowsUnauthorized()
    {
        // Arrange
        var entry = CreateTimeEntry(id: 1, userId: 2); // чужая запись
        SetupTimeEntryQuery(new[] { entry });
        _validationServiceMock
            .Setup(x => x.ValidateUserPermissionsAsync(TestUserId, 2L))
            .ReturnsAsync(ValidationResult.Failure("Недостатньо прав"));

        // Act
        var act = () => _service.DeleteAsync(1, TestUserId, TestIp, TestAgent);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    // ==================== VALIDATION HELPERS ====================

    [Fact]
    public async Task GetRemainingHoursForDayAsync_ReturnsCorrectValue()
    {
        // Arrange — уже потрачено 6 часов, лимит 24 часа
        var usedHours = 6 * HourInMs;
        var maxHours = 24 * HourInMs;
        _validationServiceMock
            .Setup(x => x.GetTotalHoursForDayAsync(TestUserId, DateTime.Today))
            .ReturnsAsync(usedHours);

        // Act
        var remaining = await _service.GetRemainingHoursForDayAsync(TestUserId, DateTime.Today);

        // Assert
        remaining.Should().Be(maxHours - usedHours);
    }
}