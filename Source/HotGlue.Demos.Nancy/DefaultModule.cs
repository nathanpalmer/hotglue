using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Nancy;

namespace HotGlue.Demos.Nancy
{
    public class DefaultModule : NancyModule
    {
        public DefaultModule()
        {
            Get["/"] = parameters =>
                {
                    return View["Index", new { Test = "Test" }];
                };
        }
    }
}