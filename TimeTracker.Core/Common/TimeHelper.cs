namespace TimeTracker.Core.Common;

public static class TimeHelper
{
    private const long MillisecondsPerSecond = 1000;
    private const long MillisecondsPerMinute = 60000;
    private const long MillisecondsPerHour = 3600000;
    private const long MillisecondsPerDay = 86400000;

    public static TimeSpan MillisecondsToTimeSpan(long milliseconds)
    {
        return TimeSpan.FromMilliseconds(milliseconds);
    }

    public static long TimeSpanToMilliseconds(TimeSpan timeSpan)
    {
        return (long)timeSpan.TotalMilliseconds;
    }

    public static string FormatHoursDecimal(long milliseconds)
    {
        var hours = milliseconds / 3600000.0;
        return hours.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture);
    }

    public static double MillisecondsToHours(long milliseconds)
    {
        return (double)milliseconds / MillisecondsPerHour;
    }

    public static long HoursToMilliseconds(double hours)
    {
        return (long)(hours * MillisecondsPerHour);
    }

    public static bool IsValidDailyHours(long milliseconds)
    {
        return milliseconds > 0 && milliseconds <= MillisecondsPerDay;
    }

    public static string FormatHours(long milliseconds)
    {
        var timeSpan = TimeSpan.FromMilliseconds(milliseconds);
        return $"{(int)timeSpan.TotalHours}:{timeSpan.Minutes:D2}";
    }

    public static string FormatHoursDetailed(long milliseconds)
    {
        var timeSpan = TimeSpan.FromMilliseconds(milliseconds);
        return $"{(int)timeSpan.TotalHours}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
    }

    public static long? ParseHoursToMilliseconds(string hoursString)
    {
        if (string.IsNullOrWhiteSpace(hoursString))
            return null;

        if (TimeSpan.TryParse(hoursString, out var timeSpan))
        {
            return (long)timeSpan.TotalMilliseconds;
        }

        return null;
    }
}