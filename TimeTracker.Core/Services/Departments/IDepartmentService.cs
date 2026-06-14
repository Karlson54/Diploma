using TimeTracker.Core.DTOs.Departments;

namespace TimeTracker.Core.Services.Departments;

public interface IDepartmentService
{
    Task<DepartmentDto?> GetByIdAsync(long id);
    Task<IEnumerable<DepartmentDto>> GetByAgencyAsync(long agencyId);
    Task<IEnumerable<DepartmentDto>> GetActiveByAgencyAsync(long agencyId);

    Task<DepartmentDto> CreateAsync(
        CreateDepartmentDto dto,
        long requestingUserId,
        string requestingUserName,
        string ipAddress,
        string userAgent);

    Task<DepartmentDto> UpdateAsync(
        long id,
        UpdateDepartmentDto dto,
        long requestingUserId,
        string requestingUserName,
        string ipAddress,
        string userAgent);

    Task DeleteAsync(
        long id,
        long requestingUserId,
        string requestingUserName,
        string ipAddress,
        string userAgent);

    Task ActivateAsync(
        long id,
        long requestingUserId,
        string requestingUserName,
        string ipAddress,
        string userAgent);

    Task DeactivateAsync(
        long id,
        long requestingUserId,
        string requestingUserName,
        string ipAddress,
        string userAgent);
}