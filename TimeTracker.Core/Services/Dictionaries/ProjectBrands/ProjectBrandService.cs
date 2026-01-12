using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TimeTracker.Core.DTOs.Dictionaries.ProjectBrands;
using TimeTracker.Data.Entities;
using TimeTracker.Data.Repositories.Dictionaries;
using TimeTracker.Data.UnitOfWork;

namespace TimeTracker.Core.Services.Dictionaries.ProjectBrands;

public class ProjectBrandService : DictionaryService<ProjectBrand, ProjectBrandDto, CreateProjectBrandDto, UpdateProjectBrandDto>, IProjectBrandService
{
    public ProjectBrandService(
        IDictionaryRepository<ProjectBrand> repository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<ProjectBrandService> logger)
        : base(repository, unitOfWork, mapper, logger)
    {
    }

    protected override async Task<bool> CanBeDeletedAsync(long id)
    {
        var hasTimeEntries = await _unitOfWork.TimeEntries
            .GetQueryable()
            .AnyAsync(te => te.ProjectBrandId == id);

        return !hasTimeEntries;
    }

    public override async Task<bool> CanBeDeactivatedAsync(long id)
    {
        var hasActiveTimeEntries = await _unitOfWork.TimeEntries
            .GetQueryable()
            .AnyAsync(te => te.ProjectBrandId == id);

        return !hasActiveTimeEntries;
    }
}