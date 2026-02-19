using TimeTracker.Core.DTOs.Roles;

namespace TimeTracker.Core.DTOs.Users;

/// <summary>
/// Детальна інформація про користувача (включаючи ролі)
/// </summary>
public class UserDetailDto
{
    /// <summary>
    /// ID користувача
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Унікальний логін
    /// </summary>
    public string Login { get; set; } = string.Empty;

    /// <summary>
    /// Email
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Повне ім'я
    /// </summary>
    public string Name { get; set; } = string.Empty;

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
    /// Дата створення облікового запису (UTC)
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Дата останнього оновлення (UTC)
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Список ролей користувача
    /// </summary>
    public List<RoleDto> Roles { get; set; } = new();
}