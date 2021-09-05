using System;
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
                )
                .ForMember(
                    dest => dest.TelegramAvatar,
                    opt => opt.MapFrom(src => Convert.ToBase64String(src.TelegramAvatar))
                );

            CreateMap<UserDto, User>()
                .ForMember(
                    dest => dest.TelegramAvatar,
                    opt => opt.Ignore()
                );
        }
    }
}