using System.Net;
using System.Text;
using EPR.LiveService.FunctionApp.Queries;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace EPR.LiveService.FunctionApp.Functions;

public class ListQueriesFunction
{
    private readonly IQueryRegistry _registry;

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
        var items = new StringBuilder();
        foreach (var definition in definitions.OrderBy(d => d.DisplayName))
        {
            items.AppendLine($"""
                <li>
                    <a href="/api/query/{WebUtility.UrlEncode(definition.Id)}">{WebUtility.HtmlEncode(definition.DisplayName)}</a>
                    <p>{WebUtility.HtmlEncode(definition.Description)}</p>
                </li>
                """);
        }

        return $$"""
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
                {{items}}
            </ul>
        </body>
        </html>
        """;
    }
}