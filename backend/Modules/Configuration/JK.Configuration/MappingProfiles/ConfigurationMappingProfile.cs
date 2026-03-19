using AutoMapper;
using JK.Configuration.Contracts;
using JK.Configuration.Database.Entities;
using JK.Configuration.Models;
using JK.Configuration.Proto;

namespace JK.Configuration.MappingProfiles;

public class ConfigurationMappingProfile : Profile
{
    public ConfigurationMappingProfile()
    {
        CreateMap<ConfigurationEntity, ConfigurationDto>();
        CreateMap<GrpcConfigurationRequest, ConfigurationRequest>()
            .ForMember(dest => dest.Market, opt => opt.MapFrom(src => src.MarketCode))
            .ForMember(dest => dest.MarketArea, opt => opt.MapFrom(src => src.HasMarketArea ? src.MarketArea : null))
            .ForMember(dest => dest.Service, opt => opt.MapFrom(src => src.ServiceCode));
        CreateMap<ConfigurationDto, GrpcConfiguration>();
    }
}
