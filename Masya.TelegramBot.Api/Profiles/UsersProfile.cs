using AutoMapper;
using Masya.TelegramBot.Api.Dtos;
using Masya.TelegramBot.DataAccess.Models;

namespace Masya.TelegramBot.Api.Profiles
{
    public sealed class UsersProfile : Profile
    {
        public UsersProfile()
        {
            CreateMap<User, UserDto>()
                .ForMember(
                    dest => dest.AgencyName,
                    opt => opt.MapFrom(src => src.Agency.Name)
                );

            CreateMap<UserDto, User>();
        }
    }
}