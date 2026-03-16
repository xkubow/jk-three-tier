using System.Text.Json.Serialization;
using Asp.Versioning;
using JK.Platform.Core.Abstraction;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JK.Platform.Rest.Server.Configurations;

public class RestServerStartupConfigurator : IStartupConfigurator
{
    public void Initialize(IConfiguration configuration)
    {
    }

    public void ConfigureOptions(IServiceCollection services, IConfiguration configuration)
    {
    }

    public void ConfigureHealthChecks(IHealthChecksBuilder healthChecks, IConfiguration configuration)
    {
    }

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddControllers(options =>
            {
                options.RespectBrowserAcceptHeader = false;
                options.OutputFormatters.RemoveType<StringOutputFormatter>();
            })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

        services
            .AddApiVersioning(options =>
            {
                options.ReportApiVersions = true;
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
            })
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });
    }

    public void ConfigureMiddleware(IApplicationBuilder app, IConfiguration configuration)
    {
        app.UseAuthentication();
        app.UseAuthorization();
    }

    public void ConfigureEndpoints(
        IEndpointRouteBuilder endpoints,
        IApplicationBuilder app,
        IConfiguration configuration)
    {
        endpoints.MapControllers();
    }
}

