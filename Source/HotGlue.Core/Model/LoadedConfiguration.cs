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
        public IJavaScriptRuntime JavaScriptRuntime { get; private set; }
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

            if (configuration.JavaScriptRuntime != null)
            {
                loaded.JavaScriptRuntime = (IJavaScriptRuntime) Activator.CreateInstance(Type.GetType(configuration.JavaScriptRuntime.Type));
            }

            if (configuration == null || configuration.Compilers == null || configuration.Compilers.Length == 0)
            {
                loaded.Compilers = new ICompile[] { };
            }
            else
            {
                var argTypes = new [] {typeof (IJavaScriptRuntime)};
                loaded.Compilers = configuration.Compilers
                                         .Where(c => string.IsNullOrWhiteSpace(c.Mode) || c.Mode.Equals(configuration.Debug ? "debug" : "release", StringComparison.OrdinalIgnoreCase))
                                         .Select(compiler =>
                                         {
                                             var type = Type.GetType(compiler.Type);
                                             if (loaded.JavaScriptRuntime != null && type.GetConstructor(argTypes) != null)
                                             {
                                                 return (ICompile) Activator.CreateInstance(type, loaded.JavaScriptRuntime);
                                             }
                                             return (ICompile) Activator.CreateInstance(type);
                                         })
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