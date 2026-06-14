using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TimeTracker.Core.DTOs.Departments;
using TimeTracker.Core.Services.Audit;
using TimeTracker.Data.Repositories.Departments;
using TimeTracker.Data.UnitOfWork;

namespace TimeTracker.Core.Services.Departments;

public class DepartmentService : IDepartmentService
{
    private readonly IDepartmentRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<DepartmentService> _logger;
    private readonly IAuditService _auditService;

    public DepartmentService(
        IDepartmentRepository repository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<DepartmentService> logger,
        IAuditService auditService)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _auditService = auditService;
    }

    public async Task<DepartmentDto?> GetByIdAsync(long id)
    {
        var department = await _repository
            .GetQueryable()
            .Include(d => d.Agency)
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == id);

        if (department == null)
            return null;

        var dto = _mapper.Map<DepartmentDto>(department);
        dto.UsersCount = await _unitOfWork.Users
            .CountAsync(u => u.DepartmentId == id);

        return dto;
    }

    public async Task<IEnumerable<DepartmentDto>> GetByAgencyAsync(long agencyId)
    {
        var departments = await _repository
            .GetQueryable()
            .Include(d => d.Agency)
            .Where(d => d.AgencyId == agencyId)
            .OrderBy(d => d.Name)
            .AsNoTracking()
            .ToListAsync();

        var dtos = _mapper.Map<List<DepartmentDto>>(departments);

        foreach (var dto in dtos)
        {
            dto.UsersCount = await _unitOfWork.Users
                .CountAsync(u => u.DepartmentId == dto.Id);
        }

        return dtos;
    }

    public async Task<IEnumerable<DepartmentDto>> GetActiveByAgencyAsync(long agencyId)
    {
        var departments = await _repository
            .GetQueryable()
            .Include(d => d.Agency)
            .Where(d => d.AgencyId == agencyId && d.IsActive)
            .OrderBy(d => d.Name)
            .AsNoTracking()
            .ToListAsync();

        var dtos = _mapper.Map<List<DepartmentDto>>(departments);

        foreach (var dto in dtos)
        {
            dto.UsersCount = await _unitOfWork.Users
                .CountAsync(u => u.DepartmentId == dto.Id);
        }

        return dtos;
    }

    public async Task<DepartmentDto> CreateAsync(
        CreateDepartmentDto dto,
        long requestingUserId,
        string requestingUserName,
        string ipAddress,
        string userAgent)
    {
        var agency = await _unitOfWork.Agencies.GetByIdAsync(dto.AgencyId);
        if (agency == null)
            throw new KeyNotFoundException($"Agency з ID {dto.AgencyId} не знайдено");

        if (!agency.IsActive)
            throw new InvalidOperationException($"Agency '{agency.Name}' деактивована");

        if (await _repository.IsNameExistsAsync(dto.Name, dto.AgencyId))
        {
            _logger.LogWarning(
                "Спроба створення відділу з існуючою назвою '{Name}' в Agency {AgencyId}",
                dto.Name, dto.AgencyId);
            throw new InvalidOperationException(
                $"Відділ з назвою '{dto.Name}' вже існує в цій агенції");
        }

        var department = _mapper.Map<Data.Entities.Department>(dto);

        await _repository.AddAsync(department);
        await _unitOfWork.SaveChangesAsync();

        await _auditService.LogCreateAsync(
            entityName: "Department",
            entityId: department.Id,
            newValues: new { department.Name, department.AgencyId, department.IsActive },
            userId: requestingUserId,
            userName: requestingUserName,
            ipAddress: ipAddress,
            userAgent: userAgent);

        return await GetByIdAsync(department.Id)
               ?? throw new InvalidOperationException("Помилка при завантаженні створеного відділу");
    }

    public async Task<DepartmentDto> UpdateAsync(
        long id,
        UpdateDepartmentDto dto,
        long requestingUserId,
        string requestingUserName,
        string ipAddress,
        string userAgent)
    {
        var department = await _repository.GetByIdAsync(id);
        if (department == null)
            throw new KeyNotFoundException($"Відділ з ID {id} не знайдено");

        if (await _repository.IsNameExistsAsync(dto.Name, department.AgencyId, excludeId: id))
        {
            _logger.LogWarning(
                "Спроба оновлення відділу {Id} з існуючою назвою '{Name}'",
                id, dto.Name);
            throw new InvalidOperationException(
                $"Відділ з назвою '{dto.Name}' вже існує в цій агенції");
        }

        var oldValues = new { department.Name, department.AgencyId };

        _mapper.Map(dto, department);
        _repository.Update(department);
        await _unitOfWork.SaveChangesAsync();

        await _auditService.LogUpdateAsync(
            entityName: "Department",
            entityId: id,
            oldValues: oldValues,
            newValues: new { department.Name, department.AgencyId },
            userId: requestingUserId,
            userName: requestingUserName,
            ipAddress: ipAddress,
            userAgent: userAgent);

        return await GetByIdAsync(id)
               ?? throw new InvalidOperationException("Помилка при завантаженні оновленого відділу");
    }

    public async Task DeleteAsync(
        long id,
        long requestingUserId,
        string requestingUserName,
        string ipAddress,
        string userAgent)
    {
        var department = await _repository.GetByIdAsync(id);
        if (department == null)
            throw new KeyNotFoundException($"Відділ з ID {id} не знайдено");

        if (await _repository.HasUsersAsync(id))
        {
            var usersCount = await _unitOfWork.Users.CountAsync(u => u.DepartmentId == id);
            _logger.LogWarning(
                "Спроба видалення відділу '{Name}' (ID: {Id}), до якого прив'язано {Count} юзерів",
                department.Name, id, usersCount);
            throw new InvalidOperationException(
                $"Неможливо видалити відділ '{department.Name}', " +
                $"оскільки до нього прив'язано {usersCount} співробітників.");
        }

        var oldValues = new { department.Name, department.AgencyId, department.IsActive };

        await _repository.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();

        await _auditService.LogDeleteAsync(
            entityName: "Department",
            entityId: id,
            oldValues: oldValues,
            userId: requestingUserId,
            userName: requestingUserName,
            ipAddress: ipAddress,
            userAgent: userAgent);
    }

    public async Task ActivateAsync(
        long id,
        long requestingUserId,
        string requestingUserName,
        string ipAddress,
        string userAgent)
    {
        var department = await _repository.GetByIdAsync(id);
        if (department == null)
            throw new KeyNotFoundException($"Відділ з ID {id} не знайдено");

        if (department.IsActive)
            throw new InvalidOperationException("Відділ вже активний");

        await _repository.ActivateAsync(id);
        await _unitOfWork.SaveChangesAsync();

        await _auditService.LogUpdateAsync(
            entityName: "Department",
            entityId: id,
            oldValues: new { department.Name, IsActive = false },
            newValues: new { department.Name, IsActive = true },
            userId: requestingUserId,
            userName: requestingUserName,
            ipAddress: ipAddress,
            userAgent: userAgent);
    }

    public async Task DeactivateAsync(
        long id,
        long requestingUserId,
        string requestingUserName,
        string ipAddress,
        string userAgent)
    {
        var department = await _repository.GetByIdAsync(id);
        if (department == null)
            throw new KeyNotFoundException($"Відділ з ID {id} не знайдено");

        if (!department.IsActive)
            throw new InvalidOperationException("Відділ вже деактивований");

        if (await _repository.HasUsersAsync(id))
        {
            var usersCount = await _unitOfWork.Users.CountAsync(u => u.DepartmentId == id);
            throw new InvalidOperationException(
                $"Неможливо деактивувати відділ '{department.Name}', " +
                $"оскільки до нього прив'язано {usersCount} активних співробітників.");
        }

        await _repository.DeactivateAsync(id);
        await _unitOfWork.SaveChangesAsync();

        await _auditService.LogUpdateAsync(
            entityName: "Department",
            entityId: id,
            oldValues: new { department.Name, IsActive = true },
            newValues: new { department.Name, IsActive = false },
            userId: requestingUserId,
            userName: requestingUserName,
            ipAddress: ipAddress,
            userAgent: userAgent);
    }
}