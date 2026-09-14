using EPR.LiveService.FunctionApp.Sql;
using Microsoft.Data.SqlClient;

namespace EPR.LiveService.FunctionApp.UnitTests.TestSupport.Fakes;

/// <summary>
/// ISqlConnectionFactory returns a concrete SqlConnection, not IDbConnection —
/// there's no seam to substitute a fake connection at, so anything past
/// CreateConnectionAsync genuinely isn't unit-testable as the code is shaped
/// today. This fake throws loudly if a test's code path ever reaches it,
/// rather than letting a test silently attempt a real network connection.
/// </summary>
internal sealed class UnreachableSqlConnectionFactory : ISqlConnectionFactory
{
    public Task<SqlConnection> CreateConnectionAsync(string targetName) =>
        throw new InvalidOperationException(
            $"CreateConnectionAsync('{targetName}') was reached by a unit test. " +
            "SqlConnection can't be faked — this code path needs an integration test instead.");
}
