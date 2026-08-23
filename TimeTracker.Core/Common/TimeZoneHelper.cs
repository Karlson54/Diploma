namespace TimeTracker.Core.Common;

public static class TimeZoneHelper
{
    private static readonly TimeZoneInfo KyivTimeZone = ResolveKyivTimeZone();

    private static TimeZoneInfo ResolveKyivTimeZone()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Europe/Kyiv");
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("FLE Standard Time");
        }
    }

    public static DateTime UtcNowInKyiv()
        => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, KyivTimeZone);

    public static DateTime TodayInKyiv() => UtcNowInKyiv().Date;
}