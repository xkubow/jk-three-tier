using Microsoft.AspNetCore.Builder;

namespace JK.Platform.Core.Abstraction;

public interface IApplicationConfigurator
{
    public void ConfigureCorrelations(IApplicationBuilder app);
    void ConfigureCors(IApplicationBuilder app);
    void ConfigureSerilog(IApplicationBuilder app);
    void ConfigureSwagger(IApplicationBuilder app);
}