namespace TimeTracker.Core.DTOs.TimeEntries;

/// <summary>
/// Повна інформація про запис робочого часу
/// </summary>
public class TimeEntryDto
{
    /// <summary>
    /// ID запису
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// ID співробітника
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// Ім'я співробітника
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Дата запису
    /// </summary>
    public DateTime EntryDate { get; set; }

    /// <summary>
    /// ID агентства
    /// </summary>
    public long AgencyId { get; set; }

    /// <summary>
    /// Назва агентства
    /// </summary>
    public string AgencyName { get; set; } = string.Empty;

    /// <summary>
    /// ID ринку
    /// </summary>
    public long MarketId { get; set; }

    /// <summary>
    /// Назва ринку
    /// </summary>
    public string MarketName { get; set; } = string.Empty;

    /// <summary>
    /// ID компанії-підрядника
    /// </summary>
    public long ContractingAgencyId { get; set; }

    /// <summary>
    /// Назва компанії-підрядника
    /// </summary>
    public string ContractingAgencyName { get; set; } = string.Empty;

    /// <summary>
    /// ID клієнта
    /// </summary>
    public long ClientId { get; set; }

    /// <summary>
    /// Назва клієнта
    /// </summary>
    public string ClientName { get; set; } = string.Empty;

    /// <summary>
    /// Назва проекту/бренду
    /// </summary>
    public string ProjectBrandName { get; set; } = string.Empty;

    /// <summary>
    /// ID медіаканалу
    /// </summary>
    public long MediaId { get; set; }

    /// <summary>
    /// Назва медіаканалу
    /// </summary>
    public string MediaName { get; set; } = string.Empty;

    /// <summary>
    /// ID типу роботи
    /// </summary>
    public long JobTypeId { get; set; }

    /// <summary>
    /// Назва типу роботи
    /// </summary>
    public string JobTypeName { get; set; } = string.Empty;

    /// <summary>
    /// Витрачений час у мілісекундах
    /// </summary>
    public long HoursMilliseconds { get; set; }

    /// <summary>
    /// Витрачений час у вигляді TimeSpan (обчислюється автоматично)
    /// </summary>
    public TimeSpan Hours => TimeSpan.FromMilliseconds(HoursMilliseconds);

    /// <summary>
    /// Відформатований час у вигляді "Hh Mm" (обчислюється автоматично)
    /// </summary>
    public string FormattedHours => Core.Common.TimeHelper.FormatHours(HoursMilliseconds);

    /// <summary>
    /// Коментар до запису
    /// </summary>
    public string? Comments { get; set; }

    /// <summary>
    /// Дата створення запису (UTC)
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Дата останнього оновлення (UTC)
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// ID відділу
    /// </summary>
    public long DepartmentId { get; set; }

    /// <summary>
    /// Назва відділу
    /// </summary>
    public string DepartmentName { get; set; } = string.Empty;
}