using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using HotGlue.Model;
using HotGlue.Nancy;
using Nancy;

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

        public static string Reference(params string[] names)
        {
            return Reference(Context.Value.DefaultOptions, names);
        }

        public static string Reference(HelperOptions options, params string[] names)
        {
            var root = HotGlueNancyStartup.Root;
            return ScriptHelper.Reference(Context.Value, options, root, names);
        }
    }
}
