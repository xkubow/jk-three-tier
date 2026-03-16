using JK.Platform.Core.Abstraction;
using JK.Platform.Core.AspNetCore.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace JK.Platform.Http.Configurations;

public abstract class HttpStartupConfigurator : IStartupConfigurator
{
    public virtual void Initialize(IConfiguration configuration)
    {
    }

    public virtual void ConfigureOptions(IServiceCollection services, IConfiguration configuration)
    {
    }

    public virtual void ConfigureHealthChecks(IHealthChecksBuilder healthChecks, IConfiguration configuration)
    {
    }

    public virtual void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddGrpc();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = GetApiTitle(configuration), Version = "v1" });
        });
    }

    public virtual void ConfigureMiddleware(IApplicationBuilder app, IConfiguration configuration)
    {
        app.UseHttpsRedirection();
    }

    public virtual void ConfigureEndpoints(
        IEndpointRouteBuilder endpoints,
        IApplicationBuilder app,
        IConfiguration configuration)
    {
        endpoints.MapControllers();
    }

    protected abstract string GetApiTitle(IConfiguration configuration);
}
