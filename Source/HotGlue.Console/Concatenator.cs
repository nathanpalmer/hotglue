using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HotGlue.Model;

namespace HotGlue.Console
{
    public class Concatenator
    {
        public static string Compile(string inFilename, string root)
        {
            var config = HotGlueConfiguration.Default();
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
