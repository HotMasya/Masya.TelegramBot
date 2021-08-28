using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Masya.TelegramBot.Api.Xml;
using Masya.TelegramBot.DataAccess;
using Masya.TelegramBot.DataAccess.Models;
using Masya.TelegramBot.DataAccess.Types;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Masya.TelegramBot.Api.Services
{
    public sealed class XmlService : IXmlService
    {
        public ApplicationDbContext DbContext { get; }
        public List<string> ErrorsList { get; }

        private readonly ILogger<XmlService> _logger;

        private readonly IEnumerable<DirectoryItem> _streets;
        private readonly IEnumerable<DirectoryItem> _districts;
        private readonly IEnumerable<DirectoryItem> _wallMaterials;
        private readonly IEnumerable<DirectoryItem> _states;
        private readonly IEnumerable<DirectoryItem> _types;
        private readonly IEnumerable<DirectoryItem> _misc;
        private readonly IEnumerable<Category> _categories;

        public XmlService(ApplicationDbContext dbContext)
        {
            DbContext = dbContext;
            ErrorsList = new List<string>();

            _categories = DbContext.Categories.ToList();

            var diretoryItems = DbContext.DirectoryItems.Include(i => i.Directory).ToList();
            _streets = diretoryItems.Where(s => s.Directory.Name.Equals(DirectoryType.Street));
            _districts = diretoryItems.Where(s => s.Directory.Name.Equals(DirectoryType.District));
            _wallMaterials = diretoryItems.Where(s => s.Directory.Name.Equals(DirectoryType.WallsMaterial));
            _states = diretoryItems.Where(s => s.Directory.Name.Equals(DirectoryType.State));
            _types = diretoryItems.Where(s => s.Directory.Name.Equals(DirectoryType.Type));
            _misc = diretoryItems.Where(s => s.Directory.Name.Equals(DirectoryType.Misc));
        }

        public async Task<RealtyFeed> GetRealtyFeed(HttpContent content)
        {
            using var stream = await content.ReadAsStreamAsync();
            return XmlHelper.ParseFromXml<RealtyFeed>(stream);
        }

        private int? GetRefId(string value)
        {
            var reference = DbContext.References.FirstOrDefault(r => r.Value == value);
            return reference is not null ? reference.ReferenceId : null;
        }

        private void MapObjects(RealtyObject offerFromDb, Offer offer)
        {
            offerFromDb.Floor = offer.Floor;
            offerFromDb.TotalFloors = offer.FloorsTotal;
            offerFromDb.Description = offer.Description;
            offerFromDb.CreatedAt = offer.CreationDate;
            offerFromDb.EditedAt = offer.LastUpdateDate;
            offerFromDb.KitchenSpace = offer.KitchenSpace.Value;
            offerFromDb.TotalArea = offer.Area.Value;
            offerFromDb.LivingSpace = offer.LivingSpace.Value;
            offerFromDb.LotArea = offer.LotArea.Value;

            if (!string.IsNullOrEmpty(offer.Location.District))
            {
                var districtId = _districts
                    .FirstOrDefault(d => d.Value.Equals(offer.Location.District))?.Id
                    ?? GetRefId(offer.Location.District);

                if (districtId.HasValue)
                {
                    offerFromDb.DistrictId = districtId.Value;
                }
                else
                {
                    ErrorsList.Add("Unable to find district with name: " + offer.Location.District);
                }
            }

            if (!string.IsNullOrEmpty(offer.Location.Address))
            {
                var streetId = _streets
                    .FirstOrDefault(s => s.Value.Equals(offer.Location.Address))?.Id
                    ?? GetRefId(offer.Location.Address);

                if (streetId.HasValue)
                {
                    offerFromDb.StreetId = streetId.Value;
                }
                else
                {
                    ErrorsList.Add("Unable to find address with name: " + offer.Location.Address);
                }
            }

            if (!string.IsNullOrEmpty(offer.Renovation))
            {
                var stateId = _states
                    .FirstOrDefault(s => s.Value.Equals(offer.Renovation))?.Id
                    ?? GetRefId(offer.Renovation);

                if (stateId.HasValue)
                {
                    offerFromDb.StateId = stateId.Value;
                }
                else
                {
                    ErrorsList.Add("Unable to find renovation with name: " + offer.Renovation);
                }
            }

            if (!string.IsNullOrEmpty(offer.BuildingType))
            {
                var wallMaterialId = _wallMaterials
                    .FirstOrDefault(w => w.Value.Equals(offer.BuildingType))?.Id
                    ?? GetRefId(offer.BuildingType);

                if (wallMaterialId.HasValue)
                {
                    offerFromDb.WallMaterialId = wallMaterialId.Value;
                }
                else
                {
                    ErrorsList.Add("Unable to find building type with name: " + offer.BuildingType);
                }
            }

            if (!string.IsNullOrEmpty(offer.Type))
            {
                var typeId = _types
                    .FirstOrDefault(t => t.Value.Equals(offer.Type))?.Id
                    ?? GetRefId(offer.Type);

                if (typeId.HasValue)
                {
                    offerFromDb.WallMaterialId = typeId.Value;
                }
                else
                {
                    ErrorsList.Add("Unable to find type with name: " + offer.Type);
                }
            }

            if (!string.IsNullOrEmpty(offer.Category))
            {
                var categoryId = _types
                    .FirstOrDefault(t => t.Value.Equals(offer.Category))?.Id
                    ?? GetRefId(offer.Category);

                if (categoryId.HasValue)
                {
                    offerFromDb.CategoryId = categoryId.Value;
                }
                else
                {
                    ErrorsList.Add("Unable to find category with name: " + offer.Category);
                }
            }
        }

        public async Task UpdateObjectsAsync(RealtyFeed feed)
        {
            var realtyObjects = await DbContext.RealtyObjects
                .ToListAsync();

            foreach (var offer in feed.Offers)
            {
                var offerFromDb = realtyObjects
                    .FirstOrDefault(
                        o => o.InternalId.HasValue && o.InternalId.Value == offer.InternalId
                    );

                if (offerFromDb is null)
                {
                    var newOffer = new RealtyObject();
                    MapObjects(newOffer, offer);
                    DbContext.RealtyObjects.Add(newOffer);
                    continue;
                }

                MapObjects(offerFromDb, offer);
            }

            await DbContext.SaveChangesAsync();
        }
    }
}