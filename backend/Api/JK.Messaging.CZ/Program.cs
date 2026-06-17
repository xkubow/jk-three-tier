using System.Net;
using JK.Messaging;
using JK.Messaging.Tasks;
using JK.Platform.Core.AspNetCore.Extensions;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Orleans.Configuration;

AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseOrleans((context, silo) =>
{
    var connectionString = context.Configuration.GetConnectionString("DefaultConnection");
    silo.UseAdoNetClustering(options =>
        {
            options.Invariant = "Npgsql";
            options.ConnectionString = connectionString;
        })
        .AddAdoNetGrainStorage("orleans", options =>
        {
            options.Invariant = "Npgsql";
            options.ConnectionString = connectionString;
            options.GrainStorageSerializer = new GrainStorageJsonSerializer();
        })
        .UseAdoNetReminderService(options =>
        {
            options.Invariant = "Npgsql";
            options.ConnectionString = connectionString;
        })
        .Configure<ClusterOptions>(options =>
        {
            options.ClusterId = "jk-messaging-local";
            options.ServiceId = "Messaging";
        });

    silo.AddStartupTask<ApiMessageRecurringTaskStartupTask>();

    silo.ConfigureEndpoints(IPAddress.Loopback, siloPort: 11111, gatewayPort: 30000);

});

builder.BuildPlatform();

var app = builder.Build();

app.UsePlatform();

app.Run();
