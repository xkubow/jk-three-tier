using Backend.Configurations;
using Backend.Database;
using Backend.Database.Repositories;
using Backend.Services;
using Backend.Stores;
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

// Add services to the container.
builder.Services.AddControllers(options =>
{
    options.Conventions.Add(new ApiPrefixConvention("api"));
});
builder.Services.AddSwaggerGen();

// Typed Configuration
builder.Services.Configure<ConfigurationSettings>(builder.Configuration.GetSection("AppSettings"));

// AutoMapper Configuration
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Database Configuration
builder.Services.AddDbContext<ConfigDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Dependency Injection
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IConfigurationRepository, ConfigurationRepository>();
builder.Services.AddScoped<IConfigurationStore, ConfigurationStore>();
builder.Services.AddScoped<IConfigurationService, ConfigurationService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors(allowFrontendCorsPolicy);

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
