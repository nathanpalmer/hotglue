using System;
using System.Configuration;
using System.IO;
using System.Web;
using System.Web.Configuration;
using HotGlue.Model;
using HotGlue.Aspnet;
using HotGlueConfigurationSection = HotGlue.Configuration.HotGlueConfigurationSection;

namespace HotGlue
{
    public static class Script
    {
        private static LoadedConfiguration _configuration;
        private static bool _debug;
        private static IReferenceLocator _locator;

        static Script()
        {
            var debug = ((CompilationSection)ConfigurationManager.GetSection(@"system.web/compilation")).Debug;
            var config = HotGlueConfiguration.Load(debug);
            _configuration = LoadedConfiguration.Load(config);
            _debug = ((CompilationSection) ConfigurationManager.GetSection(@"system.web/compilation")).Debug;
            _locator = new GraphReferenceLocator(_configuration);
        }

        public static IHtmlString Reference(string name)
        {
            var context = HttpContext.Current;
            var root = context.Server.MapPath("~");
            return new HtmlString(ScriptHelper.Reference(_configuration, _locator, root, name, _debug));
        }
    }
}