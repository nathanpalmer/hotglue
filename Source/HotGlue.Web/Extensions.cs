using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Configuration;
using HotGlue.Model;
using HotGlue.Web;

namespace HotGlue
{
    public static class Script
    {
        private static HotGlueConfiguration _configuration;
        private static bool _debug;
        private static IReferenceLocator _locator;

        static Script()
        {
            _configuration = HotGlueConfigurationSection.Load();
            _debug = ((CompilationSection) ConfigurationManager.GetSection(@"system.web/compilation")).Debug;
            _locator = new GraphReferenceLocator(_configuration);
        }

        public static string Reference(string name)
        {
            var context = HttpContext.Current;
            var root = context.Server.MapPath("~");
            var package = Package.Build(_configuration, root);

            if (_debug)
            {
                name = name.Replace("/", "\\");

                string file = name.StartsWith("\\")
                                  ? name.Substring(1)
                                  : Path.Combine(_configuration.ScriptPath.Replace("/", "\\"), name);

                var relative = file.Substring(0, file.LastIndexOf("\\", StringComparison.Ordinal)) + "\\";
                file = file.Substring(file.LastIndexOf("\\", StringComparison.Ordinal)+1);

                var reference = new Reference
                {
                    Path = relative,
                    Name = file,
                    Type = Model.Reference.TypeEnum.App
                };

                var references = _locator.Load(root, reference);

                return package.References(references);
            }

            var handlerName = name + "-glue";

            return package.References(new[]
                {
                    new Reference
                        {
                            Path = "",
                            Name = handlerName,
                            Type = Model.Reference.TypeEnum.App
                        }
                });
        }
    }

    public static class Extensions
    {
        private static readonly Regex FileNameRegex = new Regex(@"(?<file>\S+)(?<extension>\.\S+)(?:-module|-glue|-require)");

        public static Reference BuildReference(this HttpContext context, Reference.TypeEnum type)
        {
            var file = context.Server.MapPath(context.Request.AppRelativeCurrentExecutionFilePath);
            var relative = context.Server.MapPath(".") + "\\";

            var match = FileNameRegex.Match(file);
            var extension = match.Groups["extension"].Value;
            file = match.Groups["file"].Value + extension;
            file = file.Replace(relative, "");

            return new Reference
            {
                Path = relative,
                Name = file,
                Type = type,
                Extension = extension
            };
        }
    }
}
