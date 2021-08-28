using System;
using System.Xml.Serialization;

namespace Masya.TelegramBot.Api.Xml
{
    [Serializable, XmlRoot]
    public sealed class Price
    {
        [XmlElement("value")]
        public decimal Value { get; set; }

        [XmlElement("currency")]
        public string Currency { get; set; }
    }
}