using JK.Platform.Rest.Swagger.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JK.Platform.Rest.Swagger.Configurations;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPlatformSwagger(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var swaggerConfiguration = new SwaggerConfiguration();

        configuration
            .GetSection(SwaggerConfiguration.SectionName)
            .Bind(swaggerConfiguration);

        services.AddSingleton(swaggerConfiguration);

        services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();

        services.AddSwaggerGen(options =>
        {
            if (swaggerConfiguration.EnableAnnotations)
            {
                options.EnableAnnotations();
            }
        });

        return services;
    }
}