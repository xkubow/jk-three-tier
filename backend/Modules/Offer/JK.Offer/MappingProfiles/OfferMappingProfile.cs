using AutoMapper;
using JK.Offer.Contracts;
using JK.Offer.Database.Entities;
using JK.Offer.Models;

namespace JK.Offer.MappingProfiles;

public class OfferMappingProfile : Profile
{
    public OfferMappingProfile()
    {
        CreateMap<OfferEntity, OfferModel>()
            .ForMember(dest => dest.OfferNumber, opt => opt.MapFrom(src => src.Number))
            .ReverseMap()
            .ForMember(dest => dest.Number, opt => opt.MapFrom(src => src.OfferNumber));

        CreateMap<OfferModel, OfferDto>()
            .ForMember(dest => dest.Number, opt => opt.MapFrom(src => src.OfferNumber))
            .ReverseMap()
            .ForMember(dest => dest.OfferNumber, opt => opt.MapFrom(src => src.Number));
        CreateMap<PagedResponse<OfferModel>, PagedResponse<OfferDto>>();
    }
}
