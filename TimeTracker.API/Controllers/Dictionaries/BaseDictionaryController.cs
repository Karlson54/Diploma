using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeTracker.Core.DTOs.Dictionaries;
using TimeTracker.Core.Services.Dictionaries;

namespace TimeTracker.API.Controllers.Dictionaries;

[ApiController]
[Authorize]
[Produces("application/json")]
public abstract class BaseDictionaryController<TDto, TCreateDto, TUpdateDto> : ControllerBase
    where TDto : DictionaryDto
    where TCreateDto : class
    where TUpdateDto : class
{
    protected readonly IDictionaryService<TDto, TCreateDto, TUpdateDto> _service;
    protected readonly ILogger _logger;
    protected readonly string _entityName;

    protected BaseDictionaryController(
        IDictionaryService<TDto, TCreateDto, TUpdateDto> service,
        ILogger logger,
        string entityName)
    {
        _service = service;
        _logger = logger;
        _entityName = entityName;
    }

    [HttpGet]
    [Authorize(Policy = "CanViewDictionaries")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetAll()
    {
        var items = await _service.GetAllAsync();
        return Ok(items);
    }

    [HttpGet("active")]
    [Authorize(Policy = "CanViewDictionaries")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetActive()
    {
        var items = await _service.GetActiveAsync();
        return Ok(items);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "CanViewDictionaries")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public virtual async Task<IActionResult> GetById(long id)
    {
        var item = await _service.GetByIdAsync(id);
        if (item == null)
            return NotFound(new { Message = $"{_entityName} з ID {id} не знайдено" });

        return Ok(item);
    }

    [HttpGet("by-name/{name}")]
    [Authorize(Policy = "CanViewDictionaries")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public virtual async Task<IActionResult> GetByName(string name)
    {
        var item = await _service.GetByNameAsync(name);
        if (item == null)
            return NotFound(new { Message = $"{_entityName} з назвою '{name}' не знайдено" });

        return Ok(item);
    }

    [HttpGet("paged")]
    [Authorize(Policy = "CanViewDictionaries")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetPaged(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] bool? isActive = null)
    {
        var (items, totalCount) = await _service.GetPagedAsync(
            pageNumber, pageSize, searchTerm, isActive);

        return Ok(new
        {
            Data = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        });
    }

    [HttpPost]
    [Authorize(Policy = "CanEditDictionaries")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public virtual async Task<IActionResult> Create([FromBody] TCreateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var item = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при створенні {EntityName}", _entityName);
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "CanEditDictionaries")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public virtual async Task<IActionResult> Update(long id, [FromBody] TUpdateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var item = await _service.UpdateAsync(id, dto);
            return Ok(item);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { Message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при оновленні {EntityName} {Id}", _entityName, id);
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "CanEditDictionaries")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public virtual async Task<IActionResult> Delete(long id)
    {
        try
        {
            await _service.DeleteAsync(id);
            return Ok(new { Message = $"{_entityName} успішно видалено" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { Message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при видаленні {EntityName} {Id}", _entityName, id);
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPatch("{id}/activate")]
    [Authorize(Policy = "CanEditDictionaries")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public virtual async Task<IActionResult> Activate(long id)
    {
        try
        {
            await _service.ActivateAsync(id);
            return Ok(new { Message = $"{_entityName} успішно активовано" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { Message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при активації {EntityName} {Id}", _entityName, id);
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPatch("{id}/deactivate")]
    [Authorize(Policy = "CanEditDictionaries")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public virtual async Task<IActionResult> Deactivate(long id)
    {
        try
        {
            await _service.DeactivateAsync(id);
            return Ok(new { Message = $"{_entityName} успішно деактивовано" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { Message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Помилка при деактивації {EntityName} {Id}", _entityName, id);
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpGet("check-name")]
    [Authorize(Policy = "CanEditDictionaries")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> CheckName(
        [FromQuery] string name,
        [FromQuery] long? excludeId = null)
    {
        var exists = await _service.IsNameExistsAsync(name, excludeId);
        return Ok(new { Exists = exists });
    }

    [HttpGet("{id}/can-deactivate")]
    [Authorize(Policy = "CanEditDictionaries")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> CanDeactivate(long id)
    {
        var canDeactivate = await _service.CanBeDeactivatedAsync(id);
        return Ok(new { CanDeactivate = canDeactivate });
    }

    [HttpGet("statistics")]
    [Authorize(Policy = "CanViewDictionaries")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public virtual async Task<IActionResult> GetStatistics()
    {
        var activeCount = await _service.GetActiveCountAsync();
        var totalCount = await _service.GetTotalCountAsync();

        return Ok(new
        {
            ActiveCount = activeCount,
            TotalCount = totalCount,
            InactiveCount = totalCount - activeCount
        });
    }
}