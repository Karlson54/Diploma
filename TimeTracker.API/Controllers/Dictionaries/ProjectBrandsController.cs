using Microsoft.AspNetCore.Mvc;
using TimeTracker.Core.DTOs.Dictionaries.ProjectBrands;
using TimeTracker.Core.Services.Dictionaries.ProjectBrands;

namespace TimeTracker.API.Controllers.Dictionaries;

[Route("api/[controller]")]
public class ProjectBrandsController : BaseDictionaryController<ProjectBrandDto, CreateProjectBrandDto, UpdateProjectBrandDto>
{
    public ProjectBrandsController(
        IProjectBrandService service,
        ILogger<ProjectBrandsController> logger)
        : base(service, logger, "ProjectBrand")
    {
    }
}