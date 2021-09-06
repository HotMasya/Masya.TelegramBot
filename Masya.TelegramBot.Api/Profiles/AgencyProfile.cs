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
                .ReverseMap();

            CreateMap<Agency, Agency>()
                .ForMember(
                    dest => dest.Id,
                    opt => opt.Ignore()
                );
        }
    }
}