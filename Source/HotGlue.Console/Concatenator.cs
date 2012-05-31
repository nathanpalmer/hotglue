using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HotGlue.Model;

namespace HotGlue.Console
{
    public class Concatenator
    {
        public static string Compile(string inFilename, string root, string sharedScriptPath)
        {
            if (string.IsNullOrEmpty(inFilename))
            {
                throw new ArgumentNullException("inFilename");
            }
            if (string.IsNullOrEmpty(root))
            {
                throw new ArgumentNullException("root");
            }
            if (string.IsNullOrEmpty(sharedScriptPath))
            {
                throw new ArgumentNullException("sharedScriptPath");
            }
            var config = HotGlueConfiguration.Default();
            config.ScriptSharedPath = sharedScriptPath;
            var locator = new GraphReferenceLocator(config);
            var reference = new Reference
                                {
                                    Name = inFilename,
                                    Path = root
                                };
            var references = locator.Load(root, reference);
            var package = Package.Build(config, root);
            return package.Compile(references);
        }
    }
}
