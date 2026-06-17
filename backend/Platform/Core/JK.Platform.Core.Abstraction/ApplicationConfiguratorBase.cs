using Microsoft.AspNetCore.Builder;

namespace JK.Platform.Core.Abstraction;

public abstract class ApplicationConfiguratorBase: IApplicationConfigurator
{
    public virtual void ConfigureCorrelations(IApplicationBuilder app) { }
    public virtual void ConfigureCors(IApplicationBuilder app) { }
    public virtual void ConfigureSerilog(IApplicationBuilder app) { }
    public virtual void ConfigureSwagger(IApplicationBuilder app) { }
}