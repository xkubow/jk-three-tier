using JK.Platform.Http.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JK.Platform.Http.Configurations;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPlatformCors(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var corsConfiguration = new CorsConfiguration();
        configuration.GetSection(CorsConfiguration.SectionName).Bind(corsConfiguration);

        services.AddSingleton(corsConfiguration);

        services.AddCors(options =>
        {
            options.AddPolicy(corsConfiguration.PolicyName, policy =>
            {
                if (corsConfiguration.AllowedOrigins.Length == 0)
                {
                    policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                    return;
                }

                policy.WithOrigins(corsConfiguration.AllowedOrigins);

                if (corsConfiguration.AllowedMethods.Length == 1 && corsConfiguration.AllowedMethods[0] == "*")
                    policy.AllowAnyMethod();
                else
                    policy.WithMethods(corsConfiguration.AllowedMethods);

                if (corsConfiguration.AllowedHeaders.Length == 1 && corsConfiguration.AllowedHeaders[0] == "*")
                    policy.AllowAnyHeader();
                else
                    policy.WithHeaders(corsConfiguration.AllowedHeaders);

                if (corsConfiguration.AllowCredentials)
                    policy.AllowCredentials();
            });
        });

        return services;
    }
}