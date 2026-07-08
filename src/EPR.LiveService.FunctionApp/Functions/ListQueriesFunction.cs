using EPR.LiveService.FunctionApp.Queries;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace EPR.LiveService.FunctionApp.Functions;

public class ListQueriesFunction
{
    private readonly IQueryRegistry _registry;

    public ListQueriesFunction(IQueryRegistry registry) => _registry = registry;

    [Function("ListQueries")]
    public IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "queries")] HttpRequest req)
    {
        var summary = _registry.All().Select(q => new
        {
            q.Id,
            q.DisplayName,
            q.Description,
            q.Parameters
        });

        return new OkObjectResult(summary);
    }
}