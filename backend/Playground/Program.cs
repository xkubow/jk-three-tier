using Backend.Configurations;
using Backend.Database;
using Backend.Database.Repositories;
using Backend.Services;
using Backend.Stores;
using JK.Platform.Rest.Server.Configurations;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var allowFrontendCorsPolicy = "AllowFrontend";

builder.Services.AddCors(options =>
{
    options.AddPolicy(allowFrontendCorsPolicy, policy =>
    {
        policy
            .WithOrigins("http://localhost:8080", "http://localhost:8095")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Add services to the container (REST platform)
// builder.Services.AddJkRestApi("api");
builder.Services.AddSwaggerGen();

// Typed Configuration
builder.Services.Configure<ConfigurationSettings>(builder.Configuration.GetSection("AppSettings"));

// AutoMapper Configuration
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Database Configuration (PostgreSQL)
builder.Services.AddDbContext<ConfigDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Dependency Injection
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IConfigurationRepository, ConfigurationRepository>();
builder.Services.AddScoped<IConfigurationStore, ConfigurationStore>();
builder.Services.AddScoped<IConfigurationService, ConfigurationService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseWhen(
    ctx => ctx.Request.Host.Host == "localhost" 
        || ctx.Request.Host.Host == "172.27.76.33",
    branch =>
    {
        branch.UseSwagger(c =>
        {
            c.RouteTemplate = "api/swagger/{documentName}/swagger.json";
        });

        branch.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("v1/swagger.json", "jk-three-tier API v1");
            c.RoutePrefix = "api/swagger";
        });
    });
app.UseCors(allowFrontendCorsPolicy);

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
