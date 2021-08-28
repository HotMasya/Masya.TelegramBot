using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Masya.TelegramBot.Api.Xml
{
    public static class XmlHelper
    {
        public static T ParseFromXml<T>(Stream stream)
        {
            var settings = new XmlReaderSettings();
            settings.DtdProcessing = DtdProcessing.Ignore;
            using XmlReader reader = XmlReader.Create(stream, settings);
            var serializer = new XmlSerializer(typeof(T));
            var result = (T)serializer.Deserialize(stream);

            return result ?? default(T);
        }
    }
}