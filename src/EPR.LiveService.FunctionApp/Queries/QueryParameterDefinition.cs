namespace EPR.LiveService.FunctionApp.Queries;

/// <summary>
/// Describes a single parameter expected by a query, used both to validate/convert
/// incoming query-string values and to let the frontend render an appropriate input.
/// </summary>
public class QueryParameterDefinition
{
    public string Name { get; set; } = default!;

    public string Label { get; set; } = default!;

    /// <summary>
    /// One of "text" | "number" | "date". Drives both server-side type conversion
    /// and the frontend's choice of <input type="..."> element.
    /// </summary>
    public string Type { get; set; } = "text";

    public bool Required { get; set; } = true;
}