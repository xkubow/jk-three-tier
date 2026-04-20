using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace JK.Platform.Rest.Swagger.Filters;

public class IdempotencyHeaderOperationFilter: IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var method = context.ApiDescription.HttpMethod;

        if (method is not ("POST" or "PUT" or "PATCH"))
            return;

        operation.Parameters ??= new List<OpenApiParameter>();

        if (operation.Parameters.Any(p => p.Name == "Idempotency-Key"))
            return;

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "Idempotency-Key",
            In = ParameterLocation.Header,
            Required = false,
            Description = "Unique key for idempotent requests",
            Schema = new OpenApiSchema
            {
                Type = "string"
            }
        });
    }
}