using AutoMapper;
using JK.Configuration.Contracts;
using JK.Configuration.Proto;

namespace JK.Configuration.Provider.MappingProfiles;

public class ConfigurationProviderMappingProfile: Profile
{
    public ConfigurationProviderMappingProfile()
    {
        CreateMap<GrpcConfiguration, ConfigurationDto>();
    }
}