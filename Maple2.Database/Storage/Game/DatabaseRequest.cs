using Maple2.Database.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace Maple2.Database.Storage;

public abstract class DatabaseRequest<TContext>(TContext context, ILogger logger) : IDisposable
    where TContext : DbContext {
    protected readonly TContext Context = context;
    protected readonly ILogger Logger = logger;

    private IDbContextTransaction? transaction;
    public bool IsTransaction => transaction != null;

    public void BeginTransaction() {
        transaction = Context.Database.BeginTransaction();
    }

    public bool Commit() {
        if (transaction == null) {
            return false;
        }

        transaction.Commit();
        transaction = null; // transaction is completed.
        return true;
    }

    public bool SaveChanges() {
        return Context.TrySaveChanges();
    }

    public void Dispose() {
        Commit();
        Context.Dispose();
    }
}
