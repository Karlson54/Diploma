namespace TimeTracker.Core.DTOs.TimeEntries;

public class TimeEntryDto
{
    public long Id { get; set; }
    
    public long UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    
    public DateTime EntryDate { get; set; }
    
    public long AgencyId { get; set; }
    public string AgencyName { get; set; } = string.Empty;
    
    public long MarketId { get; set; }
    public string MarketName { get; set; } = string.Empty;
    
    public long ContractingAgencyId { get; set; }
    public string ContractingAgencyName { get; set; } = string.Empty;
    
    public long ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    
    public long ProjectBrandId { get; set; }
    public string ProjectBrandName { get; set; } = string.Empty;
    
    public long MediaId { get; set; }
    public string MediaName { get; set; } = string.Empty;
    
    public long JobTypeId { get; set; }
    public string JobTypeName { get; set; } = string.Empty;
    
    public long HoursMilliseconds { get; set; }
    
    public TimeSpan Hours => TimeSpan.FromMilliseconds(HoursMilliseconds);
    
    public string FormattedHours => Core.Common.TimeHelper.FormatHours(HoursMilliseconds);
    
    public string? Comments { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}