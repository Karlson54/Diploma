using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TimeTracker.Core.DTOs.Dictionaries.ContractingAgencies;
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
        ILogger<ContractingAgencyService> logger)
        : base(repository, unitOfWork, mapper, logger)
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
}