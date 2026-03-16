using AutoMapper;
using JK.Configuration.Contracts;
using JK.Configuration.Database.Entities;

namespace JK.Configuration.MappingProfiles;

public class ConfigurationMappingProfile : Profile
{
    public ConfigurationMappingProfile()
    {
        CreateMap<ConfigurationEntity, ConfigurationDto>();
    }
}
