using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JK.Platform.Core.Abstraction;

public interface IModuleInstaller
{
    string ModuleName { get; }
    void RegisterServices(IServiceCollection services, IConfiguration configuration);
    void RegisterControllers(IMvcBuilder mvcBuilder);
    void MapGrpcServices(WebApplication app);
    void MapHealthChecks(WebApplication app);
}