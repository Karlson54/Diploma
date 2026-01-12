using TimeTracker.Core.DTOs.Dictionaries.JobTypes;

namespace TimeTracker.Core.Services.Dictionaries.JobTypes;

public interface IJobTypeService : IDictionaryService<JobTypeDto, CreateJobTypeDto, UpdateJobTypeDto>
{
}