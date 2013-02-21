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

        [XmlElement("cache")]
        public ObjectType Cache { get; set; }

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

        public static HotGlueConfiguration Load(bool debug)
        {
            var section = (HotGlueConfiguration) ConfigurationManager.GetSection("hotglue");

            if (section != null &&
                section.Compilers != null && section.Compilers.Length > 0 &&
                section.Referencers != null && section.Referencers.Length > 0 &&
                section.GenerateScript != null)
            {
                // If all three sections are in the configuration we take it
                return section;
            }

            var configuration = new HotGlueConfiguration
                {
                    Debug = debug,
                    ScriptPath = section != null &&
                                 !string.IsNullOrWhiteSpace(section.ScriptPath)
                                     ? section.ScriptPath
                                     : "Scripts\\"
                };

            var assemblies = GetAssemblies();

            // Find Compiler
            if (section != null &&
                section.Compilers != null && section.Compilers.Length > 0)
            {
                // Pull from configuration
                configuration.Compilers = section.Compilers;
            }
            else
            {
                var compilers = assemblies.SelectMany(a => a.GetTypes())
                                          .Where(t => typeof (ICompile).IsAssignableFrom(t) &&
                                                      typeof (ICompress).IsAssignableFrom(t) == false &&
                                                      typeof (ICompile) != t)
                                          .Select(t => new HotGlueCompiler
                                              {
                                                  Type = t.AssemblyQualifiedName
                                              })
                                          .ToList();

                // Find Compressor
                if (!debug)
                {
                    compilers.AddRange(assemblies.SelectMany(a => a.GetTypes())
                                                 .Where(t => typeof (ICompress).IsAssignableFrom(t) &&
                                                             typeof (ICompress) != t)
                                                 .Select(t => new HotGlueCompiler
                                                     {
                                                         Type = t.AssemblyQualifiedName
                                                     }));
                }

                configuration.Compilers = compilers.ToArray();
            }

            // Script Generator
            if (section != null &&
                section.GenerateScript != null)
            {
                // Load from configuration
                configuration.GenerateScript = section.GenerateScript;
            }
            else
            {
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
            }

            // Script Cache
            if (section != null &&
                section.Cache != null)
            {
                // Load from configuration
                configuration.Cache = section.Cache;
            }

            // Find Referencers
            if (section != null &&
                section.Referencers != null && section.Referencers.Length > 0)
            {
                // Pull from configuration
                configuration.Referencers = section.Referencers;
            }
            else
            {
                configuration.Referencers = assemblies.SelectMany(a => a.GetTypes())
                                                      .Where(t => typeof (IFindReference).IsAssignableFrom(t) &&
                                                                  typeof (IFindReference) != t)
                                                      .Select(t => new HotGlueReference
                                                          {
                                                              Type = t.AssemblyQualifiedName
                                                          })
                                                      .ToArray();
            }

            return configuration;
        }

        public static List<Assembly> GetAssemblies()
        {
            var folder = new DirectoryInfo(Path.GetDirectoryName(Uri.UnescapeDataString(new UriBuilder(typeof(ICompile).Assembly.CodeBase).Path)));

            List<Assembly> assemblies = new List<Assembly>();

            var files = folder.GetFiles()
                              .Where(x => new[] {".exe", ".dll"}.Contains(x.Extension));

            var thisAssembly = typeof (HotGlueConfiguration).Assembly;
            assemblies.Add(thisAssembly);
            string thisAssemblyName = thisAssembly.FullName;
            foreach (var file in files)
            {
                AssemblyName name = new AssemblyName() { Name = System.IO.Path.GetFileNameWithoutExtension(file.Name) };
                try
                {
                    var asm = Assembly.Load(name);
                    // Only want assemblies which reference this one, as other assemblies won't
                    // implement our interfaces and scanning them is a waste of time / possibly 
                    // could fail if types can't be loaded.
                    if (asm.GetReferencedAssemblies().Any(ra => thisAssemblyName.Equals(ra.FullName)))
                    {
                        assemblies.Add(asm);
                    }
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
