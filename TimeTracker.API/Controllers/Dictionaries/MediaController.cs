using Microsoft.AspNetCore.Mvc;
using TimeTracker.Core.DTOs.Dictionaries.Media;
using TimeTracker.Core.Services.Dictionaries.Media;

namespace TimeTracker.API.Controllers.Dictionaries;

[Route("api/[controller]")]
public class MediaController : BaseDictionaryController<MediaDto, CreateMediaDto, UpdateMediaDto>
{
    public MediaController(
        IMediaService service,
        ILogger<MediaController> logger)
        : base(service, logger, "Media")
    {
    }
}