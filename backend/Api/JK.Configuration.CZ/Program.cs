using JK.Platform.Database.Migrations;
using JK.Platform.Core.AspNetCore.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.BuildPlatform();

var app = builder.Build();

if (app.Configuration.GetValue<bool>("Database:RunMigrationsOnStartup"))
    app.Services.RunBackendMigrations();

app.UsePlatform();

app.Run();
