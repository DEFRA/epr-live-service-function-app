using System.Diagnostics.CodeAnalysis;

namespace EPR.LiveService.FunctionApp.Sql;

/// <summary>
/// A single named SQL target, holding its full, ready-to-use connection string.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Single-property configuration POCO bound from appsettings — no logic to test.")]
public class SqlTargetOptions
{
    public string ConnectionString { get; set; } = default!;
}