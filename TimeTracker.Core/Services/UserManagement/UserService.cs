using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TimeTracker.Core.Common;
using TimeTracker.Core.DTOs.Users;
using TimeTracker.Data.Entities;
using TimeTracker.Data.Repositories.Users;
using TimeTracker.Data.UnitOfWork;

namespace TimeTracker.Core.Services.UserManagement;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UserService(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<UserDetailDto?> GetByIdAsync(long id)
    {
        var user = await _userRepository.GetByIdWithRolesAsync(id);
        if (user == null)
            return null;

        return _mapper.Map<UserDetailDto>(user);
    }

    public async Task<IEnumerable<UserListItemDto>> GetAllAsync()
    {
        var users = await _userRepository
            .GetQueryable()
            .Include(u => u.Agency)
            .Include(u => u.UserRoles)
            .AsNoTracking()
            .OrderBy(u => u.Name)
            .ToListAsync();

        return _mapper.Map<IEnumerable<UserListItemDto>>(users);
    }

    public async Task<IEnumerable<UserListItemDto>> GetActiveUsersAsync()
    {
        var users = await _userRepository
            .GetQueryable()
            .Include(u => u.Agency)
            .Include(u => u.UserRoles)
            .Where(u => u.IsActive)
            .AsNoTracking()
            .OrderBy(u => u.Name)
            .ToListAsync();

        return _mapper.Map<IEnumerable<UserListItemDto>>(users);
    }

    public async Task<(IEnumerable<UserListItemDto> Users, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? searchTerm = null,
        long? agencyId = null,
        bool? isActive = null)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        var query = _userRepository
            .GetQueryable()
            .Include(u => u.Agency)
            .Include(u => u.UserRoles)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(u =>
                u.Name.ToLower().Contains(term) ||
                u.Email.ToLower().Contains(term) ||
                u.Login.ToLower().Contains(term));
        }

        if (agencyId.HasValue)
        {
            query = query.Where(u => u.AgencyId == agencyId.Value);
        }

        if (isActive.HasValue)
        {
            query = query.Where(u => u.IsActive == isActive.Value);
        }

        var totalCount = await query.CountAsync();

        var users = await query
            .OrderBy(u => u.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var userDtos = _mapper.Map<IEnumerable<UserListItemDto>>(users);

        return (userDtos, totalCount);
    }

    public async Task<UserDto> CreateAsync(CreateUserDto dto)
    {
        if (await _userRepository.IsEmailExistsAsync(dto.Email))
            throw new InvalidOperationException("Email вже використовується");

        if (await _userRepository.IsLoginExistsAsync(dto.Login))
            throw new InvalidOperationException("Login вже використовується");

        var agency = await _unitOfWork.Agencies.GetByIdAsync(dto.AgencyId);
        if (agency == null)
            throw new KeyNotFoundException($"Agency з ID {dto.AgencyId} не знайдено");

        if (!agency.IsActive)
            throw new InvalidOperationException("Неможливо створити користувача для неактивного Agency");

        List<long> roleIdsToAssign;

        if (!dto.RoleId.Any())
        {
            var employeeRole = await _unitOfWork.Roles
                .GetQueryable()
                .FirstOrDefaultAsync(r => r.Name == SystemRoles.Employee && r.IsActive);

            if (employeeRole == null)
                throw new InvalidOperationException("Роль Employee не знайдена");

            roleIdsToAssign = new List<long> { employeeRole.Id };
        }
        else
        {
            var roles = await _unitOfWork.Roles
                .GetQueryable()
                .Where(r => dto.RoleId.Contains(r.Id) && r.IsActive)
                .ToListAsync();

            if (roles.Count != dto.RoleId.Count)
            {
                var foundIds = roles.Select(r => r.Id);
                var missingIds = dto.RoleId.Except(foundIds);
                throw new KeyNotFoundException(
                    $"Ролі з ID {string.Join(", ", missingIds)} не знайдено або неактивні");
            }

            roleIdsToAssign = dto.RoleId;
        }

        var user = _mapper.Map<User>(dto);
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
        user.IsActive = true;

        await _userRepository.AddAsync(user);

        foreach (var roleId in roleIdsToAssign)
        {
            user.UserRoles.Add(new UserRole
            {
                User = user,
                RoleId = roleId,
                CreatedAt = DateTime.UtcNow
            });
        }

        await _unitOfWork.SaveChangesAsync();

        var createdUser = await _userRepository
            .GetQueryable()
            .Include(u => u.Agency)
            .FirstOrDefaultAsync(u => u.Id == user.Id);

        return _mapper.Map<UserDto>(createdUser);
    }

    public async Task<UserDto> UpdateAsync(long id, UpdateUserDto dto)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
            throw new KeyNotFoundException($"Користувача з ID {id} не знайдено");

        if (await _userRepository.IsEmailExistsAsync(dto.Email, id))
            throw new InvalidOperationException("Email вже використовується іншим користувачем");

        var agency = await _unitOfWork.Agencies.GetByIdAsync(dto.AgencyId);
        if (agency == null)
            throw new KeyNotFoundException($"Agency з ID {dto.AgencyId} не знайдено");

        if (!agency.IsActive)
            throw new InvalidOperationException("Неможливо призначити користувача до неактивного Agency");

        user.Email = dto.Email.Trim();
        user.Name = dto.Name.Trim();
        user.AgencyId = dto.AgencyId;

        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<UserDto>(user);
    }

    public async Task ActivateAsync(long id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
            throw new KeyNotFoundException($"Користувача з ID {id} не знайдено");

        if (user.IsActive)
            throw new InvalidOperationException("Користувач вже активний");

        user.IsActive = true;
        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeactivateAsync(long id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
            throw new KeyNotFoundException($"Користувача з ID {id} не знайдено");

        if (!user.IsActive)
            throw new InvalidOperationException("Користувач вже деактивований");

        user.IsActive = false;
        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task ChangePasswordAsync(long userId, ChangePasswordDto dto)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new KeyNotFoundException($"Користувача з ID {userId} не знайдено");

        if (!user.IsActive)
            throw new InvalidOperationException("Неможливо змінити пароль неактивного користувача");

        if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
            throw new UnauthorizedAccessException("Поточний пароль невірний");

        if (BCrypt.Net.BCrypt.Verify(dto.NewPassword, user.PasswordHash))
            throw new InvalidOperationException("Новий пароль не може співпадати зі старим");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<bool> IsEmailExistsAsync(string email, long? excludeUserId = null)
    {
        if (excludeUserId.HasValue)
            return await _userRepository.IsEmailExistsAsync(email, excludeUserId.Value);

        return await _userRepository.IsEmailExistsAsync(email);
    }

    public async Task<bool> IsLoginExistsAsync(string login, long? excludeUserId = null)
    {
        if (excludeUserId.HasValue)
            return await _userRepository.IsLoginExistsAsync(login, excludeUserId.Value);

        return await _userRepository.IsLoginExistsAsync(login);
    }
}