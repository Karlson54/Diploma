using TimeTracker.Core.DTOs.Dictionaries.Media;

namespace TimeTracker.Core.Services.Dictionaries.Media;

public interface IMediaService : IDictionaryService<MediaDto, CreateMediaDto, UpdateMediaDto>
{
}