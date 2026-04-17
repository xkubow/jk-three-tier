using Microsoft.Extensions.DependencyInjection;

namespace JK.Platform.Persistence.EfCore.Extensions;

public static class PersistenceServiceCollectionExtensions
{
    public static IServiceCollection AddUnitOfWork(this IServiceCollection services)
    {
        services.AddScoped(typeof(IUnitOfWork<>), typeof(UnitOfWork<>));
        services.AddScoped(typeof(IUnitOfWorkFactory<>), typeof(UnitOfWorkFactory<>));

        return services;
    }
}