namespace EPR.LiveService.FunctionApp.Queries;

public interface IQueryRegistry
{
    /// <summary>
    /// Looks up a query definition by id. Throws KeyNotFoundException if not registered.
    /// </summary>
    QueryDefinition Get(string id);

    /// <summary>
    /// All registered query definitions, used to power the /api/queries listing endpoint.
    /// </summary>
    IEnumerable<QueryDefinition> All();

    /// <summary>
    /// Loads the raw SQL text for a given query id from its embedded .sql resource.
    /// </summary>
    Task<string> LoadScriptAsync(string queryId);
}