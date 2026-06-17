using JK.Platform.Database.Migrations;
using JK.Platform.Core.AspNetCore.Extensions;
using JK.Platform.Http.Extensions;

AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

var builder = WebApplication.CreateBuilder(args);

builder.BuildPlatform();

var app = builder.Build();

if (app.Configuration.GetValue<bool>("Database:RunMigrationsOnStartup"))
    app.Services.RunBackendMigrations();

app.UsePlatform();
app.UseIdempotency();

app.Run();
