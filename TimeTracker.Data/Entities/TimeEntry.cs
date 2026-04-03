namespace TimeTracker.Data.Entities;

public class TimeEntry : BaseEntity
{
    public long UserId { get; set; }
    public DateTime EntryDate { get; set; }
    public long AgencyId { get; set; }
    public long MarketId { get; set; }
    public long ContractingAgencyId { get; set; }
    public long ClientId { get; set; }
    public string ProjectBrand { get; set; } = string.Empty; // было: public long ProjectBrandId
    public long MediaId { get; set; }
    public long JobTypeId { get; set; }
    public long HoursMilliseconds { get; set; }
    public string? Comments { get; set; }

    public virtual User User { get; set; } = null!;
    public virtual Agency Agency { get; set; } = null!;
    public virtual Market Market { get; set; } = null!;
    public virtual ContractingAgency ContractingAgency { get; set; } = null!;
    public virtual Client Client { get; set; } = null!;
    // убрали: public virtual ProjectBrand ProjectBrand
    public virtual Media Media { get; set; } = null!;
    public virtual JobType JobType { get; set; } = null!;
}