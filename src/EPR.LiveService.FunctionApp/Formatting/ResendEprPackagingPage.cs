namespace EPR.LiveService.FunctionApp.Formatting;

public static class ResendEprPackagingPage
{
    public static string Build() => TemplateRenderer.Render(
        "ResendEprPackaging.sbn",
        new { });
}
