using Api.DTO;
using AutoMapper;
using Model.Users;

namespace Api.AutoMapperProfiles
{
    public class UserMapping : Profile
    {
        public UserMapping()
        {
            CreateMap<UserDto, User>()
                .ForMember(o => o.PhoneNumber, opt => opt.MapFrom(src => src.Phone))
                .ForMember(o => o.UserName, opt => opt.MapFrom(src => src.Username))
                .ReverseMap();
        }
    }
}