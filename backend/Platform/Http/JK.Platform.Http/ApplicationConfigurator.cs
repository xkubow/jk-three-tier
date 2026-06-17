using JK.Platform.Core.Abstraction;
using JK.Platform.Http.Configurations;
using Microsoft.AspNetCore.Builder;

namespace JK.Platform.Http;

public class ApplicationConfigurator: ApplicationConfiguratorBase
{
    public override void ConfigureCorrelations(IApplicationBuilder app) => app.UsePlatformCorrelation();

    public override void ConfigureCors(IApplicationBuilder app) => app.UsePlatformCors();
}