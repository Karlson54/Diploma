using Microsoft.AspNetCore.Mvc;
using TimeTracker.Core.DTOs.Dictionaries.JobTypes;
using TimeTracker.Core.Services.Dictionaries.JobTypes;

namespace TimeTracker.API.Controllers.Dictionaries;

[Route("api/[controller]")]
public class JobTypesController : BaseDictionaryController<JobTypeDto, CreateJobTypeDto, UpdateJobTypeDto>
{
    public JobTypesController(
        IJobTypeService service,
        ILogger<JobTypesController> logger)
        : base(service, logger, "JobType")
    {
    }
}