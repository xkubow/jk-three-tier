using AutoMapper;
using JK.Messaging.Database.Entities;
using JK.Messaging.Models;

namespace JK.Messaging.MappingProfiles;

public class MessagingMappingProfile : Profile
{
    public MessagingMappingProfile()
    {
        CreateMap<ApiMessageTaskEntity, ApiMessageTaskModel>().ReverseMap();
    }
}
