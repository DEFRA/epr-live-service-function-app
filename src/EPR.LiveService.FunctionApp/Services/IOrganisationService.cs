using EPR.LiveService.FunctionApp.PendingChanges;

namespace EPR.LiveService.FunctionApp.Services;

public interface IOrganisationService
{
    Task<string> UpdateOrganisationAsync(
        PendingChangeRegulatorDetails pendingChangeRegulatorDetails,
        bool hasRegulatorAccepted,
        string regulatorComment,
        string bearerToken,
        CancellationToken cancellationToken = default);
}
