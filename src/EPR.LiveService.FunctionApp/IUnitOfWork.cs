using System.Data;

namespace EPR.LiveService.FunctionApp;

public interface IUnitOfWork : IAsyncDisposable
{
    IDbConnection Connection { get; }
    IDbTransaction? Transaction { get; }

    void Begin();
    void Commit();
    void Rollback();

    T GetRepository<T>() where T : class;
}
