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
    
    Task<TDto> CreateAsync(TCreateDto dto);
    Task<TDto> UpdateAsync(long id, TUpdateDto dto);
    Task DeleteAsync(long id);
    
    // Soft Delete операції
    Task ActivateAsync(long id);
    Task DeactivateAsync(long id);
    
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