using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Routing;
using HotGlue.Generator.MVCRoutes;
using HotGlue.Model;

namespace HotGlue.Generator
{
    public class MVCRouteGenerator : ICompile
    {
        public List<string> Extensions { get; private set; }

        public MVCRouteGenerator()
        {
            Extensions = new List<string>(new[] { ".routes" });
        }

        public bool Handles(string Extension)
        {
            return Extensions.Any(e => e == Extension);
        }

        public void Compile<T>(ref T reference) where T : Reference
        {
            var template = new Template();
            var controllerName = reference.Name.Replace(Extensions.First(), "");
            if (controllerName.Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                controllerName = null;
            }

            var model = MVCRouteConfiguration.Current.GetModel(controllerName);
            reference.Content = template.Render(model);
        }
    }
}
