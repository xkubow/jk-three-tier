using AutoMapper;
using JK.Order.Contracts;
using JK.Order.Database.Entities;

namespace JK.Order.MappingProfiles;

public class OrderMappingProfile : Profile
{
    public OrderMappingProfile()
    {
        CreateMap<OrderEntity, OrderDto>().ReverseMap();
    }
}

