using AutoMapper;
using Masya.TelegramBot.Api.Dtos;
using Masya.TelegramBot.DataAccess.Models;

namespace Masya.TelegramBot.Api.Profiles
{
    public sealed class MinMaxProfile : Profile
    {
        public MinMaxProfile()
        {
            CreateMap<Price, PriceDto>();
            CreateMap<PriceDto, Price>()
                .ForMember(
                    dest => dest.Id,
                    opt => opt.Ignore()
                );

            CreateMap<Floor, FloorDto>();
            CreateMap<FloorDto, Floor>()
                .ForMember(
                    dest => dest.Id,
                    opt => opt.Ignore()
                );
        }
    }
}