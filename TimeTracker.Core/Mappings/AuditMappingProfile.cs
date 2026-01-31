using AutoMapper;
using TimeTracker.Core.DTOs.Audit;
using TimeTracker.Data.Entities;

namespace TimeTracker.Core.Mappings;

public class AuditMappingProfile : Profile
{
    public AuditMappingProfile()
    {
        // AuditLog -> AuditLogDto
        CreateMap<AuditLog, AuditLogDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
            .ForMember(dest => dest.Action, opt => opt.MapFrom(src => src.Action))
            .ForMember(dest => dest.EntityName, opt => opt.MapFrom(src => src.EntityName))
            .ForMember(dest => dest.EntityId, opt => opt.MapFrom(src => src.EntityId))
            .ForMember(dest => dest.OldValues, opt => opt.MapFrom(src => src.OldValues))
            .ForMember(dest => dest.NewValues, opt => opt.MapFrom(src => src.NewValues))
            .ForMember(dest => dest.IpAddress, opt => opt.MapFrom(src => src.IpAddress))
            .ForMember(dest => dest.UserAgent, opt => opt.MapFrom(src => src.UserAgent))
            .ForMember(dest => dest.Success, opt => opt.MapFrom(src => src.Success))
            .ForMember(dest => dest.ErrorMessage, opt => opt.MapFrom(src => src.ErrorMessage))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt));
    }
}