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

        private string _scriptPath;
        [XmlElement("scriptPath")]
        public string ScriptPath
        {
            get { return _scriptPath; }
            set { _scriptPath = value.Reslash(); }
        }

        private string _scriptSharedPath;
        [XmlElement("scriptSharedPath")]
        public string ScriptSharedPath
        {
            get { return _scriptSharedPath; }
            set { _scriptSharedPath = value.Reslash(); }
        }

        [XmlElement("generate")]
        public ObjectType GenerateScript { get; set; }

        [XmlArray("compilers")]
        [XmlArrayItem("compiler")]
        public HotGlueCompiler[] Compilers { get; set; }

        [XmlArray("referencers")]
        [XmlArrayItem("reference")]
        public HotGlueReference[] Referencers { get; set; }

        public HotGlueConfiguration()
        {
            ScriptPath = "Scripts";
            ScriptSharedPath = "Scripts\\Shared";
        }
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
