using EPR.LiveService.FunctionApp.Formatting;
using EPR.LiveService.FunctionApp.Features;
using EPR.LiveService.FunctionApp.Queries;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;

namespace EPR.LiveService.FunctionApp.Functions;

public class ListQueriesFunction
{
    private readonly IQueryRegistry _registry;
    private readonly IFeatureRegistry _featureRegistry;

    public ListQueriesFunction(IQueryRegistry registry, IFeatureRegistry featureRegistry)
    {
        _registry = registry;
        _featureRegistry = featureRegistry;
    }

    [Function("ListQueries")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "queries")] HttpRequestData req)
    {
        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "text/html; charset=utf-8");
        await response.WriteStringAsync(Build(_registry.All(), _featureRegistry.All()));
        return response;
    }

    private static string Build(
        IEnumerable<QueryDefinition> definitions,
        IEnumerable<FeatureDefinition> features)
    {
        ArgumentNullException.ThrowIfNull(definitions);
        ArgumentNullException.ThrowIfNull(features);

        var model = new
        {
            Definitions = definitions
                .OrderBy(definition => definition.DisplayName)
                .ToArray(),
            Features = features
                .OrderBy(feature => feature.DisplayName)
                .ToArray()
        };

        return TemplateRenderer.Render("ListQueries.sbn", model);
    }
}
