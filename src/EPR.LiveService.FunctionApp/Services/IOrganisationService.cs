using EPR.LiveService.FunctionApp.UserDetailsChange;

namespace EPR.LiveService.FunctionApp.Services;

public interface IOrganisationService
{
    Task<OrganisationUpdateResponse> UpdateOrganisationAsync(
        RegulatorDetails regulatorDetails,
        bool hasRegulatorAccepted,
        string regulatorComment,
        string bearerToken,
        CancellationToken cancellationToken = default);
}
