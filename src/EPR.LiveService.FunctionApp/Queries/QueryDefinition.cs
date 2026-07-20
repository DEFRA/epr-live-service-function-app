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

    /// <summary>
    /// The output formats this query supports. Drives which radio buttons the
    /// query form renders — if only one is declared, the form skips the radio
    /// group entirely and submits that format as a hidden field. Defaults to
    /// every known format when omitted from the .json definition
    /// </summary>
    public List<QueryOutputFormat> Outputs { get; set; } = [.. Enum.GetValues<QueryOutputFormat>()];
}