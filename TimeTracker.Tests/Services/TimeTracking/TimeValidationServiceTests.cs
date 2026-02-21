using FluentAssertions;
using MockQueryable.Moq;
using Moq;
using TimeTracker.Core.Services.TimeTracking;
using TimeTracker.Data.Entities;
using TimeTracker.Data.Repositories.TimeEntries;
using TimeTracker.Data.Repositories.Users;
using TimeTracker.Data.UnitOfWork;
using TimeTracker.Tests.Services.TestBase;

namespace TimeTracker.Tests.Services.TimeTracking;

public class TimeValidationServiceTests : ServiceTestBase
{
    private readonly Mock<ITimeEntryRepository> _timeEntryRepoMock;
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly TimeValidationService _service;

    private const long HourInMs = 3_600_000;
    private const long MaxDayMs = 86_400_000; // 24 години

    public TimeValidationServiceTests()
    {
        _timeEntryRepoMock = new Mock<ITimeEntryRepository>();
        _userRepoMock = new Mock<IUserRepository>();

        _service = new TimeValidationService(
            _timeEntryRepoMock.Object,
            _userRepoMock.Object,
            UnitOfWorkMock.Object,
            CreateLogger<TimeValidationService>().Object);
    }

    // ==================== HELPERS ====================

    private static User CreateActiveUser(long id = 1) => new()
    {
        Id = id,
        Name = "John Doe",
        Email = "john@example.com",
        Login = "john.doe",
        AgencyId = 1,
        IsActive = true,
        UserRoles = new List<UserRole>()
    };

    private static User CreateUserWithRoles(long id = 1, params string[] roleNames) => new()
    {
        Id = id,
        Name = "John Doe",
        Email = "john@example.com",
        Login = "john.doe",
        AgencyId = 1,
        IsActive = true,
        UserRoles = roleNames.Select((name, i) => new UserRole
        {
            RoleId = i + 1,
            Role = new Role { Id = i + 1, Name = name, IsActive = true }
        }).ToList()
    };

    private void SetupTimeEntryQuery(IEnumerable<TimeEntry> entries)
    {
        var mock = entries.AsQueryable().BuildMock();
        _timeEntryRepoMock.Setup(x => x.GetQueryable()).Returns(mock);
    }

    // ==================== ValidateCreateAsync TESTS ====================

    [Fact]
    public async Task ValidateCreateAsync_ValidData_ReturnsSuccess()
    {
        // Arrange
        _userRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(CreateActiveUser());
        _timeEntryRepoMock.Setup(x => x.GetTotalHoursForDateAsync(1, DateTime.Today))
            .ReturnsAsync(0);

        // Act
        var result = await _service.ValidateCreateAsync(1, DateTime.Today, 8 * HourInMs);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task ValidateCreateAsync_ExceedsMaxHours_ReturnsFailure()
    {
        // Arrange
        _userRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(CreateActiveUser());
        _timeEntryRepoMock.Setup(x => x.GetTotalHoursForDateAsync(1, DateTime.Today))
            .ReturnsAsync(20 * HourInMs); // вже є 20 годин

        // Act
        var result = await _service.ValidateCreateAsync(1, DateTime.Today, 8 * HourInMs); // додаємо ще 8

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Contains("ліміт"));
    }

