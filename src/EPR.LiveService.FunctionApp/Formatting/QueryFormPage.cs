using System.Text.Json;
using EPR.LiveService.FunctionApp.Queries;

namespace EPR.LiveService.FunctionApp.Formatting;

public static class QueryFormPage
{
    public static string Build(QueryDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);

        var outputs = definition.Outputs
            .Select((format, index) => new
            {
                Key = format.Key(),
                DisplayName = format.DisplayName(),
                Hint = format.Hint(),
                Checked = index == 0
            })
            .ToArray();

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
                IsSelect = param.Type == "select",
                InputType = param.Type switch
                {
                    "number" => "number",
                    "date" => "date",
                    _ => "text"
                },
                Options = param.Type == "select"
                    ? param.Options?.Select((option, index) => new
                    {
                        option.Value,
                        option.Label,
                        Checked = index == 0
                    }).ToArray()
                    : null
            }).ToArray(),
            Outputs = outputs,
            ShowOutputPicker = outputs.Length > 1,
            DefaultOutput = outputs.Length > 0 ? outputs[0].Key : QueryOutputFormat.Csv.Key()
        };

        return TemplateRenderer.Render("QueryForm.sbn", model);
    }
}