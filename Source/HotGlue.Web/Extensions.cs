using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Configuration;
using HotGlue.Model;
using HotGlue.Web;

namespace HotGlue
{
    public static class Script
    {
        private static HotGlueConfiguration _configuration;
        private static Package _package;
        private static bool _debug;
        private static IReferenceLocator _locator;

        static Script()
        {
            _configuration = HotGlueConfigurationSection.Load();
            _package = Package.Build(_configuration);
            _debug = ((CompilationSection) ConfigurationManager.GetSection(@"system.web/compilation")).Debug;
            _locator = new GraphReferenceLocator(_configuration, new List<IFindReference>() { new SlashSlashEqualReference(), new RequireReference() });
        }

        public static string Reference(string name)
        {
            if (_debug)
            {
                var context = HttpContext.Current;
                var root = context.Server.MapPath("~");
                name = name.Replace("/", "\\");
                name = name.StartsWith("\\") ? name.Substring(1) : name;
                var file = Path.Combine(root, name.Replace("/", "\\"));
                var relative = file.Substring(0, file.LastIndexOf("\\")) + "\\";
                file = file.Substring(file.LastIndexOf("\\")+1);

                var reference = new Reference
                {
                    Path = relative,
                    Name = file,
                    Type = Model.Reference.TypeEnum.App
                };

                var references = _locator.Load(root, reference);

                return _package.References(references);
            }

            var handlerName = name.Replace(".js", ".jsglue");

            return _package.References(new[]
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
        public static Reference BuildReference(this HttpContext context, Reference.TypeEnum type)
        {
            var file = context.Server.MapPath(context.Request.AppRelativeCurrentExecutionFilePath);
            file = file.Substring(0, file.LastIndexOf(".js")+3);
            var relative = context.Server.MapPath(".") + "\\";
            file = file.Replace(relative, "");

            return new Reference
            {
                Path = relative,
                Name = file,
                Type = type
            };
        }
    }
}
