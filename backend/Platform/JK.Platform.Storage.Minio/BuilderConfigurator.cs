using JK.Platform.Core.Abstraction;
using JK.Platform.Storage.Minio.Extensions;
using Microsoft.Extensions.Hosting;

namespace JK.Platform.Storage.Minio;

public class BuilderConfigurator: BuilderConfiguratorBase
{
    public override void ConfigureServices(IHostApplicationBuilder applicationBuilder)
    {
        applicationBuilder.Services.AddMinioStorage(applicationBuilder.Configuration);
    }
}
