using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace HotGlue.Model
{
    public class HotGlueConfiguration
    {
        [XmlArray("referencer")]
        public string Referencer { get; set; }

        [XmlArray("compilers")]
        [XmlArrayItem("compiler")]
        public HotGlueCompiler[] Compilers { get; set; }

        //public static HotGlueConfiguration Load()
    }

    public class HotGlueCompiler
    {
        [XmlAttribute("type")]
        public string Type { get; set; }
        [XmlAttribute("extension")]
        public string Extension { get; set; }
    }
}
