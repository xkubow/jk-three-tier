using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace JK.Platform.Core.Abstraction;

public interface IBuilderConfigurator
{
    void ConfigureServices(IHostApplicationBuilder applicationBuilder);
    void AddLogging(IHostApplicationBuilder applicationBuilder);
    void AddConfiguration(IHostApplicationBuilder applicationBuilder);
    void AddWebHostConfiguration(IHostApplicationBuilder applicationBuilder);
    IMvcBuilder? AddRestServer(IHostApplicationBuilder applicationBuilder);
}