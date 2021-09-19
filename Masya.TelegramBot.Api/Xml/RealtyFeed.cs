using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Masya.TelegramBot.Api.Xml
{
    [Serializable, XmlRoot(ElementName = "realty-feed", Namespace = "http://webmaster.yandex.ru/schemas/feed/realty/2010-06")]
    public sealed class RealtyFeed
    {
        [XmlElement(ElementName = "generation-date")]
        public DateTime GenerationDate { get; set; }

        [XmlElement(ElementName = "offer")]
        public List<Offer> Offers { get; set; }
    }
}