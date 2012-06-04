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

        public static HtmlString Reference(string name)
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

                return new HtmlString(package.References(references));
            }

            var appName = name + "-glue";
            var appDirectory = new DirectoryInfo(context.Server.MapPath("."));
            var appFile = new FileInfo(Path.Combine(context.Server.MapPath("~") + _configuration.ScriptPath, appName));
            var appReference = new SystemReference(appDirectory, appFile, appName) {Type = Model.Reference.TypeEnum.App};

            return new HtmlString(package.References(new[] {appReference}));
        }
    }

    public static class Extensions
    {
        private static readonly Regex FileNameRegex = new Regex(@"(?<file>\S+)(?<extension>\.(.+(?=(-module|-glue|-require))|.+))");

        public static SystemReference BuildReference(this HttpContext context, Reference.TypeEnum type)
        {
            var fullPath = context.Server.MapPath(context.Request.AppRelativeCurrentExecutionFilePath);
            var name = Path.GetFileName(fullPath);
            var directory = Path.GetDirectoryName(fullPath);

            var match = FileNameRegex.Match(name);
            name = match.Groups["file"].Value + match.Groups["extension"].Value;

            var reference = new SystemReference(new DirectoryInfo(context.Server.MapPath("~")), new FileInfo(Path.Combine(directory, name)), name);
            var keys = context.Request.QueryString["name"] ?? "";
            foreach(var key in keys.Split(','))
            {
                reference.ReferenceNames.Add(key);
            }
            return reference;
        }
    }
}
