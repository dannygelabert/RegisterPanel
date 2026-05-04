using RegisterPanel.Application.Common;

namespace RegisterPanel.Application.Interfaces;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<Result<TResult>> ExecuteInTransactionAsync<TResult>(
        Func<CancellationToken, Task<Result<TResult>>> operation,
        CancellationToken cancellationToken = default);
}
