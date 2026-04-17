using AutoMapper;
using JK.Messaging.Contracts;
using JK.Messaging.Database.Entities;
using JK.Messaging.Models;

namespace JK.Messaging.MappingProfiles;

public class MessagingMappingProfile : Profile
{
    public MessagingMappingProfile()
    {
        CreateMap<MessagingEntity, MessagingModel>().ReverseMap();
        CreateMap<ApiMessageTaskEntity, ApiMessageTaskModel>().ReverseMap();
        CreateMap<MessagingModel, MessagingDto>()
            .ForMember(d => d.UpdatedAt, o => o.MapFrom(s => s.UpdatedAt ?? s.CreatedAt));
        CreateMap(typeof(PagedResponse<>), typeof(PagedResponse<>));
    }
}
