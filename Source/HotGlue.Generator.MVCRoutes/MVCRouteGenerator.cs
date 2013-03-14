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
        public static RouteCollection routes;
        private static Assembly _assembly;
        public List<string> Extensions { get; private set; }

        public MVCRouteGenerator()
        {
            Extensions = new List<string>(new[] { ".routes" });
        }

        public static void RegisterRoutes(RouteCollection routes, Assembly mvcAssembly)
        {
            Validate(routes, mvcAssembly);
            MVCRouteGenerator.routes = routes;
            _assembly = mvcAssembly;
        }

        public bool Handles(string Extension)
        {
            return Extensions.Any(e => e == Extension);
        }

        private static void Validate(RouteCollection routes, Assembly assembly)
        {
            if (routes == null)
                throw new Exception("You must register the routes in global.aspx");

            if (routes.Count == 0)
                throw new Exception("No routes found");

            if (assembly == null)
                throw new Exception("Assembly must not be null");
        }

        public void Compile<T>(ref T reference) where T : Reference
        {
            Validate(routes, _assembly);

            var template = new Template();
            var controllerName = reference.Name.Replace(Extensions.First(), "");
            if (controllerName.Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                controllerName = null;
            }

            var model = new JavaScriptRoutingModel(_assembly, controllerName);
            reference.Content = template.Render(model);
        }
    }
}
