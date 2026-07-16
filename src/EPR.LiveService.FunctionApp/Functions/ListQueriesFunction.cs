using EPR.LiveService.FunctionApp.Formatting;
using EPR.LiveService.FunctionApp.Queries;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;

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
