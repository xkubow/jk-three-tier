using Microsoft.EntityFrameworkCore;

namespace JK.Platform.Persistence.EfCore;

public interface IUnitOfWorkFactory<TDbContext>
    where TDbContext : DbContextBase
{
    IUnitOfWork<TDbContext> Create();
}