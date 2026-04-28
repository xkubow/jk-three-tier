using JK.Platform.Database.Migrations;
using JK.Configuration.Provider;
using JK.Platform.Core.AspNetCore.Discovery;
using JK.Platform.Grpc.Server.Extensions;
using JK.Platform.Http.Configurations;
using JK.Platform.Http.Extensions;
using JK.Platform.Rest.Server.Configurations;
using JK.Platform.Rest.Swagger.Configurations;
using Microsoft.AspNetCore.Server.Kestrel.Core;

AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

var builder = WebApplication.CreateBuilder(args);
if (builder.Environment.IsEnvironment("K8s"))
{
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.ListenAnyIP(8080, listenOptions => { listenOptions.Protocols = HttpProtocols.Http1; });

        options.ListenAnyIP(8081, listenOptions => { listenOptions.Protocols = HttpProtocols.Http2; });
    });
}

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.AddConfigurationServerProvider();

var mvcBuilder = builder.Services.AddPlatformRestServer(builder.Configuration);

builder.Services.AddPlatformCors(builder.Configuration);
builder.Services.AddPlatformSwagger(builder.Configuration);
builder.Services.AddGrpcPlatform();
builder.Services.AddHealthChecks();

var moduleInstallerTypes = DomainDiscovery.FindModuleInstallerTypes();
var installers = DomainDiscovery.CreateModuleInstallers(moduleInstallerTypes);

foreach (var installer in installers)
{
    installer.RegisterServices(builder.Services, builder.Configuration);
    installer.RegisterControllers(mvcBuilder);
}

var app = builder.Build();

if (app.Configuration.GetValue<bool>("Database:RunMigrationsOnStartup"))
    app.Services.RunBackendMigrations();

app.UseRouting();
app.UsePlatformCors();

app.UseAuthentication();
app.UseAuthorization();

app.UsePlatformSwagger();

app.MapControllers();

foreach (var installer in installers)
{
    installer.MapGrpcServices(app);
    installer.MapHealthChecks(app);
}

app.Run();
