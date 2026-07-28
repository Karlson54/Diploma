namespace TimeTracker.Core.Common;

public static class SpecialJobTypes
{
    public const string Vacation = "Vacation";
    public const string VacationComment = "Vacation";
    
    // 480 хвилин * 60 * 1000 = 8 годин у мілісекундах
    public const long VacationHoursMilliseconds = 480 * 60 * 1000;

    public static bool IsVacation(string? jobTypeName)
    {
        return !string.IsNullOrWhiteSpace(jobTypeName) &&
               jobTypeName.Trim().Equals(Vacation, StringComparison.OrdinalIgnoreCase);
    }
}