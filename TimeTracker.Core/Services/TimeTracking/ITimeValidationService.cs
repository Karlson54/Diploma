namespace TimeTracker.Core.Services.TimeTracking;

public interface ITimeValidationService
{
    Task<ValidationResult> ValidateCreateAsync(
        long userId,
        DateTime entryDate,
        long hoursMilliseconds);

    Task<ValidationResult> ValidateUpdateAsync(
        long entryId,
        long userId,
        DateTime entryDate,
        long hoursMilliseconds);

    Task<ValidationResult> ValidateReferencesAsync(
        long agencyId,
        long? marketId,
        long? contractingAgencyId,
        long? clientId,
        long? mediaId,
        long? jobTypeId);

    Task<ValidationResult> ValidateUserPermissionsAsync(
        long userId,
        long? targetUserId = null);

    Task<long> GetTotalHoursForDayAsync(long userId, DateTime date);

    Task<long> GetTotalHoursForDayAsync(long userId, DateTime date, long excludeEntryId);
}

public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();

    public static ValidationResult Success() => new() { IsValid = true };

    public static ValidationResult Failure(string error) => new()
    {
        IsValid = false,
        Errors = new List<string> { error }
    };

    public static ValidationResult Failure(IEnumerable<string> errors) => new()
    {
        IsValid = false,
        Errors = errors.ToList()
    };

    public void AddError(string error)
    {
        IsValid = false;
        Errors.Add(error);
    }
}