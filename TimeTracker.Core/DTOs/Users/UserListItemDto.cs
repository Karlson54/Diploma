namespace TimeTracker.Core.DTOs.Users;

/// <summary>
/// Скорочена інформація про користувача для відображення у списках
/// </summary>
public class UserListItemDto
{
    /// <summary>
    /// ID користувача
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Повне ім'я
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Логін
    /// </summary>
    public string Login { get; set; } = string.Empty;

    /// <summary>
    /// Email
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// ID агентства
    /// </summary>
    public long AgencyId { get; set; }

    /// <summary>
    /// Назва агентства
    /// </summary>
    public string AgencyName { get; set; } = string.Empty;

    /// <summary>
    /// Чи активний користувач
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Кількість призначених ролей
    /// </summary>
    public int RolesCount { get; set; }
    
    /// <summary>
    /// Назви ролей користувача
    /// </summary>
    public List<string> Roles { get; set; } = new();
}