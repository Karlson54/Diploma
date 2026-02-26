using AutoMapper;
using TimeTracker.Core.DTOs.TimeEntries;
using TimeTracker.Data.Entities;

namespace TimeTracker.Core.Mappings;

public class TimeEntryMappingProfile : Profile
{
    public TimeEntryMappingProfile()
    {
        // TimeEntry -> TimeEntryDto (повний маппінг з усіма назвами)
        CreateMap<TimeEntry, TimeEntryDto>()
            .ForMember(dest => dest.UserName,
                opt => opt.MapFrom(src => src.User.Name))
            .ForMember(dest => dest.AgencyName,
                opt => opt.MapFrom(src => src.Agency.Name))
            .ForMember(dest => dest.MarketName,
                opt => opt.MapFrom(src => src.Market.Name))
            .ForMember(dest => dest.ContractingAgencyName,
                opt => opt.MapFrom(src => src.ContractingAgency.Name))
            .ForMember(dest => dest.ClientName,
                opt => opt.MapFrom(src => src.Client.Name))
            .ForMember(dest => dest.ProjectBrandName,
                opt => opt.MapFrom(src => src.ProjectBrand.Name))
            .ForMember(dest => dest.MediaName,
                opt => opt.MapFrom(src => src.Media.Name))
            .ForMember(dest => dest.JobTypeName,
                opt => opt.MapFrom(src => src.JobType.Name));

        // TimeEntry -> TimeEntryListItemDto (спрощений для списків)
        CreateMap<TimeEntry, TimeEntryListItemDto>()
            .ForMember(dest => dest.UserName,
                opt => opt.MapFrom(src => src.User.Name))
            .ForMember(dest => dest.MarketId,
                opt => opt.MapFrom(src => src.MarketId))
            .ForMember(dest => dest.MarketName,
                opt => opt.MapFrom(src => src.Market != null ? src.Market.Name : null))
            .ForMember(dest => dest.ContractingAgencyId,
                opt => opt.MapFrom(src => src.ContractingAgencyId))
            .ForMember(dest => dest.ContractingAgencyName,
                opt => opt.MapFrom(src => src.ContractingAgency != null ? src.ContractingAgency.Name : null))
            .ForMember(dest => dest.ClientId,
                opt => opt.MapFrom(src => src.ClientId))
            .ForMember(dest => dest.ClientName,
                opt => opt.MapFrom(src => src.Client != null ? src.Client.Name : null))
            .ForMember(dest => dest.ProjectBrandId,
                opt => opt.MapFrom(src => src.ProjectBrandId))
            .ForMember(dest => dest.ProjectBrandName,
                opt => opt.MapFrom(src => src.ProjectBrand != null ? src.ProjectBrand.Name : null))
            .ForMember(dest => dest.MediaId,
                opt => opt.MapFrom(src => src.MediaId))
            .ForMember(dest => dest.MediaName,
                opt => opt.MapFrom(src => src.Media != null ? src.Media.Name : null))
            .ForMember(dest => dest.JobTypeId,
                opt => opt.MapFrom(src => src.JobTypeId))
            .ForMember(dest => dest.JobTypeName,
                opt => opt.MapFrom(src => src.JobType != null ? src.JobType.Name : null))
            .ForMember(dest => dest.AgencyId,
                opt => opt.MapFrom(src => src.AgencyId))
            .ForMember(dest => dest.AgencyName,
                opt => opt.MapFrom(src => src.Agency != null ? src.Agency.Name : null));

        // TimeEntry -> TimeEntryDetailDto
        CreateMap<TimeEntry, TimeEntryDetailDto>()
            .IncludeBase<TimeEntry, TimeEntryDto>();

        // CreateTimeEntryDto -> TimeEntry
        CreateMap<CreateTimeEntryDto, TimeEntry>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore())
            .ForMember(dest => dest.Agency, opt => opt.Ignore())
            .ForMember(dest => dest.Market, opt => opt.Ignore())
            .ForMember(dest => dest.ContractingAgency, opt => opt.Ignore())
            .ForMember(dest => dest.Client, opt => opt.Ignore())
            .ForMember(dest => dest.ProjectBrand, opt => opt.Ignore())
            .ForMember(dest => dest.Media, opt => opt.Ignore())
            .ForMember(dest => dest.JobType, opt => opt.Ignore());

        // UpdateTimeEntryDto -> TimeEntry
        CreateMap<UpdateTimeEntryDto, TimeEntry>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.UserId, opt => opt.Ignore()) // UserId не змінюється при оновленні
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore())
            .ForMember(dest => dest.Agency, opt => opt.Ignore())
            .ForMember(dest => dest.Market, opt => opt.Ignore())
            .ForMember(dest => dest.ContractingAgency, opt => opt.Ignore())
            .ForMember(dest => dest.Client, opt => opt.Ignore())
            .ForMember(dest => dest.ProjectBrand, opt => opt.Ignore())
            .ForMember(dest => dest.Media, opt => opt.Ignore())
            .ForMember(dest => dest.JobType, opt => opt.Ignore());
    }
}