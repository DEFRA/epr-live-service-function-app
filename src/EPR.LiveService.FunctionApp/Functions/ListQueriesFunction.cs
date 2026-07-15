using EPR.LiveService.FunctionApp.Queries;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Scriban;
using System.Net;
using System.Text;

namespace EPR.LiveService.FunctionApp.Functions;

public class ListQueriesFunction
{
    private readonly IQueryRegistry _registry;

    private const string AvailableQueriesTemplateText = """
<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8">
    <title>Available Queries</title>
    <style>
        body { font-family: system-ui, sans-serif; max-width: 700px; margin: 2rem auto; background: #1e1e1e; color: #d4d4d4; }
        h1 { margin-bottom: 1.5rem; }
        ul { list-style: none; padding: 0; }
        li { padding: 1rem 0; border-bottom: 1px solid #3a3a3a; }
        li:last-child { border-bottom: none; }
        a { color: #4ea1f3; font-size: 1.1rem; font-weight: 600; text-decoration: none; }
        a:hover { text-decoration: underline; }
        p { margin: 0.4rem 0 0; color: #aaaaaa; }
    </style>
</head>
<body>
    <h1>Available Queries</h1>
    <ul>
        {{ for definition in definitions }}
        <li>
            <a href="/api/query/{{ definition.id | html.url_encode | html.escape }}">{{ definition.display_name | html.escape }}</a>
            <p>{{ definition.description | html.escape }}</p>
        </li>
        {{ end }}
    </ul>
</body>
</html>
""";

    private static readonly Template AvailableQueriesTemplate =
        ParseTemplate(AvailableQueriesTemplateText);

    public ListQueriesFunction(IQueryRegistry registry) => _registry = registry;

    [Function("ListQueries")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "queries")] HttpRequestData req)
    {
        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "text/html; charset=utf-8");
        await response.WriteStringAsync(Build(_registry.All()));
        return response;
    }

    private static string Build(IEnumerable<QueryDefinition> definitions)
    {
        ArgumentNullException.ThrowIfNull(definitions);

        var model = new
        {
            Definitions = definitions
                .OrderBy(definition => definition.DisplayName)
                .ToArray()
        };

        return AvailableQueriesTemplate.Render(model);
    }

    private static Template ParseTemplate(string templateText)
    {
        var template = Template.Parse(templateText);

        if (template.HasErrors)
        {
            var errors = string.Join(
                Environment.NewLine,
                template.Messages.Select(message => message.ToString()));

            throw new InvalidOperationException(
                $"The Scriban template is invalid:{Environment.NewLine}{errors}");
        }

        return template;
    }
}