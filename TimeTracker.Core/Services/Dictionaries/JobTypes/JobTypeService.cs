using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TimeTracker.Core.DTOs.Dictionaries.JobTypes;
using TimeTracker.Data.Entities;
using TimeTracker.Data.Repositories.Dictionaries;
using TimeTracker.Data.UnitOfWork;

namespace TimeTracker.Core.Services.Dictionaries.JobTypes;

public class JobTypeService : DictionaryService<JobType, JobTypeDto, CreateJobTypeDto, UpdateJobTypeDto>, IJobTypeService
{
    public JobTypeService(
        IDictionaryRepository<JobType> repository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<JobTypeService> logger)
        : base(repository, unitOfWork, mapper, logger)
    {
    }

    protected override async Task<bool> CanBeDeletedAsync(long id)
    {
        var hasTimeEntries = await _unitOfWork.TimeEntries
            .GetQueryable()
            .AnyAsync(te => te.JobTypeId == id);

        return !hasTimeEntries;
    }

    public override async Task<bool> CanBeDeactivatedAsync(long id)
    {
        var hasActiveTimeEntries = await _unitOfWork.TimeEntries
            .GetQueryable()
            .AnyAsync(te => te.JobTypeId == id);

        return !hasActiveTimeEntries;
    }
}