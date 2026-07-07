using EPR.LiveService.FunctionApp.Models;

namespace EPR.LiveService.FunctionApp.Repositories.Interfaces;

public interface IOrganisationRepository
{
    Task<IEnumerable<GetOrganisationByOrgRefResults>> GetOrganisationByOrgRefAsync(string orgRef);
}