using EPR.LiveService.FunctionApp.Services;
using EPR.LiveService.FunctionApp.UserDetailsChange;

namespace EPR.LiveService.FunctionApp.UnitTests.TestSupport.Fakes;

internal sealed class UnreachableOrganisationService : IOrganisationService
{
    public Task<OrganisationUpdateResponse> UpdateOrganisationAsync(
        RegulatorDetails regulatorDetails,
        bool hasRegulatorAccepted,
        string regulatorComment,
        CancellationToken cancellationToken = default) =>
        throw new InvalidOperationException("UpdateOrganisationAsync was reached by a unit test — this path needs a real SQL connection first, so it belongs in an integration test.");
}
