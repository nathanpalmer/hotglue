using System;
using System.Configuration;
using System.IO;
using System.Web;
using System.Web.Configuration;
using HotGlue.Model;
using HotGlue.Aspnet;

namespace HotGlue
{
    public static class Script
    {
        private static readonly Lazy<HelperContext> Context
            = new Lazy<HelperContext>(CreateContext, System.Threading.LazyThreadSafetyMode.PublicationOnly);

        static HelperContext CreateContext()
        {
            var debug = ((CompilationSection)ConfigurationManager.GetSection(@"system.web/compilation")).Debug;
            var config = HotGlueConfiguration.Load(debug);
            var configuration = LoadedConfiguration.Load(config);
            var locator = new GraphReferenceLocator(configuration);
            return new HelperContext(configuration, locator, debug);
        }

        public static IHtmlString Reference(params string[] names)
        {
            var context = HttpContext.Current;
            var root = context.Server.MapPath("~");
            return new HtmlString(ScriptHelper.Reference(Context.Value, root, names));
        }
    }
}