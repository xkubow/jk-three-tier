using Microsoft.EntityFrameworkCore;

namespace JK.Platform.Core.Persistence;

public abstract class UnitOfWork<TContext> : IUnitOfWork
    where TContext : DbContext
{
    protected readonly TContext Context;

    protected UnitOfWork(TContext context)
    {
        Context = context;
    }

    public virtual async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await Context.SaveChangesAsync(cancellationToken);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            Context.Dispose();
        }
    }
}
