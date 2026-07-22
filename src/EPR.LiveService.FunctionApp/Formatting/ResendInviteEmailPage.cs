namespace EPR.LiveService.FunctionApp.Formatting;

using EPR.LiveService.FunctionApp.Notifications;

public static class ResendInviteEmailPage
{
    public static string Build(ResendInviteEmailRequest? values = null) => TemplateRenderer.Render(
        "ResendInviteEmail.sbn",
        new
        {
            EmailAddress = values?.EmailAddress ?? string.Empty,
            OrganisationName = values?.OrganisationName ?? string.Empty,
            FirstName = values?.FirstName ?? string.Empty,
            LastName = values?.LastName ?? string.Empty,
            JoinTheTeamLink = values?.JoinTheTeamLink ?? string.Empty
        });
}
