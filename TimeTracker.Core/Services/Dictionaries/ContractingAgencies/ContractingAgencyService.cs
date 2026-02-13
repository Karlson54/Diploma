using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TimeTracker.Core.DTOs.Dictionaries.ContractingAgencies;
using TimeTracker.Core.Services.Audit;
using TimeTracker.Data.Entities;
using TimeTracker.Data.Repositories.Dictionaries;
using TimeTracker.Data.UnitOfWork;

namespace TimeTracker.Core.Services.Dictionaries.ContractingAgencies;

public class ContractingAgencyService : DictionaryService<ContractingAgency, ContractingAgencyDto, CreateContractingAgencyDto, UpdateContractingAgencyDto>, IContractingAgencyService
{
    public ContractingAgencyService(
        IDictionaryRepository<ContractingAgency> repository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<ContractingAgencyService> logger,
        IAuditService auditService)
        : base(repository, unitOfWork, mapper, logger, auditService)
    {
    }

    protected override async Task<bool> CanBeDeletedAsync(long id)
    {
        var hasTimeEntries = await _unitOfWork.TimeEntries
            .GetQueryable()
            .AnyAsync(te => te.ContractingAgencyId == id);

        return !hasTimeEntries;
    }

    public override async Task<bool> CanBeDeactivatedAsync(long id)
    {
        var hasActiveTimeEntries = await _unitOfWork.TimeEntries
            .GetQueryable()
            .AnyAsync(te => te.ContractingAgencyId == id);

        return !hasActiveTimeEntries;
    }

    //DeleteAsync для детальних помилок
    public override async Task DeleteAsync(
        long id,
        long userId,
        string userName,
        string ipAddress,
        string userAgent)
    {
        var contractingAgency = await _repository.GetByIdAsync(id);
        if (contractingAgency == null)
        {
            throw new KeyNotFoundException($"ContractingAgency з ID {id} не знайдено");
        }

        // Перевірка наявності TimeEntries
        var timeEntriesCount = await _unitOfWork.TimeEntries
            .GetQueryable()
            .CountAsync(te => te.ContractingAgencyId == id);
        
        if (timeEntriesCount > 0)
        {
            _logger.LogWarning(
                "Спроба видалення ContractingAgency '{Name}' (ID: {Id}), який має {Count} TimeEntries",
                contractingAgency.Name, id, timeEntriesCount);
            
            throw new InvalidOperationException(
                $"Неможливо видалити ContractingAgency '{contractingAgency.Name}', " +
                $"оскільки до нього прив'язано {timeEntriesCount} записів часу. " +
                "Спочатку видаліть або змініть всі пов'язані записи.");
        }

        // Зберігаємо дані для аудиту
        var oldValues = new
        {
            contractingAgency.Id,
            contractingAgency.Name,
            contractingAgency.IsActive
        };

        await _repository.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "ContractingAgency видалено. Id: {Id}, Name: {Name}",
            id, contractingAgency.Name);

        //АУДИТ В БД
        await _auditService.LogDeleteAsync(
            entityName: _entityName,
            entityId: id,
            oldValues: oldValues,
            userId: userId,
            userName: userName,
            ipAddress: ipAddress,
            userAgent: userAgent);
    }

    //DeactivateAsync для детальних помилок
    public override async Task DeactivateAsync(
        long id,
        long userId,
        string userName,
        string ipAddress,
        string userAgent)
    {
        var contractingAgency = await _repository.GetByIdAsync(id);
        if (contractingAgency == null)
        {
            throw new KeyNotFoundException($"ContractingAgency з ID {id} не знайдено");
        }

        if (!contractingAgency.IsActive)
        {
            _logger.LogWarning(
                "Спроба деактивації вже деактивованого ContractingAgency '{Name}' (ID: {Id})",
                contractingAgency.Name, id);
            throw new InvalidOperationException("ContractingAgency вже деактивоване");
        }

        // Перевірка активних TimeEntries
        var activeTimeEntriesCount = await _unitOfWork.TimeEntries
            .GetQueryable()
            .CountAsync(te => te.ContractingAgencyId == id);

        if (activeTimeEntriesCount > 0)
        {
            _logger.LogWarning(
                "Неможливо деактивувати ContractingAgency '{Name}' (ID: {Id}) - є {Count} записів часу",
                contractingAgency.Name, id, activeTimeEntriesCount);
            
            throw new InvalidOperationException(
                $"Неможливо деактивувати ContractingAgency '{contractingAgency.Name}', " +
                $"оскільки до нього прив'язано {activeTimeEntriesCount} записів часу. " +
                "Спочатку видаліть або змініть всі пов'язані записи.");
        }

        await _repository.DeactivateAsync(id);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "ContractingAgency деактивовано. Id: {Id}, Name: {Name}",
            id, contractingAgency.Name);

        //АУДИТ В БД
        await _auditService.LogDictionaryDeactivatedAsync(
            dictionaryType: _entityName,
            dictionaryId: id,
            dictionaryName: contractingAgency.Name,
            userId: userId,
            userName: userName,
            ipAddress: ipAddress,
            userAgent: userAgent);
    }
}