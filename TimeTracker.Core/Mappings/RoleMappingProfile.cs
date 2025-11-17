using AutoMapper;
using TimeTracker.Core.DTOs.Roles;
using TimeTracker.Data.Entities;

namespace TimeTracker.Core.Mappings;

public class RoleMappingProfile : Profile
{
    public RoleMappingProfile()
    {
        CreateMap<Role, RoleDto>();

        CreateMap<Role, RoleDetailDto>()
            .ForMember(dest => dest.UsersCount, opt => opt.Ignore())
            .ForMember(dest => dest.PermissionsList, opt => opt.Ignore());

        CreateMap<Role, RoleListItemDto>()
            .ForMember(dest => dest.UsersCount, opt => opt.Ignore());

        CreateMap<User, UserInRoleDto>()
            .ForMember(dest => dest.AgencyName,
                opt => opt.MapFrom(src => src.Agency != null ? src.Agency.Name : string.Empty))
            .ForMember(dest => dest.AssignedAt,
                opt => opt.MapFrom(src => src.CreatedAt));
    }
}