using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace EPR.LiveService.FunctionApp.Sql;

public class SqlConnectionFactory : ISqlConnectionFactory
{
    private readonly Dictionary<string, SqlTargetOptions> _targets;

    public SqlConnectionFactory(IOptions<Dictionary<string, SqlTargetOptions>> targets)
    {
        _targets = new Dictionary<string, SqlTargetOptions>(
            targets.Value, StringComparer.OrdinalIgnoreCase);
    }

    public async Task<SqlConnection> CreateConnectionAsync(string targetName)
    {
        if (!_targets.TryGetValue(targetName, out var target))
        {
            throw new ArgumentException(
                $"Unknown SQL target '{targetName}'. Registered targets: " +
                string.Join(", ", _targets.Keys));
        }

        var connection = new SqlConnection(target.ConnectionString);
        await connection.OpenAsync();
        return connection;
    }
}