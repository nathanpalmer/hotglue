using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace HotGlue.Model
{
    public class HotGlueConfiguration
    {
        public string ScriptPath { get; set; }
        public string ScriptSharedFolder { get; set; }

        [XmlArray("referencer")]
        public string Referencer { get; set; }

        [XmlArray("compilers")]
        [XmlArrayItem("compiler")]
        public HotGlueCompiler[] Compilers { get; set; }

        //public static HotGlueConfiguration Load()

        public HotGlueConfiguration()
        {
            ScriptPath = "~/Scripts";
        }
    }

    public class HotGlueCompiler
    {
        [XmlAttribute("type")]
        public string Type { get; set; }
        [XmlAttribute("extension")]
        public string Extension { get; set; }
    }
}
