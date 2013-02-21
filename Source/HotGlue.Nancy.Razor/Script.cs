using System;
using HotGlue.Model;
using HotGlue.Nancy;
using Nancy;
using Nancy.ViewEngines.Razor;

namespace HotGlue
{
    public static class Script
    {
        private static readonly Lazy<HelperContext> Context 
            = new Lazy<HelperContext>(CreateContext, System.Threading.LazyThreadSafetyMode.PublicationOnly);

        static HelperContext CreateContext()
        {
            var debug = StaticConfiguration.IsRunningDebug;
            var config = HotGlueConfiguration.Load(debug);
            var configuration = LoadedConfiguration.Load(config);
            var locator = new GraphReferenceLocator(configuration);
            return new HelperContext(configuration, locator, debug);
        }

        public static IHtmlString Reference(params string[] names)
        {
            var root = HotGlueNancyStartup.Root;
            return new NonEncodedHtmlString(ScriptHelper.Reference(Context.Value, root, names));
        }
    }
}