    [Fact]
    public async Task ValidateCreateAsync_FutureDate_ReturnsFailure()
    {
        // Arrange
        _userRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(CreateActiveUser());
        _timeEntryRepoMock.Setup(x => x.GetTotalHoursForDateAsync(1, It.IsAny<DateTime>()))
            .ReturnsAsync(0);

        // Act
        var result = await _service.ValidateCreateAsync(1, DateTime.Today.AddDays(5), 8 * HourInMs);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Contains("майбутн"));
    }

    [Fact]
    public async Task ValidateCreateAsync_NonExistentUser_ReturnsFailure()
    {
        // Arrange
        _userRepoMock.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((User?)null);

        // Act
        var result = await _service.ValidateCreateAsync(999, DateTime.Today, 8 * HourInMs);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Contains("999"));
    }

    [Fact]
    public async Task ValidateCreateAsync_InactiveUser_ReturnsFailure()
    {
        // Arrange
        var inactiveUser = CreateActiveUser();
        inactiveUser.IsActive = false;
        _userRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(inactiveUser);

        // Act
        var result = await _service.ValidateCreateAsync(1, DateTime.Today, 8 * HourInMs);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Contains("деактивован"));
    }

    [Fact]
    public async Task ValidateCreateAsync_ExactlyMaxHours_ReturnsSuccess()
    {
        // Arrange
        _userRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(CreateActiveUser());
        _timeEntryRepoMock.Setup(x => x.GetTotalHoursForDateAsync(1, DateTime.Today))
            .ReturnsAsync(0);

        // Act — рівно 24 години
        var result = await _service.ValidateCreateAsync(1, DateTime.Today, MaxDayMs);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    // ==================== ValidateUpdateAsync TESTS ====================

    [Fact]
    public async Task ValidateUpdateAsync_ValidData_ReturnsSuccess()
    {
        // Arrange
        var entry = new TimeEntry { Id = 1, UserId = 1, EntryDate = DateTime.Today, HoursMilliseconds = 8 * HourInMs };
        _timeEntryRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(entry);
        SetupTimeEntryQuery(new[] { entry });

        // Act
        var result = await _service.ValidateUpdateAsync(1, 1, DateTime.Today, 6 * HourInMs);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateUpdateAsync_NonExistentEntry_ReturnsFailure()
    {
        // Arrange
        _timeEntryRepoMock.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((TimeEntry?)null);

        // Act
        var result = await _service.ValidateUpdateAsync(999, 1, DateTime.Today, 8 * HourInMs);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Contains("999"));
    }

    [Fact]
    public async Task ValidateUpdateAsync_EntryBelongsToOtherUser_ReturnsFailure()
    {
        // Arrange
        var entry = new TimeEntry { Id = 1, UserId = 2, EntryDate = DateTime.Today, HoursMilliseconds = 8 * HourInMs };
        _timeEntryRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(entry);

        // Act — userId=1 намагається оновити запис userId=2
        var result = await _service.ValidateUpdateAsync(1, 1, DateTime.Today, 6 * HourInMs);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Contains("чужий"));
    }

    // ==================== ValidateUserPermissionsAsync TESTS ====================

    [Fact]
    public async Task ValidateUserPermissionsAsync_OwnEntry_ReturnsSuccess()
    {
        // Arrange
        _userRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(CreateActiveUser());

        // Act — targetUserId == userId (власний запис)
        var result = await _service.ValidateUserPermissionsAsync(1, 1);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateUserPermissionsAsync_AdminEditOtherUser_ReturnsSuccess()
    {
        // Arrange
        var admin = CreateUserWithRoles(1, "Admin");
        _userRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(admin);
        _userRepoMock.Setup(x => x.GetByIdWithRolesAsync(1)).ReturnsAsync(admin);

        // Act — Admin редагує запис userId=2
        var result = await _service.ValidateUserPermissionsAsync(1, 2);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateUserPermissionsAsync_EmployeeEditOtherUser_ReturnsFailure()
    {
        // Arrange
        var employee = CreateUserWithRoles(1, "Employee");
        _userRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(employee);
        _userRepoMock.Setup(x => x.GetByIdWithRolesAsync(1)).ReturnsAsync(employee);

        // Act — Employee намагається редагувати запис userId=2
        var result = await _service.ValidateUserPermissionsAsync(1, 2);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Contains("прав"));
    }

    [Fact]
    public async Task ValidateUserPermissionsAsync_InactiveUser_ReturnsFailure()
    {
        // Arrange
        var inactiveUser = CreateActiveUser();
        inactiveUser.IsActive = false;
        _userRepoMock.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(inactiveUser);

        // Act
        var result = await _service.ValidateUserPermissionsAsync(1, null);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Contains("деактивован"));
    }

    // ==================== GetTotalHoursForDayAsync TESTS ====================

    [Fact]
    public async Task GetTotalHoursForDayAsync_ReturnsCorrectSum()
    {
        // Arrange
        _timeEntryRepoMock.Setup(x => x.GetTotalHoursForDateAsync(1, DateTime.Today))
            .ReturnsAsync(6 * HourInMs);

        // Act
        var result = await _service.GetTotalHoursForDayAsync(1, DateTime.Today);

        // Assert
        result.Should().Be(6 * HourInMs);
    }

    [Fact]
    public async Task GetTotalHoursForDayAsync_WithExclude_ExcludesEntry()
    {
        // Arrange
        var entries = new[]
        {
            new TimeEntry { Id = 1, UserId = 1, EntryDate = DateTime.Today, HoursMilliseconds = 6 * HourInMs },
            new TimeEntry { Id = 2, UserId = 1, EntryDate = DateTime.Today, HoursMilliseconds = 4 * HourInMs }
        };
        SetupTimeEntryQuery(entries);

        // Act — виключаємо запис Id=1 (6 годин), має повернути тільки 4 години
        var result = await _service.GetTotalHoursForDayAsync(1, DateTime.Today, excludeEntryId: 1);

        // Assert
        result.Should().Be(4 * HourInMs);
    }
}