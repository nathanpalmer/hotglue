using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using HotGlue.Model;

namespace HotGlue.Web
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

        public static HotGlueConfiguration Load()
        {
            return (HotGlueConfiguration) ConfigurationManager.GetSection("hotglue")
                   ?? new HotGlueConfiguration
                       {
                           ScriptPath = "Scripts\\",
                           ScriptSharedFolder = "Scripts\\Shared\\"
                       };
        }
    }
}
