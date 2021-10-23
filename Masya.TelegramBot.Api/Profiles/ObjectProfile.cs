using AutoMapper;
using Masya.TelegramBot.Api.Dtos;
using Masya.TelegramBot.DataAccess.Models;

namespace Masya.TelegramBot.Api.Profiles
{
    public sealed class ObjectProfile : Profile
    {
        public ObjectProfile()
        {
            CreateMap<RealtyObject, RealtyObjectDto>().ReverseMap();
        }
    }
}