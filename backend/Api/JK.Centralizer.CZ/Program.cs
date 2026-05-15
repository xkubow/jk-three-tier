using JK.Configuration.Provider;
using JK.Platform.Core.AspNetCore.Discovery;
using JK.Platform.Core.Observability;
using JK.Platform.Http.Configurations;
using JK.Platform.Rest.Server.Configurations;
using JK.Platform.Rest.Swagger.Configurations;

AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.AddConfigurationServerProvider();

var mvcBuilder = builder.Services.AddPlatformRestServer(builder.Configuration);

builder.Services.AddPlatformCorrelation();
builder.Services.AddPlatformCors(builder.Configuration);
builder.Services.AddPlatformSwagger(builder.Configuration);
builder.Services.AddHealthChecks();
builder.Services.AddPlatformOpenTelemetry(builder.Configuration, builder.Environment);

var moduleInstallerTypes = DomainDiscovery.FindModuleInstallerTypes();
var installers = DomainDiscovery.CreateModuleInstallers(moduleInstallerTypes);

foreach (var installer in installers)
{
    installer.RegisterServices(builder.Services, builder.Configuration, builder.Environment);
    installer.RegisterControllers(mvcBuilder);
}

var app = builder.Build();

app.UsePlatformCorrelation();
app.UseRouting();
app.UsePlatformCors();

app.UseAuthentication();
app.UseAuthorization();

app.UsePlatformSwagger();

app.MapControllers();

foreach (var installer in installers)
{
    // Centralizer probably won't have Grpc services, but we call the installers for others if any
    installer.MapGrpcServices(app);
    installer.MapHealthChecks(app);
}

app.Run();
