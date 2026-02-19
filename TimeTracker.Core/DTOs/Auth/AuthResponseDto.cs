namespace TimeTracker.Core.DTOs.Auth;

/// <summary>
/// Відповідь після успішної аутентифікації або реєстрації
/// </summary>
public class AuthResponseDto
{
    /// <summary>
    /// ID користувача
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// Логін користувача
    /// </summary>
    public string Login { get; set; } = string.Empty;

    /// <summary>
    /// Email користувача
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Повне ім'я користувача
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
    /// Список ролей користувача
    /// </summary>
    public List<string> Roles { get; set; } = new();

    /// <summary>
    /// JWT токен для авторизації запитів
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Час закінчення дії токену (UTC)
    /// </summary>
    public DateTime ExpiresAt { get; set; }
}