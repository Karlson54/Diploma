namespace TimeTracker.Core.DTOs.Dictionaries;

public class DictionaryDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}