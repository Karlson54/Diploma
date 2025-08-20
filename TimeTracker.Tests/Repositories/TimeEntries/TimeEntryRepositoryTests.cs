using FluentAssertions;
using TimeTracker.Data.Entities;
using TimeTracker.Data.Repositories.TimeEntries;
using TimeTracker.Tests.Repositories.TestBase;
using Xunit;

namespace TimeTracker.Tests.Repositories.TimeEntries;

public class TimeEntryRepositoryTests : RepositoryTestBase
{
    private readonly TimeEntryRepository _repository;
    private const long HourInMs = 3600000;

    public TimeEntryRepositoryTests()
    {
        _repository = new TimeEntryRepository(Context);
    }

    [Fact]
    public async Task GetUserTimeEntriesAsync_ShouldReturnUserEntries()
    {
        // Act
        var result = await _repository.GetUserTimeEntriesAsync(1); // John's entries

        // Assert
        result.Should().HaveCount(2); // John имеет 2 записи
        result.All(te => te.UserId == 1).Should().BeTrue();
        result.Should().BeInDescendingOrder(te => te.EntryDate);
    }

    [Fact]
    public async Task GetUserTimeEntriesAsync_WithDateRange_ShouldFilterByDate()
    {
        // Arrange
        var fromDate = DateTime.Today;
        var toDate = DateTime.Today;

        // Act
        var result = await _repository.GetUserTimeEntriesAsync(1, fromDate, toDate);

        // Assert
        result.Should().HaveCount(1); // Только одна запись John'а на сегодня
        result.First().EntryDate.Date.Should().Be(DateTime.Today);
    }

    [Fact]
    public async Task HasTimeEntryForDateAsync_ShouldReturnCorrectResult()
    {
        // Act & Assert
        var hasToday = await _repository.HasTimeEntryForDateAsync(1, DateTime.Today);
        hasToday.Should().BeTrue();

        var hasTomorrow = await _repository.HasTimeEntryForDateAsync(1, DateTime.Today.AddDays(1));
        hasTomorrow.Should().BeFalse();
    }

    [Fact]
    public async Task GetTotalHoursForDateAsync_ShouldReturnCorrectTotal()
    {
        // Act
        var totalHours = await _repository.GetTotalHoursForDateAsync(1, DateTime.Today);

        // Assert
        totalHours.Should().Be(8 * HourInMs); // John работал 8 часов сегодня
    }

    [Fact]
    public async Task CanAddTimeAsync_ShouldValidateMaxHours()
    {
        // Act & Assert
        var canAdd4Hours = await _repository.CanAddTimeAsync(1, DateTime.Today, 4 * HourInMs);
        canAdd4Hours.Should().BeTrue(); // 8 + 4 = 12 часов (можно)

        var canAdd20Hours = await _repository.CanAddTimeAsync(1, DateTime.Today, 20 * HourInMs);
        canAdd20Hours.Should().BeFalse(); // 8 + 20 = 28 часов (нельзя)
    }

    [Fact]
    public async Task GetTimeEntriesByPeriodAsync_ShouldReturnEntriesInRange()
    {
        // Arrange
        var fromDate = DateTime.Today.AddDays(-1);
        var toDate = DateTime.Today;

        // Act
        var result = await _repository.GetTimeEntriesByPeriodAsync(fromDate, toDate);

        // Assert
        result.Should().HaveCount(3); // Все записи в этом диапазоне
        result.Should().BeInAscendingOrder(te => te.EntryDate);
    }

    [Fact]
    public async Task GetTimeEntriesByAgencyAsync_ShouldFilterByAgency()
    {
        // Act
        var result = await _repository.GetTimeEntriesByAgencyAsync(1, DateTime.Today.AddDays(-1), DateTime.Today);

        // Assert
        result.Should().HaveCount(3); // Все записи принадлежат агентству 1
        result.All(te => te.AgencyId == 1).Should().BeTrue();
    }

    [Fact]
    public async Task GetTimeEntriesByClientAsync_ShouldFilterByClient()
    {
        // Act
        var result = await _repository.GetTimeEntriesByClientAsync(1, DateTime.Today.AddDays(-1), DateTime.Today);

        // Assert
        result.Should().HaveCount(2); // 2 записи для клиента P&G
        result.All(te => te.ClientId == 1).Should().BeTrue();
    }

    [Fact]
    public async Task GetUserTotalHoursAsync_ShouldReturnCorrectTotals()
    {
        // Act
        var result = await _repository.GetUserTotalHoursAsync(DateTime.Today.AddDays(-1), DateTime.Today);

        // Assert
        result.Should().HaveCount(2); // John и Jane
        result[1].Should().Be(12 * HourInMs); // John: 8 + 4 = 12 часов
        result[2].Should().Be(6 * HourInMs);  // Jane: 6 часов
    }

