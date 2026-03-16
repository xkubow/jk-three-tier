using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JK.Platform.Core.Abstraction;

public interface IStartupConfigurator
{
    void Initialize(IConfiguration configuration);

    void ConfigureOptions(IServiceCollection services, IConfiguration configuration);

    void ConfigureHealthChecks(IHealthChecksBuilder healthChecks, IConfiguration configuration);

    void ConfigureServices(IServiceCollection services, IConfiguration configuration);

    void ConfigureMiddleware(IApplicationBuilder app, IConfiguration configuration);

    void ConfigureEndpoints(
        IEndpointRouteBuilder endpoints,
        IApplicationBuilder app,
        IConfiguration configuration);
}

