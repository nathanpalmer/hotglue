using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            var controllerName = reference.Name.Replace(Extensions.First(), "");
            if (controllerName.Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                reference.Content = "var routes = 'all routes generated';";
            }

            reference.Content = "var routes = '" + controllerName + " route generated';";
        }
    }
}
