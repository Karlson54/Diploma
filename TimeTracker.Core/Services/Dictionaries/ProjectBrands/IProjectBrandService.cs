using TimeTracker.Core.DTOs.Dictionaries.ProjectBrands;

namespace TimeTracker.Core.Services.Dictionaries.ProjectBrands;

public interface
    IProjectBrandService : IDictionaryService<ProjectBrandDto, CreateProjectBrandDto, UpdateProjectBrandDto>
{
}