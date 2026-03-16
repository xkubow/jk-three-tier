using Asp.Versioning;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JK.Platform.Rest.Server.Configurations;

public static class ServiceCollectionExtensions
{
    public static IMvcBuilder AddPlatformRestServer(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var mvcBuilder = services
            .AddControllers(options =>
            {
                options.ConfigurePlatformRestMvc();
            })
            .AddJsonOptions(options =>
            {
                options.ConfigurePlatformRestJson();
            });

        services
            .AddApiVersioning(options =>
            {
                options.ReportApiVersions = true;
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
                options.DefaultApiVersion = new Asp.Versioning.ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
            })
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

        return mvcBuilder;
    }
}