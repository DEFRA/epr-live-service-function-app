using EPR.LiveService.FunctionApp;
using Microsoft.Data.SqlClient;
using System.Data;

namespace EPR.LiveService.Functions.Repositories;

public abstract class RepositoryBase
{
    protected readonly ConnectionStrings _connectionStrings;
    protected readonly IUnitOfWork? _uow;

    protected RepositoryBase(ConnectionStrings connectionStrings, IUnitOfWork? uow = null)
    {
        _connectionStrings = connectionStrings;
        _uow = uow;
    }

    protected async Task<T> WithConnectionAsync<T>(
        Func<IDbConnection, IDbTransaction?, Task<T>> getData)
    {
        if (_uow != null)
        {
            return await getData(_uow.Connection, _uow.Transaction);
        }
        else
        {
            await using var connection = new SqlConnection(_connectionStrings.SqlDbConnectionString);
            await connection.OpenAsync();
            return await getData(connection, null);
        }
    }

    protected async Task WithConnectionAsync(
        Func<IDbConnection, IDbTransaction?, Task> action)
    {
        if (_uow != null)
        {
            await action(_uow.Connection, _uow.Transaction);
        }
        else
        {
            await using var connection = new SqlConnection(_connectionStrings.SqlDbConnectionString);
            await connection.OpenAsync();
            await action(connection, null);
        }
    }
}
