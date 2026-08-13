using EPR.LiveService.FunctionApp.UserDetailsChange;

namespace EPR.LiveService.FunctionApp.Formatting;

public static class UserDetailsChangePage
{
    public static string Build(UserDetailsChangeRequest? values = null) => TemplateRenderer.Render(
        "UpdateUserDetails.sbn",
        new
        {
            RegulatorEmail = values?.RegulatorEmail ?? string.Empty,
            UserEmail = values?.UserEmail ?? string.Empty,
            UserOrganisationId = values?.UserOrganisationId ?? string.Empty,
            Accepted = values?.RegulatorResponse?.Equals("Accepted", StringComparison.OrdinalIgnoreCase) == true,
            Rejected = values?.RegulatorResponse?.Equals("Rejected", StringComparison.OrdinalIgnoreCase) == true,
            RegulatorComments = values?.RegulatorComments ?? string.Empty
        });
}
