using TimeTracker.Core.DTOs.Dictionaries;

namespace TimeTracker.Core.Services.Dictionaries;

public interface IDictionaryService<TDto, TCreateDto, TUpdateDto>
    where TDto : DictionaryDto
    where TCreateDto : class
    where TUpdateDto : class
{
    // Базові CRUD операції
    Task<TDto?> GetByIdAsync(long id);
    Task<TDto?> GetByNameAsync(string name);
    Task<IEnumerable<TDto>> GetAllAsync();
    Task<IEnumerable<TDto>> GetActiveAsync();
    
    // CRUD з аудитом
    Task<TDto> CreateAsync(
        TCreateDto dto, 
        long userId, 
        string userName, 
        string ipAddress, 
        string userAgent);
    
    Task<TDto> UpdateAsync(
        long id, 
        TUpdateDto dto, 
        long userId, 
        string userName, 
        string ipAddress, 
        string userAgent);
    
    Task DeleteAsync(
        long id, 
        long userId, 
        string userName, 
        string ipAddress, 
        string userAgent);
    
    // Soft Delete операції з аудитом - додано параметри
    Task ActivateAsync(
        long id, 
        long userId, 
        string userName, 
        string ipAddress, 
        string userAgent);
    
    Task DeactivateAsync(
        long id, 
        long userId, 
        string userName, 
        string ipAddress, 
        string userAgent);
    
    // Валідація
    Task<bool> IsNameExistsAsync(string name, long? excludeId = null);
    Task<bool> CanBeDeactivatedAsync(long id);
    
    // Статистика
    Task<int> GetActiveCountAsync();
    Task<int> GetTotalCountAsync();
    
    // Пагінація
    Task<(IEnumerable<TDto> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? searchTerm = null,
        bool? isActive = null);
}