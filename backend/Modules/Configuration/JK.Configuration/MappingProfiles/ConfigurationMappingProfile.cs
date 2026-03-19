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
        CreateMap<ConfigurationEntity, ConfigurationModel>().ReverseMap();
        CreateMap<ConfigurationModel, ConfigurationDto>();
        CreateMap(typeof(PagedResponse<>), typeof(PagedResponse<>));
        CreateMap<GrpcConfigurationRequest, ConfigurationRequest>()
            .ForMember(dest => dest.MarketArea, opt => opt.MapFrom(src => src.HasMarketArea ? src.MarketArea : null));
        CreateMap<ConfigurationDto, GrpcConfiguration>();
    }
}
