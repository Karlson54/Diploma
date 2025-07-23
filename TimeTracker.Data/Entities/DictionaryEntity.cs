namespace TimeTracker.Data.Entities;

public class DictionaryEntity: BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    
}