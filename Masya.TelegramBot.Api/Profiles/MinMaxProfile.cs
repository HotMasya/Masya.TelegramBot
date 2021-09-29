using AutoMapper;
using Masya.TelegramBot.Api.Dtos;
using Masya.TelegramBot.DataAccess.Models;

namespace Masya.TelegramBot.Api.Profiles
{
    public sealed class MinMaxProfile : Profile
    {
        public MinMaxProfile()
        {
            CreateMap<Price, PriceDto>().ReverseMap();
            CreateMap<Floor, FloorDto>().ReverseMap();
        }
    }
}