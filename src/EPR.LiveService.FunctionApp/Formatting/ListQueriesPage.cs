using EPR.LiveService.FunctionApp.Queries;

namespace EPR.LiveService.FunctionApp.Formatting;

public static class ListQueriesPage
{
    public static string Build(IEnumerable<QueryDefinition> definitions)
    {
        ArgumentNullException.ThrowIfNull(definitions);

        var model = new
        {
            Definitions = definitions
                .OrderBy(definition => definition.DisplayName)
                .ToArray()
        };

        return TemplateRenderer.Render("ListQueries.sbn", model);
    }
}
