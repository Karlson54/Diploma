using AutoMapper;
using TimeTracker.Core.DTOs.Dictionaries.Agencies;
using TimeTracker.Core.DTOs.Dictionaries.Clients;
using TimeTracker.Core.DTOs.Dictionaries.ContractingAgencies;
using TimeTracker.Core.DTOs.Dictionaries.JobTypes;
using TimeTracker.Core.DTOs.Dictionaries.Markets;
using TimeTracker.Core.DTOs.Dictionaries.Media;
using TimeTracker.Data.Entities;

namespace TimeTracker.Core.Mappings;

public class DictionaryMappingProfile : Profile
{
    public DictionaryMappingProfile()
    {
        // Market mappings
        CreateMap<Market, MarketDto>();
        CreateMap<CreateMarketDto, Market>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.TimeEntries, opt => opt.Ignore());
        CreateMap<UpdateMarketDto, Market>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.TimeEntries, opt => opt.Ignore());

        // Media mappings
        CreateMap<Media, MediaDto>();
        CreateMap<CreateMediaDto, Media>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.TimeEntries, opt => opt.Ignore());
        CreateMap<UpdateMediaDto, Media>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.TimeEntries, opt => opt.Ignore());

        // JobType mappings
        CreateMap<JobType, JobTypeDto>();
        CreateMap<CreateJobTypeDto, JobType>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.TimeEntries, opt => opt.Ignore());
        CreateMap<UpdateJobTypeDto, JobType>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.TimeEntries, opt => opt.Ignore());

        // ContractingAgency mappings
        CreateMap<ContractingAgency, ContractingAgencyDto>();
        CreateMap<CreateContractingAgencyDto, ContractingAgency>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.TimeEntries, opt => opt.Ignore());
        CreateMap<UpdateContractingAgencyDto, ContractingAgency>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.TimeEntries, opt => opt.Ignore());

        // Agency mappings
        CreateMap<Agency, AgencyDto>()
            .ForMember(dest => dest.UsersCount, opt => opt.Ignore());
        CreateMap<CreateAgencyDto, Agency>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Users, opt => opt.Ignore())
            .ForMember(dest => dest.TimeEntries, opt => opt.Ignore());
        CreateMap<UpdateAgencyDto, Agency>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Users, opt => opt.Ignore())
            .ForMember(dest => dest.TimeEntries, opt => opt.Ignore());

        // Client mappings
        CreateMap<Client, ClientDto>();
        CreateMap<CreateClientDto, Client>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.TimeEntries, opt => opt.Ignore());
        CreateMap<UpdateClientDto, Client>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.TimeEntries, opt => opt.Ignore());
    }
}