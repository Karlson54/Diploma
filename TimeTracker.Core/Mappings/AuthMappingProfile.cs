using AutoMapper;
using TimeTracker.Core.DTOs.Auth;
using TimeTracker.Data.Entities;

namespace TimeTracker.Core.Mappings;

public class AuthMappingProfile : Profile
{
    public AuthMappingProfile()
    {
        CreateMap<RegisterDto, User>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Login, opt => opt.MapFrom(src => src.Login.Trim()))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.Trim().ToLower()))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name.Trim()))
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore()) // Хешуємо окремо в сервісі
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Agency, opt => opt.Ignore())
            .ForMember(dest => dest.UserRoles, opt => opt.Ignore())
            .ForMember(dest => dest.TimeEntries, opt => opt.Ignore());

        CreateMap<User, AuthResponseDto>()
            .ForMember(dest => dest.AgencyName, 
                opt => opt.MapFrom(src => src.Agency != null ? src.Agency.Name : string.Empty))
            .ForMember(dest => dest.Roles, opt => opt.Ignore()) // Заповнюємо в сервісі
            .ForMember(dest => dest.Token, opt => opt.Ignore()) // Генеруємо в сервісі
            .ForMember(dest => dest.ExpiresAt, opt => opt.Ignore()); // Розраховуємо в сервісі
    }
}