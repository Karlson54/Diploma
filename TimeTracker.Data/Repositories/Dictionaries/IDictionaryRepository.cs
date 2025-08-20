// TimeTracker.Data/Repositories/Dictionaries/IDictionaryRepository.cs
using TimeTracker.Data.Entities;
using TimeTracker.Data.Repositories.Common;

namespace TimeTracker.Data.Repositories.Dictionaries;

public interface IDictionaryRepository<T> : IRepository<T> where T : DictionaryEntity
{
    // Специализированные методы для справочников
    Task<T?> GetByNameAsync(string name);
    Task<IEnumerable<T>> GetActiveAsync();
    Task<bool> IsNameExistsAsync(string name);
    Task<bool> IsNameExistsAsync(string name, long excludeId);
    
    // Статистика
    Task<int> GetActiveCountAsync();
    
    // Деактивация (мягкое удаление)
    Task DeactivateAsync(long id);
    Task ActivateAsync(long id);
    Task<bool> CanBeDeactivatedAsync(long id);
}