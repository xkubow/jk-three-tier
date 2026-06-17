using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace JK.Platform.Core.Abstraction;

public abstract class BuilderConfiguratorBase: IBuilderConfigurator
{
    public virtual void ConfigureServices(IHostApplicationBuilder applicationBuilder) { }
    public virtual void AddLogging(IHostApplicationBuilder applicationBuilder) { }
    public virtual void AddConfiguration(IHostApplicationBuilder applicationBuilder) { }
    public virtual void AddWebHostConfiguration(IHostApplicationBuilder applicationBuilder) { }
    public virtual IMvcBuilder? AddRestServer(IHostApplicationBuilder applicationBuilder) => null;
}