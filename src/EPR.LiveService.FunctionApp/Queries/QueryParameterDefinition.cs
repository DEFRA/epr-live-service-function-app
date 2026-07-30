namespace EPR.LiveService.FunctionApp.Queries;

/// <summary>
/// A single choice offered by a "select"-type parameter, rendered as one radio
/// button in the generated form.
/// </summary>
public class SelectOption
{
    public string Value { get; set; } = default!;

    public string Label { get; set; } = default!;
}

/// <summary>
/// Describes a single parameter expected by a query, used both to validate/convert
/// incoming query-string values and to let the frontend render an appropriate input.
/// </summary>
public class QueryParameterDefinition
{
    public string Name { get; set; } = default!;

    public string Label { get; set; } = default!;

    /// <summary>
    /// One of "text" | "number" | "date" | "select". Drives both server-side type
    /// conversion and the frontend's choice of input element. "select" renders as
    /// a radio-button group instead of a free-text/number/date input.
    /// </summary>
    public string Type { get; set; } = "text";

    public bool Required { get; set; } = true;

    /// <summary>
    /// Only populated when Type == "select" — the set of choices to render as
    /// radio buttons. Each option's Value is submitted as the parameter's raw
    /// string value, same as any other text parameter.
    /// </summary>
    public List<SelectOption>? Options { get; set; }
}