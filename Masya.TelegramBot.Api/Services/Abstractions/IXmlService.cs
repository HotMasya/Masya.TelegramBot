using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Masya.TelegramBot.Api.Xml;
using Masya.TelegramBot.DataAccess;

namespace Masya.TelegramBot.Api.Services.Abstractions
{
    public interface IXmlService
    {
        ApplicationDbContext DbContext { get; }
        Task<RealtyFeed> GetRealtyFeed(HttpContent content);
        Task UpdateObjectsAsync(RealtyFeed feed, int agencyId);
    }
}