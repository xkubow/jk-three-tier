using Microsoft.AspNetCore.Builder;

namespace JK.Platform.Grpc.Server.Extensions;

public static class GrpcPlatformApplicationExtensions
{
    public static WebApplication UseGrpcSwaggerUi(this WebApplication app, string version = "v1")
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint($"/swagger/{version}/swagger.json", version);
        });

        return app;
    }
}