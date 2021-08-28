using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Masya.TelegramBot.Api.Options;
using Masya.TelegramBot.Api.Xml;
using Masya.TelegramBot.DataAccess;
using Masya.TelegramBot.DataAccess.Types;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Masya.TelegramBot.Api.Services
{
    public sealed class XmlService : IXmlService
    {
        public IServiceProvider Services { get; }
        public XmlOptions Options { get; }

        private readonly ILogger<XmlService> _logger;

        public XmlService(IServiceProvider services, IOptions<XmlOptions> options)
        {
            Services = services;
            Options = options.Value;
        }

        private async Task<RealtyFeed> GetRealtyFeed(HttpContent content)
        {
            using var stream = await content.ReadAsStreamAsync();
            return XmlHelper.ParseFromXml<RealtyFeed>(stream);
        }

        public static async Task UpdateObjectsAsync(ApplicationDbContext context, RealtyFeed feed)
        {
            var realtyObjects = await context.RealtyObjects
                .ToListAsync();

            var diretoryItems = await context.DirectoryItems
                .Include(i => i.Directory)
                .ToListAsync();

            var streets = diretoryItems.Where(s => s.Directory.Name.Equals(DirectoryType.Street));
            var districts = diretoryItems.Where(s => s.Directory.Name.Equals(DirectoryType.District));
            var wallMaterials = diretoryItems.Where(s => s.Directory.Name.Equals(DirectoryType.WallsMaterial));
            var states = diretoryItems.Where(s => s.Directory.Name.Equals(DirectoryType.State));
            var types = diretoryItems.Where(s => s.Directory.Name.Equals(DirectoryType.Type));
            var misc = diretoryItems.Where(s => s.Directory.Name.Equals(DirectoryType.Misc));

            foreach (var offer in feed.Offers)
            {
                var offerFromDb = realtyObjects
                    .FirstOrDefault(
                        o => o.InternalId.HasValue && o.InternalId.Value == offer.InternalId
                    );

                if (offerFromDb is null)
                {
                    continue;
                }

                offerFromDb.Floor = offer.Floor;
                offerFromDb.TotalFloors = offer.FloorsTotal;
                offerFromDb.Description = offer.Description;
                offerFromDb.CreatedAt = offer.CreationDate;
                offerFromDb.EditedAt = offer.LastUpdateDate;
                offerFromDb.KitchenSpace = offer.KitchenSpace.Value;
                offerFromDb.TotalArea = offer.Area.Value;
                offerFromDb.LivingSpace = offer.LivingSpace.Value;
                offerFromDb.LotArea = offer.LotArea.Value;

            }
        }

        public async Task StartWatching(CancellationToken token = default)
        {
            while (!token.IsCancellationRequested)
            {
                _logger.LogInformation("Starting to update objects the database.");
                using (var scope = Services.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var xmlUrls = dbContext.Agencies.Select(a => a.ImportUrl).ToList();
                    var httpClient = new HttpClient();
                    foreach (var url in xmlUrls)
                    {
                        if (!string.IsNullOrEmpty(url))
                        {
                            var response = await httpClient.GetAsync(url);
                            if (response.StatusCode == HttpStatusCode.OK)
                            {
                                var realtyFeed = await GetRealtyFeed(response.Content);

                            }

                            _logger.LogInformation("{0} - Finished with status code: {1}", url, ((int)response.StatusCode));
                        }
                    }
                }
                _logger.LogInformation("Finished updating objects in the database.");
                await Task.Delay(TimeSpan.FromHours(Options.UpdateTimeInHours), token);
            }
        }
    }
}