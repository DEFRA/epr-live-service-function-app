namespace EPR.LiveService.FunctionApp.Formatting;

/// <summary>
/// Optionally adds actions to a formatted query result without coupling the
/// query, SQL, or formatter to a particular downstream feature.
/// </summary>
public interface IQueryResultActionProvider
{
    IEnumerable<QueryResultAction> GetActions(
        string queryId,
        IReadOnlyDictionary<string, object> record);
}
