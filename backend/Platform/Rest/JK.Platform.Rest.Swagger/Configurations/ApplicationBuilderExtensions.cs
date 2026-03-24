using Asp.Versioning.ApiExplorer;
using JK.Platform.Rest.Swagger.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace JK.Platform.Rest.Swagger.Configurations;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UsePlatformSwagger(this IApplicationBuilder app)
    {
        var swaggerConfiguration = app.ApplicationServices.GetRequiredService<SwaggerConfiguration>();

        if (!swaggerConfiguration.Enabled)
        {
            return app;
        }

        var apiVersionDescriptionProvider =
            app.ApplicationServices.GetRequiredService<IApiVersionDescriptionProvider>();

        app.UseSwagger();

        app.UseSwaggerUI(options =>
        {
            options.RoutePrefix = swaggerConfiguration.RoutePrefix;

            foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions)
            {
                options.SwaggerEndpoint(
                    $"/swagger/{description.GroupName}/swagger.json",
                    $"{swaggerConfiguration.Title} {description.GroupName.ToUpperInvariant()}");
            }
        });

        return app;
    }
}