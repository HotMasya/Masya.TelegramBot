using System;
using System.Xml.Serialization;

namespace Masya.TelegramBot.Api.Xml
{
    [Serializable, XmlRoot(ElementName = "location")]
    public sealed class Location
    {
        [XmlElement(ElementName = "country")]
        public string Country { get; set; }

        [XmlElement(ElementName = "region")]
        public string Region { get; set; }

        [XmlElement(ElementName = "locality-name")]
        public string LocalityName { get; set; }

        [XmlElement(ElementName = "district")]
        public string District { get; set; }

        [XmlElement(ElementName = "direction")]
        public string Direction { get; set; }

        [XmlElement(ElementName = "address")]
        public string Address { get; set; }
    }
}