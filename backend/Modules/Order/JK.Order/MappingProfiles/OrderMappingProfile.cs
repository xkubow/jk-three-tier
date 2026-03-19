using AutoMapper;
using JK.Order.Contracts;
using JK.Order.Database.Entities;
using JK.Order.Models;
using JK.Platform.Persistence.EfCore;

namespace JK.Order.MappingProfiles;

public class OrderMappingProfile : Profile
{
    public OrderMappingProfile()
    {
        CreateMap<OrderEntity, OrderModel>().ReverseMap();
        CreateMap<OrderModel, OrderDto>();
        CreateMap(typeof(PagedResponse<>), typeof(PagedResponse<>));
    }
}

