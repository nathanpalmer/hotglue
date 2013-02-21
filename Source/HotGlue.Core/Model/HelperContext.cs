using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HotGlue;

namespace HotGlue.Model
{
    public class HelperContext
    {
        public HelperContext(LoadedConfiguration configuration, IReferenceLocator locator, bool debug)
        {
            Configuration = configuration;
            Locator = locator;
            Debug = debug;
        }

        public LoadedConfiguration Configuration { get; private set; }
        public IReferenceLocator Locator { get; private set; }
        public bool Debug { get; private set; }
    }
}
