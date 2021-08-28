using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Masya.TelegramBot.Api.Xml
{
    [Serializable, XmlRoot(ElementName = "offer")]
    public sealed class Offer
    {
        [XmlAttribute(AttributeName = "internal-id")]
        public int InternalId { get; set; }

        [XmlElement(ElementName = "type", IsNullable = true)]
        public string Type { get; set; }

        [XmlElement(ElementName = "property-type", IsNullable = true)]
        public string PropertyType { get; set; }

        [XmlElement(ElementName = "category", IsNullable = true)]
        public string Category { get; set; }

        [XmlElement(ElementName = "url", IsNullable = true)]
        public string Url { get; set; }

        [XmlElement(ElementName = "creation-date")]
        public DateTime CreationDate { get; set; }

        [XmlElement(ElementName = "last-update-date")]
        public DateTime LastUpdateDate { get; set; }

        [XmlElement(ElementName = "location")]
        public Location Location { get; set; }

        [XmlElement(ElementName = "description")]
        public string Description { get; set; }

        [XmlElement(ElementName = "built-year", IsNullable = true)]
        public int BuiltYear { get; set; }

        [XmlElement(ElementName = "building-type", IsNullable = true)]
        public string BuildingType { get; set; }

        [XmlElement(ElementName = "renovation", IsNullable = true)]
        public string Renovation { get; set; }

        [XmlElement(ElementName = "gas-supply", IsNullable = true)]
        public string GasSupply { get; set; }

        [XmlElement(ElementName = "water-supply", IsNullable = true)]
        public string WaterSupply { get; set; }

        [XmlElement(ElementName = "sewerage-supply", IsNullable = true)]
        public string SewerageSupply { get; set; }

        [XmlElement(ElementName = "rooms")]
        public int Rooms { get; set; }

        [XmlElement(ElementName = "floor")]
        public int Floor { get; set; }

        [XmlElement(ElementName = "floors-total")]
        public int FloorsTotal { get; set; }

        [XmlElement(ElementName = "area")]
        public Area Area { get; set; }

        [XmlElement(ElementName = "living-space")]
        public Area LivingSpace { get; set; }

        [XmlElement(ElementName = "kitchen-space")]
        public Area KitchenSpace { get; set; }

        [XmlElement(ElementName = "lot-area")]
        public Area LotArea { get; set; }

        [XmlElement(ElementName = "balcony", IsNullable = true)]
        public string Balcony { get; set; }

        [XmlElement(ElementName = "phone")]
        public string Phone { get; set; }

        [XmlElement(ElementName = "price")]
        public Price Price { get; set; }

        [XmlElement(ElementName = "image")]
        public List<string> ImageUrls { get; set; }
    }
}