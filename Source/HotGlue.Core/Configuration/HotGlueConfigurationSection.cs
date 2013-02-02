using System.Configuration;
using System.Xml;
using System.Xml.Serialization;
using HotGlue.Model;

namespace HotGlue.Configuration
{
    public class HotGlueConfigurationSection : IConfigurationSectionHandler
    {
        public object Create(object parent, object configContext, XmlNode section)
        {
            var t = typeof(HotGlueConfiguration);
            var ser = new XmlSerializer(t);

            var config = ser.Deserialize(new XmlNodeReader(section)) as HotGlueConfiguration;
            if (config != null)
            {
                return config;
            }

            return null;
        }
    }
}
