using AutoMapper;
using Domain.Entities;
using SZRST.Shared.response;

namespace Application.Mapper
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<FacilityType, FacilityTypeSummary>();
            CreateMap<Country, CountrySummary>();
            CreateMap<City, CitySummary>();
            CreateMap<Location, LocationSummary>()
                .ForMember(dest => dest.Country, opt => opt.MapFrom(src => src.Country));
            CreateMap<Facility, FacilityResponse>();
        }
    }
}