    [Fact]
    public async Task GetTimeEntriesPagedAsync_ShouldReturnPagedResults()
    {
        // Act
        var result = await _repository.GetTimeEntriesPagedAsync(1, 2); // Первая страница, 2 элемента

        // Assert
        result.Entries.Should().HaveCount(2);
        result.TotalCount.Should().Be(3);
        result.Entries.Should().BeInDescendingOrder(te => te.EntryDate);
    }

    [Fact]
    public async Task GetTimeEntriesPagedAsync_WithFilters_ShouldApplyFilters()
    {
        // Act
        var result = await _repository.GetTimeEntriesPagedAsync(
            1, 10, 
            userId: 1, 
            fromDate: DateTime.Today);

        // Assert
        result.Entries.Should().HaveCount(1); // Только одна запись John'а на сегодня
        result.TotalCount.Should().Be(1);
        result.Entries.All(te => te.UserId == 1 && te.EntryDate.Date == DateTime.Today).Should().BeTrue();
    }

    [Fact]
    public async Task CanUpdateTimeAsync_ShouldValidateUpdate()
    {
        // Arrange - John имеет запись на 8 часов сегодня (entryId = 1)
        const long entryId = 1;
        const long userId = 1;
        var date = DateTime.Today;

        // Act & Assert
        var canUpdate6Hours = await _repository.CanUpdateTimeAsync(entryId, userId, date, 6 * HourInMs);
        canUpdate6Hours.Should().BeTrue(); // Обновление на 6 час
        var canUpdate30Hours = await _repository.CanUpdateTimeAsync(entryId, userId, date, 30 * HourInMs);
       canUpdate30Hours.Should().BeFalse(); // 30 часов превышает лимит
   }

   [Fact]
   public async Task GetDailySummaryAsync_ShouldReturnDailySummary()
   {
       // Act
       var result = await _repository.GetDailySummaryAsync(1, DateTime.Today);

       // Assert
       result.Should().NotBeNull();
       var summary = result as dynamic;
       ((DateTime)summary!.Date).Date.Should().Be(DateTime.Today);
       ((long)summary.TotalHours).Should().Be(8 * HourInMs);
       ((int)summary.TotalEntries).Should().Be(1);
   }

   [Fact]
   public async Task GetDailySummaryAsync_ForDateWithoutEntries_ShouldReturnNull()
   {
       // Act
       var result = await _repository.GetDailySummaryAsync(1, DateTime.Today.AddDays(10));

       // Assert
       result.Should().BeNull();
   }

   [Fact]
   public async Task GetUserSummaryAsync_ShouldReturnUserStatistics()
   {
       // Act
       var result = await _repository.GetUserSummaryAsync(DateTime.Today.AddDays(-1), DateTime.Today);

       // Assert
       var summaries = result.ToList();
       summaries.Should().HaveCount(2); // John и Jane

       var johnSummary = summaries.First() as dynamic;
       ((long)johnSummary!.TotalHours).Should().Be(12 * HourInMs); // John: 8 + 4 = 12 часов
       ((int)johnSummary.TotalEntries).Should().Be(2);
   }

   [Fact]
   public async Task GetClientSummaryAsync_ShouldReturnClientStatistics()
   {
       // Act
       var result = await _repository.GetClientSummaryAsync(DateTime.Today.AddDays(-1), DateTime.Today);

       // Assert
       var summaries = result.ToList();
       summaries.Should().HaveCount(2); // P&G и Nestlé

       var pgSummary = summaries.FirstOrDefault(s => ((dynamic)s).ClientId == 1L) as dynamic;
       pgSummary.Should().NotBeNull();
       ((long)pgSummary!.TotalHours).Should().Be(12 * HourInMs); // P&G: 8 + 4 = 12 часов
   }

   [Fact]
   public async Task GetTimeEntriesWithDetailsAsync_ShouldLoadNavigationProperties()
   {
       // Act
       var result = await _repository.GetTimeEntriesWithDetailsAsync(
           DateTime.Today.AddDays(-1), 
           DateTime.Today);

       // Assert
       result.Should().HaveCount(3);
       
       foreach (var entry in result)
       {
           entry.User.Should().NotBeNull();
           entry.Agency.Should().NotBeNull();
           entry.Client.Should().NotBeNull();
           entry.ProjectBrand.Should().NotBeNull();
           entry.Media.Should().NotBeNull();
           entry.JobType.Should().NotBeNull();
           entry.Market.Should().NotBeNull();
           entry.ContractingAgency.Should().NotBeNull();
       }
   }
}