using TimeTracker.Core.DTOs.Dictionaries.ContractingAgencies;

namespace TimeTracker.Core.Services.Dictionaries.ContractingAgencies;

public interface IContractingAgencyService : IDictionaryService<ContractingAgencyDto, CreateContractingAgencyDto,
    UpdateContractingAgencyDto>
{
}