using TimeTracker.Core.Common;
using TimeTracker.Data.Entities;
using TimeTracker.Data.UnitOfWork;

namespace TimeTracker.API.Infrastructure.BackgroundServices;

public class AuditCleanupBackgroundService : BackgroundService
{
    // Раз на добу перевіряємо, чи не настав час чистити
    private static readonly TimeSpan CheckInterval = TimeSpan.FromHours(24);

    // Скользящее вікно зберігання аудиту — 30 днів
    private static readonly TimeSpan RetentionPeriod = TimeSpan.FromDays(30);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AuditCleanupBackgroundService> _logger;

    public AuditCleanupBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<AuditCleanupBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Невелика пауза при старті, щоб не заважати сідінгу/ініціалізації застосунку
        await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunIfDueAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Помилка під час перевірки/виконання AuditLogCleanup job");
            }

            try
            {
                await Task.Delay(CheckInterval, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                // Нормальне завершення при зупинці сервісу (graceful shutdown)
            }
        }
    }

    private async Task RunIfDueAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var jobState = await unitOfWork.SystemJobStates
            .GetSingleAsync(j => j.JobName == SystemJobNames.AuditLogCleanup);

        var isDue = jobState?.LastRunAt == null ||
                    DateTime.UtcNow - jobState.LastRunAt.Value >= RetentionPeriod;

        if (!isDue)
            return;

        var thresholdDate = DateTime.UtcNow.Subtract(RetentionPeriod);

        try
        {
            var deletedCount = await unitOfWork.AuditLogs.DeleteOldLogsAsync(thresholdDate);

            await UpsertJobStateAsync(unitOfWork, jobState, success: true, errorMessage: null);
            await unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "AuditLogCleanup завершено успішно. Видалено записів: {Count}, поріг: {Threshold:yyyy-MM-dd}",
                deletedCount, thresholdDate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AuditLogCleanup завершився з помилкою");
            await UpsertJobStateAsync(unitOfWork, jobState, success: false, errorMessage: ex.Message);
            await unitOfWork.SaveChangesAsync();
        }
    }

    private static async Task UpsertJobStateAsync(
        IUnitOfWork unitOfWork,
        SystemJobState? existingState,
        bool success,
        string? errorMessage)
    {
        if (existingState == null)
        {
            var newState = new SystemJobState
            {
                JobName = SystemJobNames.AuditLogCleanup,
                LastRunAt = DateTime.UtcNow,
                LastStatus = success ? "Success" : "Failed",
                LastErrorMessage = errorMessage
            };

            await unitOfWork.SystemJobStates.AddAsync(newState);
        }
        else
        {
            existingState.LastRunAt = DateTime.UtcNow;
            existingState.LastStatus = success ? "Success" : "Failed";
            existingState.LastErrorMessage = errorMessage;

            unitOfWork.SystemJobStates.Update(existingState);
        }
    }
}