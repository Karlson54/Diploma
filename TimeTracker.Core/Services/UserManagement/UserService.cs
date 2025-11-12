using AutoMapper;
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
        var users = await _userRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<UserListItemDto>>(users);
    }

    public async Task<IEnumerable<UserListItemDto>> GetActiveUsersAsync()
    {
        var users = await _userRepository.GetActiveUsersAsync();
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

        var (users, totalCount) = await _userRepository.GetUsersPagedAsync(
            pageNumber, pageSize, searchTerm, agencyId, isActive);

        var userDtos = _mapper.Map<IEnumerable<UserListItemDto>>(users);

        return (userDtos, totalCount);
    }

    public async Task<UserDto> CreateAsync(CreateUserDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Login))
            throw new ArgumentException("Login не може бути порожнім");

        if (string.IsNullOrWhiteSpace(dto.Email))
            throw new ArgumentException("Email не може бути порожнім");

        if (string.IsNullOrWhiteSpace(dto.Password))
            throw new ArgumentException("Password не може бути порожнім");

        if (dto.Password.Length < 6)
            throw new ArgumentException("Password має бути мінімум 6 символів");

        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentException("Name не може бути порожнім");

        if (await _userRepository.IsEmailExistsAsync(dto.Email))
            throw new InvalidOperationException("Email вже використовується");

        if (await _userRepository.IsLoginExistsAsync(dto.Login))
            throw new InvalidOperationException("Login вже використовується");

        var user = _mapper.Map<User>(dto);
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
        user.IsActive = true;

        await _userRepository.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        var createdUser = await _userRepository.GetByIdAsync(user.Id);
        return _mapper.Map<UserDto>(createdUser);
    }

    public async Task<UserDto> UpdateAsync(long id, UpdateUserDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email))
            throw new ArgumentException("Email не може бути порожнім");

        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentException("Name не може бути порожнім");

        var user = await _userRepository.GetByIdAsync(id);
        if (user == null)
            throw new KeyNotFoundException($"Користувача з ID {id} не знайдено");

        if (await _userRepository.IsEmailExistsAsync(dto.Email, id))
            throw new InvalidOperationException("Email вже використовується іншим користувачем");

        _mapper.Map(dto, user);
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
        if (string.IsNullOrWhiteSpace(dto.CurrentPassword))
            throw new ArgumentException("Поточний пароль не може бути порожнім");

        if (string.IsNullOrWhiteSpace(dto.NewPassword))
            throw new ArgumentException("Новий пароль не може бути порожнім");

        if (dto.NewPassword.Length < 6)
            throw new ArgumentException("Новий пароль має бути мінімум 6 символів");

        if (dto.NewPassword != dto.ConfirmPassword)
            throw new ArgumentException("Новий пароль та підтвердження не збігаються");

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new KeyNotFoundException($"Користувача з ID {userId} не знайдено");

        if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
            throw new UnauthorizedAccessException("Поточний пароль невірний");

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