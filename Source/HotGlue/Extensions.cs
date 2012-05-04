using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using HotGlue.Model;

namespace HotGlue
{
    public static class Script
    {
        private static HotGlueConfiguration _configuration;
        private static Package _package;

        static Script()
        {
            _configuration = (HotGlueConfiguration)ConfigurationManager.GetSection("hotglue");
            _package = Package.Build(_configuration);
        }

        public static string Reference(string name)
        {
            // Find other references
            var references = new[]
                {
                    new Reference
                        {
                            Root = "",
                            Path = name,
                            Module = false
                        }
                };

            return _package.References(references);
        }
    }
}
