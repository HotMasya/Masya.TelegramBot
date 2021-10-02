using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Masya.TelegramBot.Api.Xml
{
    [Serializable, XmlRoot]
    public sealed class SalesAgent
    {
        [XmlElement(ElementName = "phone")]
        public List<string> Phones { get; set; }
    }
}