using EPR.LiveService.FunctionApp.Models;

namespace EPR.LiveService.FunctionApp.Repositories.Interfaces;

public interface IOrganisationRepository
{
    Task<GetOrganisationByOrgRefResults?> GetOrganisationByOrgRefAsync(string orgRef);
}