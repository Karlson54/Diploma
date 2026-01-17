namespace TimeTracker.Core.Common;

public static class ValidationConstants
{
    // TimeEntry constants
    public const long MinHoursMs = 1;
    public const long MaxHoursPerDayMs = 86400000; // 24 години в мілісекундах
    public const int MaxCommentsLength = 500;
    
    // Error messages for TimeEntry
    public const string MaxDailyHoursError = "Час не може перевищувати 24 години";
    public const string NotFutureDateError = "Дата не може бути в майбутньому";
    public const string MinHoursError = "Час має бути більше 0";
    
    // Validation messages templates
    public const string RequiredFieldError = "{0} обов'язковий";
    public const string PositiveNumberError = "{0} має бути додатним числом";
    public const string RangeError = "{0} має бути від {1} до {2}";
}