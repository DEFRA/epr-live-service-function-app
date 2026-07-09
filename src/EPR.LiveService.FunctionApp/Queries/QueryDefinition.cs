namespace EPR.LiveService.FunctionApp.Queries;

/// <summary>
/// Metadata for a single registered query. One instance is loaded from each
/// Queries/Definitions/{id}.json file, paired with a Queries/Scripts/{id}.sql file.
/// </summary>
public class QueryDefinition
{
    public string Id { get; set; } = default!;

    public string DisplayName { get; set; } = default!;

    public string Description { get; set; } = default!;

    /// <summary>
    /// Name of the SQL target this query runs against (e.g. "accounts"),
    /// matching a key in the SqlTargets configuration section.
    /// </summary>
    public string Target { get; set; } = default!;

    public List<QueryParameterDefinition> Parameters { get; set; } = new();
}