using Microsoft.AspNetCore.Builder;

namespace JK.Platform.Core.Abstraction;

public interface IMiddlewareConfigurator
{
    void Configure(IApplicationBuilder app);
}