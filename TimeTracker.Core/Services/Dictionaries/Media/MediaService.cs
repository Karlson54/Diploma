using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TimeTracker.Core.DTOs.Dictionaries.Media;
using TimeTracker.Data.Repositories.Dictionaries;
using TimeTracker.Data.UnitOfWork;

namespace TimeTracker.Core.Services.Dictionaries.Media;

public class MediaService : DictionaryService<Data.Entities.Media, MediaDto, CreateMediaDto, UpdateMediaDto>, IMediaService
{
    public MediaService(
        IDictionaryRepository<Data.Entities.Media> repository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<MediaService> logger)
        : base(repository, unitOfWork, mapper, logger)
    {
    }

    protected override async Task<bool> CanBeDeletedAsync(long id)
    {
        var hasTimeEntries = await _unitOfWork.TimeEntries
            .GetQueryable()
            .AnyAsync(te => te.MediaId == id);

        return !hasTimeEntries;
    }

    public override async Task<bool> CanBeDeactivatedAsync(long id)
    {
        var hasActiveTimeEntries = await _unitOfWork.TimeEntries
            .GetQueryable()
            .AnyAsync(te => te.MediaId == id);

        return !hasActiveTimeEntries;
    }
}