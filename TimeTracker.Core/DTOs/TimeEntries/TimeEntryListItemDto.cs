namespace TimeTracker.Core.DTOs.TimeEntries;

public class TimeEntryListItemDto
{
    public long Id { get; set; }
    
    public long UserId { get; set; }
    
    public DateTime EntryDate { get; set; }
    
    public string UserName { get; set; } = string.Empty;
    
    public long? MarketId { get; set; }
    public string? MarketName { get; set; }
    
    public long? ContractingAgencyId { get; set; }
    public string? ContractingAgencyName { get; set; }
    
    public long? ClientId { get; set; }
    public string? ClientName { get; set; }
    
    public long? ProjectBrandId { get; set; }
    public string? ProjectBrandName { get; set; }
    
    public long? MediaId { get; set; }
    public string? MediaName { get; set; }
    
    public long? JobTypeId { get; set; }
    public string? JobTypeName { get; set; }
    
    public long HoursMilliseconds { get; set; }
    
    public TimeSpan Hours => TimeSpan.FromMilliseconds(HoursMilliseconds);
    
    public string FormattedHours => Core.Common.TimeHelper.FormatHours(HoursMilliseconds);
    
    public string? Comments { get; set; }
}