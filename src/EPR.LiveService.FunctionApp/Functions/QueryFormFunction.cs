using System.Net;
using EPR.LiveService.FunctionApp.Formatting;
using EPR.LiveService.FunctionApp.Queries;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace EPR.LiveService.FunctionApp.Functions;

public class QueryFormFunction
{
    private readonly IQueryRegistry _registry;

    public QueryFormFunction(IQueryRegistry registry) => _registry = registry;

    [Function("QueryForm")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "query/{queryId}")] HttpRequestData req,
        string queryId)
    {
        QueryDefinition definition;
        try
        {
            definition = _registry.Get(queryId);
        }
        catch (KeyNotFoundException ex)
        {
            var notFound = req.CreateResponse(HttpStatusCode.NotFound);
            await notFound.WriteStringAsync(ex.Message);
            return notFound;
        }

        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "text/html; charset=utf-8");
        await response.WriteStringAsync(QueryFormPage.Build(definition));
        return response;
    }
}