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
        private static readonly LoadedConfiguration _configuration;
        private static readonly bool _debug;
        private static readonly IReferenceLocator _locator;

        static Script()
        {
            _debug = StaticConfiguration.IsRunningDebug;
            var config = HotGlueConfiguration.Load(_debug);
            _configuration = LoadedConfiguration.Load(config);
            _locator = new GraphReferenceLocator(_configuration);
        }

        public static string Reference(params string[] names)
        {
            var root = HotGlueNancyStartup.Root;
            return ScriptHelper.Reference(_configuration, _locator, root, names, _debug);
        }
    }
}
