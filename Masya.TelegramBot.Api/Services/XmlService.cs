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
        private readonly IEnumerable<Category> _categories;

        public XmlService(ApplicationDbContext dbContext, ILogger<IXmlService> logger)
        {
            DbContext = dbContext;
            _logger = logger;

            _categories = DbContext.Categories.ToList();

            var diretoryItems = DbContext.DirectoryItems.Include(i => i.Directory).ToList();
            _streets = diretoryItems.Where(s => s.Directory.Id == (int)DirectoryType.Street);
            _districts = diretoryItems.Where(s => s.Directory.Id == (int)DirectoryType.District);
            _wallMaterials = diretoryItems.Where(s => s.Directory.Id == (int)DirectoryType.Material);
            _states = diretoryItems.Where(s => s.Directory.Id == (int)DirectoryType.State);
        }

        public async Task<RealtyFeed> GetRealtyFeed(HttpContent content)
        {
            var stream = await content.ReadAsStreamAsync();
            return XmlHelper.ParseFromXml<RealtyFeed>(stream);
        }

        private int? GetRefId(string value)
        {
            var reference = DbContext.References.FirstOrDefault(r => r.Value.ToLower() == value.ToLower());
            return reference?.ReferenceId;
        }

        private void MapObjects(RealtyObject offerFromDb, Offer offer, int agencyId)
        {
            offerFromDb.Floor = offer.Floor == 0 ? null : offer.Floor;
            offerFromDb.TotalFloors = offer.FloorsTotal == 0 ? null : offer.FloorsTotal;
            offerFromDb.Description = offer.Description;
            offerFromDb.CreatedAt = offer.CreationDate;
            offerFromDb.EditedAt = offer.CreationDate;
            offerFromDb.KitchenSpace = offer.KitchenSpace?.Value == 0 ? null : offer.KitchenSpace?.Value;
            offerFromDb.TotalArea = offer.Area?.Value == 0 ? null : offer.Area?.Value;
            offerFromDb.LivingSpace = offer.LivingSpace?.Value == 0 ? null : offer.LivingSpace?.Value;
            offerFromDb.LotArea = offer.LotArea?.Value == 0 ? null : offer.LotArea?.Value;

            if (offer.SalesAgent?.Phones != null && offer.SalesAgent.Phones.Count > 0)
            {
                offerFromDb.Phone = string.Join(", ", offer.SalesAgent.Phones);
            }

            if (offer.Price != null)
            {
                offerFromDb.Price = (int?)offer.Price.Value;
            }

            if (offerFromDb.Images != null)
            {
                DbContext.Images.RemoveRange(offerFromDb.Images);
            }

            if (offer.ImageUrls != null && offer.ImageUrls.Count > 0)
            {
                offerFromDb.Images = new List<Image>();
                foreach (string imageUrl in offer.ImageUrls)
                {
                    if (!string.IsNullOrEmpty(imageUrl))
                    {
                        offerFromDb.Images.Add(new Image
                        {
                            RealtyObjectId = offerFromDb.Id,
                            Url = imageUrl
                        });
                    }
                }
            }

            if (!string.IsNullOrEmpty(offer.Location.District))
            {
                var districtId = _districts
                    .FirstOrDefault(
                        d => d.Value.ToLower().Equals(offer.Location.District.Trim().ToLower())
                    )?.Id
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
                    .FirstOrDefault(
                        s => s.Value.ToLower().Equals(offer.Location.Address.Trim().ToLower())
                    )?.Id
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
                    .FirstOrDefault(
                        s => s.Value.ToLower().Equals(offer.Renovation.Trim().ToLower())
                    )?.Id
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
                    .FirstOrDefault(
                        w => w.Value.ToLower().Equals(offer.BuildingType.Trim().ToLower())
                    )?.Id
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

            if (!string.IsNullOrEmpty(offer.Category))
            {
                var categoryId = _categories
                    .FirstOrDefault(
                        t => t.Name.ToLower().Equals(offer.Category.Trim().ToLower())
                    )?.Id
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
                .Include(ro => ro.Images)
                .Include(ro => ro.Category)
                .Include(ro => ro.District)
                .Include(ro => ro.State)
                .Include(ro => ro.Street)
                .Include(ro => ro.WallMaterial)
                .ToListAsync();

            foreach (var offer in feed.Offers)
            {
                var offerFromDb = realtyObjects
                    .FirstOrDefault(
                        o => o.InternalId.HasValue && o.InternalId.Value == offer.InternalId
                    );

                if (offerFromDb is null)
                {
                    var newOffer = new RealtyObject
                    {
                        InternalId = offer.InternalId
                    };
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