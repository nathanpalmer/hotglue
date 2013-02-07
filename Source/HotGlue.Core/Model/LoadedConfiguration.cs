using System;
using System.Collections.Generic;
using System.Linq;

namespace HotGlue.Model
{
    public class LoadedConfiguration
    {
        public string ScriptPath { get; set; }
        public IGenerateScriptReference GenerateScriptReference { get; private set; }
        public IFileCache FileCache { get; private set; }
        public ICompile[] Compilers { get; private set; }
        public IFindReference[] FindReferences { get; private set; }

        public static LoadedConfiguration Load(HotGlueConfiguration configuration)
        {
            var loaded = new LoadedConfiguration();

            loaded.ScriptPath = configuration.ScriptPath;

            if (configuration == null || configuration.GenerateScript == null)
            {
                loaded.GenerateScriptReference = new HTMLGenerateScriptReference();
            }
            else
            {
                loaded.GenerateScriptReference = (IGenerateScriptReference)Activator.CreateInstance(Type.GetType(configuration.GenerateScript.Type));
            }

            if (configuration == null || configuration.Cache == null)
            {
                loaded.FileCache = null;
            }
            else
            {
                loaded.FileCache = (IFileCache)Activator.CreateInstance(Type.GetType(configuration.Cache.Type));
            }

            if (configuration == null || configuration.Compilers == null || configuration.Compilers.Length == 0)
            {
                loaded.Compilers = new ICompile[] { };
            }
            else
            {
                loaded.Compilers = configuration.Compilers
                                         .Where(c => string.IsNullOrWhiteSpace(c.Mode) || c.Mode.Equals(configuration.Debug ? "debug" : "release", StringComparison.OrdinalIgnoreCase))
                                         .Select(compiler => (ICompile)Activator.CreateInstance(Type.GetType(compiler.Type)))
                                         .ToArray();
            }

            if (configuration.Referencers == null || configuration.Referencers.Length == 0)
            {
                loaded.FindReferences = new IFindReference[]
                    {
                        new SlashSlashEqualReference(),
                        new RequireReference(),
                        new TripleSlashReference()
                    };
            }
            else
            {
                loaded.FindReferences = configuration.Referencers
                                         .Select(r => (IFindReference)Activator.CreateInstance(Type.GetType(r.Type)))
                                         .ToArray();
            }

            return loaded;
        }
    }
}