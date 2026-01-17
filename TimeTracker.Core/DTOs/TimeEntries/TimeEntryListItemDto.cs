namespace TimeTracker.Core.DTOs.TimeEntries;

public class TimeEntryListItemDto
{
    public long Id { get; set; }
    
    public DateTime EntryDate { get; set; }
    
    public string UserName { get; set; } = string.Empty;
    
    public string ClientName { get; set; } = string.Empty;
    
    public string ProjectBrandName { get; set; } = string.Empty;
    
    public long HoursMilliseconds { get; set; }
    
    public TimeSpan Hours => TimeSpan.FromMilliseconds(HoursMilliseconds);
    
    public string FormattedHours => Core.Common.TimeHelper.FormatHours(HoursMilliseconds);
    
    public string? Comments { get; set; }
}