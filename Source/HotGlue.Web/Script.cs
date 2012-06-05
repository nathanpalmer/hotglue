using System;
using System.Configuration;
using System.IO;
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
                name = name.Reslash();

                string file = name.StartsWith("/")
                                  ? name.Substring(1)
                                  : Path.Combine(_configuration.ScriptPath.Reslash(), name).Reslash();
                file = file.StartsWith("/") ? file.Substring(1) : file;

                name = file.Substring(file.LastIndexOf("/", StringComparison.Ordinal)+1);

                var reference = new SystemReference(new DirectoryInfo(root), new FileInfo(Path.Combine(root, file)), name);

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
}