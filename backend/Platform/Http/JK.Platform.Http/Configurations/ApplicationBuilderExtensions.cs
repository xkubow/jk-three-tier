using JK.Platform.Http.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace JK.Platform.Http.Configurations;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UsePlatformCors(this IApplicationBuilder app)
    {
        var corsConfiguration = app.ApplicationServices.GetRequiredService<CorsConfiguration>();
        return app.UseCors(corsConfiguration.PolicyName);
    }
}