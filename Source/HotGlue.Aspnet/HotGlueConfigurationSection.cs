using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.Configuration;
using System.Xml;
using System.Xml.Serialization;
using HotGlue.Model;

namespace HotGlue.Aspnet
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
                config.Debug = ((CompilationSection)ConfigurationManager.GetSection(@"system.web/compilation")).Debug;
                return config;
            }

            return null;
        }

        public static HotGlueConfiguration Load()
        {
            var debug = ((CompilationSection)ConfigurationManager.GetSection(@"system.web/compilation")).Debug;
            var configuration = HotGlueConfiguration.Load(debug);
            return configuration;
        }
    }
}
