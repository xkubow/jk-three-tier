using Asp.Versioning.ApiExplorer;
using JK.Platform.Rest.Swagger.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JK.Platform.Rest.Swagger.Configurations;

public sealed class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider _apiVersionDescriptionProvider;
    private readonly SwaggerConfiguration _swaggerConfiguration;

    public ConfigureSwaggerOptions(
        IApiVersionDescriptionProvider apiVersionDescriptionProvider,
        SwaggerConfiguration swaggerConfiguration)
    {
        _apiVersionDescriptionProvider = apiVersionDescriptionProvider;
        _swaggerConfiguration = swaggerConfiguration;
    }

    public void Configure(SwaggerGenOptions options)
    {
        options.CustomSchemaIds(type => (type.FullName ?? type.Name).Replace("+", "."));

        foreach (var description in _apiVersionDescriptionProvider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(description.GroupName, CreateOpenApiInfo(description));
        }

        if (_swaggerConfiguration.AddJwtBearerSecurity)
        {
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter JWT Bearer token."
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        }
    }

    private OpenApiInfo CreateOpenApiInfo(ApiVersionDescription description)
    {
        var info = new OpenApiInfo
        {
            Title = _swaggerConfiguration.Title,
            Description = _swaggerConfiguration.Description,
            Version = description.ApiVersion.ToString()
        };

        if (!string.IsNullOrWhiteSpace(_swaggerConfiguration.ContactName))
        {
            info.Contact = new OpenApiContact
            {
                Name = _swaggerConfiguration.ContactName,
                Email = _swaggerConfiguration.ContactEmail
            };
        }

        return info;
    }
}