using AutoMapper;
using TimeTracker.Core.DTOs.Users;
using TimeTracker.Core.DTOs.Roles;
using TimeTracker.Data.Entities;

namespace TimeTracker.Core.Mappings;

public class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        // User -> UserDto
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.AgencyName, 
                opt => opt.MapFrom(src => src.Agency != null ? src.Agency.Name : string.Empty));

        // User -> UserDetailDto
        CreateMap<User, UserDetailDto>()
            .ForMember(dest => dest.AgencyName, 
                opt => opt.MapFrom(src => src.Agency != null ? src.Agency.Name : string.Empty))
            .ForMember(dest => dest.Roles,
                opt => opt.MapFrom(src => src.UserRoles.Select(ur => ur.Role)));

        // User -> UserListItemDto
        CreateMap<User, UserListItemDto>()
            .ForMember(dest => dest.AgencyName,
                opt => opt.MapFrom(src => src.Agency != null ? src.Agency.Name : string.Empty))
            .ForMember(dest => dest.RolesCount,
                opt => opt.MapFrom(src => src.UserRoles.Count));

        // CreateUserDto -> User
        CreateMap<CreateUserDto, User>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => true))
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Agency, opt => opt.Ignore())
            .ForMember(dest => dest.UserRoles, opt => opt.Ignore())
            .ForMember(dest => dest.TimeEntries, opt => opt.Ignore());

        // UpdateUserDto -> User
        CreateMap<UpdateUserDto, User>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Login, opt => opt.Ignore())
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Agency, opt => opt.Ignore())
            .ForMember(dest => dest.UserRoles, opt => opt.Ignore())
            .ForMember(dest => dest.TimeEntries, opt => opt.Ignore());
    }
}