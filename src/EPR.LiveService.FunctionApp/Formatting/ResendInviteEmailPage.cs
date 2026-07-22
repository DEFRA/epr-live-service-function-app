namespace EPR.LiveService.FunctionApp.Formatting;

public static class ResendInviteEmailPage
{
    public static string Build() => TemplateRenderer.Render(
        "ResendInviteEmail.sbn",
        new { });
}
