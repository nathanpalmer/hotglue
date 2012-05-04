using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web.Configuration;
using HotGlue.Model;

namespace HotGlue
{
    public static class Script
    {
        private static HotGlueConfiguration _configuration;
        private static Package _package;
        private static bool _debug;

        static Script()
        {
            _configuration = (HotGlueConfiguration)ConfigurationManager.GetSection("hotglue");
            _package = Package.Build(_configuration);
            CompilationSection compilationSection = (CompilationSection) ConfigurationManager.GetSection(@"system.web/compilation");
            _debug = compilationSection.Debug;
        }

        public static string Reference(string name)
        {
            if (_debug)
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

            var handlerName = name.Replace(".js", ".jsglue");

            return _package.References(new[]
                {
                    new Reference
                        {
                            Root = "",
                            Path = handlerName,
                            Module = false
                        }
                });
        }
    }
}
