using JK.Platform.Core.AspNetCore.Extensions;

AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

var builder = WebApplication.CreateBuilder(args);

builder.BuildPlatform();

var app = builder.Build();

app.UsePlatform();

app.Run();
