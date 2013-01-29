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
        private static LoadedConfiguration _configuration;
        private static bool _debug;
        private static IReferenceLocator _locator;

        static Script()
        {
            var config = HotGlueConfigurationSection.Load();
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