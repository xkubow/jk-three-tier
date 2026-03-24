using AutoMapper;
using Microsoft.Extensions.Configuration;

namespace JK.Platform.Core.Abstraction;

public interface IAutoMapperStartupConfigurator
{
    void ConfigureAutomapperGlobalMappings(
        IMapperConfigurationExpression mapperConfiguration,
        IConfiguration configuration);
}

