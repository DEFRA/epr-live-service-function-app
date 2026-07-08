namespace EPR.LiveService.FunctionApp.Sql;

/// <summary>
/// A single named SQL target, holding its full, ready-to-use connection string.
/// </summary>
public class SqlTargetOptions
{
    public string ConnectionString { get; set; } = default!;
}