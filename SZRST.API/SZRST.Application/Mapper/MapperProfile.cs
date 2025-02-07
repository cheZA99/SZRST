using AutoMapper;
using Domain.Entities;
using Application.Requests.User;
using SZRST.Shared.response;
namespace Application.Mapper
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<Facility, FacilityResponse>();
           
        }
    }
}
