using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
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
        }

        public static HotGlueConfiguration Default()
        {
            return new HotGlueConfiguration()
            {
               Referencers = new []
                    {
                        new HotGlueReference { Type = typeof(SlashSlashEqualReference).FullName }, 
                        new HotGlueReference { Type = typeof(RequireReference).FullName },
                        new HotGlueReference { Type = typeof(TripleSlashReference).FullName }
                    }
            };
        }

        public static HotGlueConfiguration Load()
        {
            var section = (HotGlueConfiguration)ConfigurationManager.GetSection("hotglue");
            if (section != null) return section;

            var configuration = new HotGlueConfiguration
            {
                ScriptPath = "Scripts\\"
            };

            var assemblies = GetAssemblies();

            // Find Compiler
            configuration.Compilers = assemblies.SelectMany(a => a.GetTypes())
                                                .Where(t => typeof (ICompile).IsAssignableFrom(t) &&
                                                            typeof (ICompile) != t)
                                                .Select(t => new HotGlueCompiler
                                                    {
                                                        Type = t.AssemblyQualifiedName,
                                                        //TODO: Not sure we're actually using these extensions anywhere
                                                        //Extension = String.Join(",", ((ICompile) Activator.CreateInstance(t)).Extensions)
                                                    })
                                                .ToArray();

            // Script Generator
            var generate = assemblies.SelectMany(a => a.GetTypes())
                                     .Where(t => typeof (IGenerateScriptReference).IsAssignableFrom(t) &&
                                                 typeof (IGenerateScriptReference) != t &&
                                                 typeof (HTMLGenerateScriptReference) != t)
                                     .Select(t => new HotGlueGenerator
                                         {
                                             Type = t.AssemblyQualifiedName
                                         })
                                     .FirstOrDefault();
            if (generate != null)
            {
                configuration.GenerateScript = generate;
            }

            // Find Referencers
            configuration.Referencers = assemblies.SelectMany(a => a.GetTypes())
                                                  .Where(t => typeof (IFindReference).IsAssignableFrom(t) &&
                                                              typeof (IFindReference) != t)
                                                  .Select(t => new HotGlueReference
                                                      {
                                                          Type = t.AssemblyQualifiedName
                                                      })
                                                  .ToArray();

            // Find Compressors
            //TODO: Right now there isn't a way to scan for compressors because they are just compilers

            return configuration;
        }

        public static List<Assembly> GetAssemblies()
        {
            var folder = new DirectoryInfo(Path.GetDirectoryName(Uri.UnescapeDataString(new UriBuilder(typeof(ICompile).Assembly.CodeBase).Path)));

            List<Assembly> assemblies = new List<Assembly>();

            var files = folder.GetFiles()
                              .Where(x => new[] {".exe", ".dll"}.Contains(x.Extension));

            foreach (var file in files)
            {
                AssemblyName name = new AssemblyName() { Name = System.IO.Path.GetFileNameWithoutExtension(file.Name) };
                try
                {
                    var asm = Assembly.Load(name);
                    assemblies.Add(asm);
                }
                catch
                {
                    // ignore exceptions here...
                }
            }

            return assemblies;
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
