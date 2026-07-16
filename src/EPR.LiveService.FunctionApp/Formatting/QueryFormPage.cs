using System.Text.Json;
using EPR.LiveService.FunctionApp.Queries;

namespace EPR.LiveService.FunctionApp.Formatting;

public static class QueryFormPage
{
    public static string Build(QueryDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);

        var model = new
        {
            definition.Id,
            definition.DisplayName,
            definition.Description,
            QueryIdJson = JsonSerializer.Serialize(definition.Id),
            Parameters = definition.Parameters.Select(param => new
            {
                param.Name,
                param.Label,
                param.Required,
                InputType = param.Type switch
                {
                    "number" => "number",
                    "date" => "date",
                    _ => "text"
                }
            }).ToArray()
        };

        return TemplateRenderer.Render("QueryForm.sbn", model);
    }
}
