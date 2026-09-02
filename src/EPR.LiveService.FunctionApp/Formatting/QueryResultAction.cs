namespace EPR.LiveService.FunctionApp.Formatting;

public record QueryResultAction(
    string Label,
    string Url,
    IReadOnlyList<QueryResultActionField>? Fields = null);

public record QueryResultActionField(string Name, string Value);
