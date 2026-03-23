using FluentValidation;
using JK.Order.Configurations;
using JK.Order.Database;
using JK.Order.Grpc;
using JK.Platform.Core.Abstraction;
using JK.Platform.Core.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JK.Order;

public class OrderModuleInstaller : IModuleInstaller
{
    public string ModuleName => "Order";

    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        var assembly = typeof(OrderAssemblyMarker).Assembly;

        services.AddDbContext<OrderDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            options.UseNpgsql(connectionString);
        });


        services.AddAutoMapper(assembly);
        services.AddValidatorsFromAssembly(assembly);

        services.RegisterInjectableServices(assembly);
    }

    public void RegisterControllers(IMvcBuilder mvcBuilder)
    {
        mvcBuilder.AddApplicationPart(typeof(OrderAssemblyMarker).Assembly);
    }

    public void MapGrpcServices(WebApplication app)
    {
        app.MapGrpcService<OrderGrpcService>();
    }
}


