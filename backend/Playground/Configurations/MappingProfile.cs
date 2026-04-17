using AutoMapper;
using Backend.Models;
using JK.Playground.Database.Entities;

namespace Backend.Configurations;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<ConfigurationEntity, ConfigurationModel>();
    }
}
