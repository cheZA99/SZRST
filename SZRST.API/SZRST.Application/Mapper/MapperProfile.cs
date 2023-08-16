using AutoMapper;
using Domain.Entities;
using Application.Requests.User;
namespace Application.Mapper
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            //CreateMap<User, User>().ReverseMap();
            //CreateMap<User, UserInsertRequest>().ReverseMap();
            //CreateMap<User, UserUpdateRequest>().ReverseMap();
            //CreateMap<UserInsertRequest, Models.User>().ReverseMap();
            //CreateMap<UserUpdateRequest, Models.User>().ReverseMap();
        }
    }
}
