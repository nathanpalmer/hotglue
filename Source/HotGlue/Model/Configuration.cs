using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace HotGlue.Model
{
    [XmlRoot("hotglue", Namespace = "")]
    public class HotGlueConfiguration
    {
        [XmlAttribute("debug")]
        public bool Debug { get; set; }

        [XmlElement("scriptPath")]
        public string ScriptPath { get; set; }
        [XmlElement("scriptSharedPath")]
        public string ScriptSharedPath { get; set; }

        [XmlElement("generate")]
        public ObjectType GenerateScript { get; set; }

        [XmlArray("compilers")]
        [XmlArrayItem("compiler")]
        public HotGlueCompiler[] Compilers { get; set; }

        [XmlArray("referencers")]
        [XmlArrayItem("reference")]
        public HotGlueReference[] Referencers { get; set; }
    }

    public class HotGlueCompiler : ObjectType
    {
        [XmlAttribute("extension")]
        public string Extension { get; set; }
        [XmlAttribute("mode")]
        public string Mode { get; set; }
    }

    public class HotGlueReference : ObjectType
    {
    }

    public class HotGlueGenerator : ObjectType
    {
    }

    public class ObjectType
    {
        [XmlAttribute("type")]
        public string Type { get; set; }
    }
}
