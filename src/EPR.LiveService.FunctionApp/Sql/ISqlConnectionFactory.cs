using Microsoft.Data.SqlClient;

namespace EPR.LiveService.FunctionApp.Sql;

public interface ISqlConnectionFactory
{
    Task<SqlConnection> CreateConnectionAsync(string targetName);
}