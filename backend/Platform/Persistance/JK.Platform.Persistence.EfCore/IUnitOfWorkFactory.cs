using Microsoft.EntityFrameworkCore;

namespace JK.Platform.Persistence.EfCore;

public interface IUnitOfWorkFactory<TDbContext>
    where TDbContext : DbContext
{
    IUnitOfWork<TDbContext> Create();
}