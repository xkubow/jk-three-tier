using JK.Platform.Http.MIddlewares;
using JK.Platform.Http.Stores;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace JK.Platform.Http.Extensions;

public static class IdempotencyExtensions
{
    public static IServiceCollection AddIdempotency(this IServiceCollection services)
    {
        services.AddSingleton<IIdempotencyStore, IdempotencyStore>();
        return services;
    }

    public static IApplicationBuilder UseIdempotency(this IApplicationBuilder app)
    {
        return app.UseMiddleware<IdempotencyMiddleware>();
    }
}