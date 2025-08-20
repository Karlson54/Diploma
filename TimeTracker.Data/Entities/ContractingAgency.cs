namespace TimeTracker.Data.Entities;

public class ContractingAgency : DictionaryEntity
{
    public virtual ICollection<TimeEntry> TimeEntries { get; set; } = new List<TimeEntry>();
}