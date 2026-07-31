using EPR.LiveService.FunctionApp.PendingChanges;

namespace EPR.LiveService.FunctionApp.Formatting;

public static class PendingChangeDetailsPage
{
    public static string Build(PendingChangeDetailsRequest? values = null) => TemplateRenderer.Render(
        "PendingChangeDetails.sbn",
        new
        {
            BearerToken = values?.BearerToken ?? string.Empty,
            RegulatorEmail = values?.RegulatorEmail ?? string.Empty,
            UserEmail = values?.UserEmail ?? string.Empty,
            UserOrganisationId = values?.UserOrganisationId ?? string.Empty,
            Accepted = values?.RegulatorResponse?.Equals("Accepted", StringComparison.OrdinalIgnoreCase) == true,
            Rejected = values?.RegulatorResponse?.Equals("Rejected", StringComparison.OrdinalIgnoreCase) == true,
            RegulatorComments = values?.RegulatorComments ?? string.Empty
        });
}
