using System;
using System.Xml.Serialization;

namespace Masya.TelegramBot.Api.Xml
{
    [Serializable, XmlRoot]
    public sealed class Area
    {
        [XmlElement(ElementName = "value")]
        public int Value { get; set; }

        [XmlElement(ElementName = "unit")]
        public string Unit { get; set; }
    }
}