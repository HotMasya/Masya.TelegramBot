using System;
using AutoMapper;
using Masya.TelegramBot.Api.Dtos;
using Masya.TelegramBot.DataAccess.Models;

namespace Masya.TelegramBot.Api.Profiles
{
    public sealed class AgencyProfile : Profile
    {
        public AgencyProfile()
        {
            CreateMap<Agency, AgencyDto>()
                .ForMember(
                    dest => dest.Agents,
                    opt => opt.MapFrom(src => src.Users)
                );

            CreateMap<AgencyDto, Agency>();

            CreateMap<Agency, Agency>()
                .ForMember(
                    dest => dest.Id,
                    opt => opt.Ignore()
                );

            CreateMap<User, AgentDto>()
                .ForMember(
                    dest => dest.TelegramAvatar,
                    opt => opt.MapFrom(src => Convert.ToBase64String(src.TelegramAvatar))
                );
        }
    }
}