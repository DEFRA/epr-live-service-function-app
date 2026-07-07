namespace EPR.LiveService.FunctionApp.Repositories.Interfaces;

public interface IOrganisationRepository
{
    Task<IEnumerable<dynamic>> GetOrganisationByOrgRefAsync(string orgRef);
}