using JK.Platform.Core.Abstraction;
using JK.Platform.Core.AspNetCore.Discovery;
using JK.Platform.Core.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace JK.Platform.Core.AspNetCore.Extensions;

public static class WebApplicationBuilderExtension
{

    public static void BuildPlatform(this IHostApplicationBuilder applicationBuilder)
    {
        if(applicationBuilder is not WebApplicationBuilder webApplicationBuilder)
            throw new InvalidOperationException("WebApplicationBuilder is required");

        // Register all injectable services from all domain assemblies
        var domainAssemblies = DomainDiscovery.FindDomainAssemblies();
        foreach (var assembly in domainAssemblies)
        {
            applicationBuilder.Services.RegisterInjectableServices(assembly);
        }

        var builderConfigurations = FindPlatformConfigurations<IBuilderConfigurator>();
        IMvcBuilder? mvcBuilder = null;
        foreach (var builderConfiguration in builderConfigurations)
        {
            builderConfiguration.AddConfiguration(applicationBuilder);
            builderConfiguration.AddLogging(applicationBuilder);
            builderConfiguration.ConfigureServices(applicationBuilder);
            builderConfiguration.AddWebHostConfiguration(applicationBuilder);
            mvcBuilder ??= builderConfiguration.AddRestServer(applicationBuilder);
        }

        var moduleInstallerTypes = DomainDiscovery.FindModuleInstallerTypes();
        var installers = DomainDiscovery.CreateModuleInstallers(moduleInstallerTypes);

        foreach (var installer in installers)
        {
            installer.RegisterServices(applicationBuilder.Services, applicationBuilder.Configuration, webApplicationBuilder.Environment);
            if(mvcBuilder != null)
                installer.RegisterControllers(mvcBuilder);
        }

    }

    public static void UsePlatform(this IApplicationBuilder applicationBuilder)
    {
        var platformConfigurations = FindPlatformConfigurations<IApplicationConfigurator>();
        foreach (var configuration in platformConfigurations)
            configuration.ConfigureCorrelations(applicationBuilder);
        foreach (var configuration in platformConfigurations)
            configuration.ConfigureSerilog(applicationBuilder);

        applicationBuilder.UseRouting();

        foreach (var configuration in platformConfigurations)
            configuration.ConfigureCors(applicationBuilder);

        applicationBuilder.UseAuthentication();
        applicationBuilder.UseAuthorization();

        foreach (var configuration in platformConfigurations)
            configuration.ConfigureSwagger(applicationBuilder);

        if(applicationBuilder is IEndpointRouteBuilder endpointRouteBuilder)
            endpointRouteBuilder.MapControllers();

        var moduleInstallerTypes = DomainDiscovery.FindModuleInstallerTypes();
        var installers = DomainDiscovery.CreateModuleInstallers(moduleInstallerTypes);

        if(applicationBuilder is not WebApplication webApplication)
            throw new InvalidOperationException("webApplication is required");

        foreach (var installer in installers)
        {
            installer.MapGrpcServices(webApplication);
            installer.MapHealthChecks(webApplication);
        }
    }

    private static List<TConfigurator> FindPlatformConfigurations<TConfigurator>()
    {
        var jkAssemblies = DomainDiscovery.FindDomainAssemblies();
        List<TConfigurator> builderConfigurations = new();
        foreach (var jkAssembly in jkAssemblies)
        {
            try
            {
                builderConfigurations.AddRange(
                    jkAssembly.GetTypes()
                        .Where(p => typeof(TConfigurator).IsAssignableFrom(p) && !p.IsAbstract && p.IsClass)
                        .Select(p => (TConfigurator)Activator.CreateInstance(p)!).ToList()
                );
            }
            catch
            {
                // Skip assemblies that can't be fully loaded
            }
        }

        return builderConfigurations;

    }
}