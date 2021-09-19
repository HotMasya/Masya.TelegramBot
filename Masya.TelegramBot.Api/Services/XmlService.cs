using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Masya.TelegramBot.Api.Services.Abstractions;
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

        private readonly ILogger<IXmlService> _logger;

        private readonly IEnumerable<DirectoryItem> _streets;
        private readonly IEnumerable<DirectoryItem> _districts;
        private readonly IEnumerable<DirectoryItem> _wallMaterials;
        private readonly IEnumerable<DirectoryItem> _states;
        private readonly IEnumerable<DirectoryItem> _types;
        private readonly IEnumerable<DirectoryItem> _misc;
        private readonly IEnumerable<Category> _categories;

        public XmlService(ApplicationDbContext dbContext, ILogger<IXmlService> logger)
        {
            DbContext = dbContext;
            _logger = logger;

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
            using (var stream = await content.ReadAsStreamAsync())
            {
                return XmlHelper.ParseFromXml<RealtyFeed>(stream);
            }
        }

        private int? GetRefId(string value)
        {
            var reference = DbContext.References.FirstOrDefault(r => r.Value.ToLower() == value.ToLower());
            return reference?.ReferenceId;
        }

        private void MapObjects(RealtyObject offerFromDb, Offer offer, int agencyId)
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
                    .FirstOrDefault(d => d.Value.ToLower().Equals(offer.Location.District.ToLower()))?.Id
                    ?? GetRefId(offer.Location.District);

                if (districtId.HasValue)
                {
                    offerFromDb.DistrictId = districtId.Value;
                }
                else
                {
                    _logger.LogError(
                        "Unable to resove district \"{district}\" in object with internal id {internalId}. {AgencyId}",
                        offer.Location.District,
                        offer.InternalId,
                        agencyId
                    );
                }
            }

            if (!string.IsNullOrEmpty(offer.Location.Address))
            {
                var streetId = _streets
                    .FirstOrDefault(s => s.Value.ToLower().Equals(offer.Location.Address.ToLower()))?.Id
                    ?? GetRefId(offer.Location.Address);

                if (streetId.HasValue)
                {
                    offerFromDb.StreetId = streetId.Value;
                }
                else
                {
                    _logger.LogError(
                        "Unable to resove address \"{address}\" in object with internal id {internalId}. {AgencyId}",
                        offer.Location.Address,
                        offer.InternalId,
                        agencyId
                    );
                }
            }

            if (!string.IsNullOrEmpty(offer.Renovation))
            {
                var stateId = _states
                    .FirstOrDefault(s => s.Value.ToLower().Equals(offer.Renovation.ToLower()))?.Id
                    ?? GetRefId(offer.Renovation);

                if (stateId.HasValue)
                {
                    offerFromDb.StateId = stateId.Value;
                }
                else
                {
                    _logger.LogError(
                        "Unable to resove renovation \"{renovation}\" in object with internal id {internalId}. {AgencyId}",
                        offer.Renovation,
                        offer.InternalId,
                        agencyId
                    );
                }
            }

            if (!string.IsNullOrEmpty(offer.BuildingType))
            {
                var wallMaterialId = _wallMaterials
                    .FirstOrDefault(w => w.Value.ToLower().Equals(offer.BuildingType.ToLower()))?.Id
                    ?? GetRefId(offer.BuildingType);

                if (wallMaterialId.HasValue)
                {
                    offerFromDb.WallMaterialId = wallMaterialId.Value;
                }
                else
                {
                    _logger.LogError(
                        "Unable to resove building type \"{buildingType}\" in object with internal id {internalId}. {AgencyId}",
                        offer.BuildingType,
                        offer.InternalId,
                        agencyId
                    );
                }
            }

            if (!string.IsNullOrEmpty(offer.Type))
            {
                var typeId = _types
                    .FirstOrDefault(t => t.Value.ToLower().Equals(offer.Type.ToLower()))?.Id
                    ?? GetRefId(offer.Type);

                if (typeId.HasValue)
                {
                    offerFromDb.WallMaterialId = typeId.Value;
                }
                else
                {
                    _logger.LogError(
                        "Unable to resove type \"{type}\" in object with internal id {internalId}. {AgencyId}",
                        offer.Type,
                        offer.InternalId,
                        agencyId
                    );
                }
            }

            if (!string.IsNullOrEmpty(offer.Category))
            {
                var categoryId = _types
                    .FirstOrDefault(t => t.Value.ToLower().Equals(offer.Category.ToLower()))?.Id
                    ?? GetRefId(offer.Category);

                if (categoryId.HasValue)
                {
                    offerFromDb.CategoryId = categoryId.Value;
                }
                else
                {
                    _logger.LogError(
                        "Unable to resove category \"{category}\" in object with internal id {internalId}. {AgencyId}",
                        offer.Category,
                        offer.InternalId,
                        agencyId
                    );
                }
            }
        }

        public async Task UpdateObjectsAsync(RealtyFeed feed, int agencyId)
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
                    MapObjects(newOffer, offer, agencyId);
                    DbContext.RealtyObjects.Add(newOffer);
                    continue;
                }

                MapObjects(offerFromDb, offer, agencyId);
            }

            await DbContext.SaveChangesAsync();
        }
    }
}