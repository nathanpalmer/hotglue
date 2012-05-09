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
        public string ScriptSharedPath { get; set; }

        [XmlArray("generate")]
        public string GenerateScript { get; set; }

        [XmlArray("compilers")]
        [XmlArrayItem("compiler")]
        public HotGlueCompiler[] Compilers { get; set; }

        [XmlArray("referencers")]
        [XmlArrayItem("reference")]
        public HotGlueReference[] Referencers { get; set; }
    }

    public class HotGlueCompiler
    {
        [XmlAttribute("type")]
        public string Type { get; set; }
        [XmlAttribute("extension")]
        public string Extension { get; set; }
    }

    public class HotGlueReference
    {
        [XmlAttribute("type")]
        public string Type { get; set; }
    }
}
