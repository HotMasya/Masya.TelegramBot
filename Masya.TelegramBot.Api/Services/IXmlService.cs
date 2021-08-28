using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Masya.TelegramBot.Api.Xml;
using Masya.TelegramBot.DataAccess;

namespace Masya.TelegramBot.Api.Services
{
    public interface IXmlService
    {
        ApplicationDbContext DbContext { get; }
        List<string> ErrorsList { get; }
        Task<RealtyFeed> GetRealtyFeed(HttpContent content);
        Task UpdateObjectsAsync(RealtyFeed feed);
    }
}