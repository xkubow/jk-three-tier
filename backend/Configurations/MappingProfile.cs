using AutoMapper;
using Backend.Database.Entities;
using Backend.Models;

namespace Backend.Configurations;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<ConfigurationEntity, ConfigurationModel>();
    }
}
