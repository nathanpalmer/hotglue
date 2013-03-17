using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;

namespace HotGlue.Generator
{
    public static class MVCRouteConfiguration
    {
        public static void Initialize(RouteCollection routes, Assembly mvcAssembly)
        {
            current = new MVCRouteSettings(routes, mvcAssembly);
        }

        public static void Initialize(Action<RouteCollection> registerRoutes, Assembly mvcAssembly)
        {
            RouteCollection routes = new RouteCollection();
            registerRoutes(routes);
            Initialize(routes, mvcAssembly);
        }

        private static MVCRouteSettings current;

        public static MVCRouteSettings Current
        {
            get
            {
                if (current == null)
                    throw new Exception("MVC Route settings have not been initialized");

                return current;
            }
        }

        public class MVCRouteSettings
        {
            private static object lockObject = new object();
            private Assembly MvcAssembly;
            private ConcurrentDictionary<string, Type> controllers;

            public RouteCollection Routes { get; private set; }

            public MVCRouteSettings(RouteCollection routes, Assembly mvcAssembly)
            {
                if (routes == null)
                    throw new ArgumentNullException("routes");

                if (routes.Count == 0)
                    throw new ArgumentException("No routes found", "routes");

                if (mvcAssembly == null)
                    throw new ArgumentNullException("mvcAssembly");

                Routes = routes;
                MvcAssembly = mvcAssembly;
            }

            public TemplateModel GetModel(string controllerName = null)
            {
                lock (lockObject)
                {
                    if (controllers == null)
                        GetControllers();
                }

                if (!String.IsNullOrWhiteSpace(controllerName))
                    return ToModel(controllers[RemoveControllerFromName(controllerName)]);

                return ToModel(controllers.Values);
            }

            private void GetControllers()
            {
                var controllerBase = typeof(Controller);
                var controllerInfo = MvcAssembly.GetTypes().Where(t => t != controllerBase && controllerBase.IsAssignableFrom(t));
                controllers = new ConcurrentDictionary<string, Type>();
                foreach (var controller in controllerInfo)
                {
                    controllers.AddOrUpdate(RemoveControllerFromName(controller.Name), controller, (name, type) => type);
                }
            }

            private static String RemoveControllerFromName(String controllerName)
            {
                var startIndex = controllerName.LastIndexOf("Controller", StringComparison.OrdinalIgnoreCase);
                if (startIndex >= 0) // Case insensitive replace
                {
                    return controllerName.Substring(0, startIndex).Trim().ToLower();
                }
                return controllerName.Trim().ToLower();
            }

            private TemplateModel ToModel(Type controllerType)
            {
                return ToModel(new[] {controllerType});
            }

            private TemplateModel ToModel(IEnumerable<Type> controllerType)
            {
                var controllers = new TemplateModel();
                foreach (var controller in controllerType)
                {
                    var controllerDictionary = new SortedDictionary<string, string[]>();

                    var methodInfoCollection = controller.GetMethods(BindingFlags.Public | BindingFlags.Instance |
                                                                     BindingFlags.DeclaredOnly)
                                                         .Where(m => !m.IsSpecialName)
                                                         .Where(m => !m.GetCustomAttributes(typeof(NonActionAttribute), true).Any());
                    var methodDistinct = methodInfoCollection.Select(m => m.Name).Distinct();
                    foreach (var methodName in methodDistinct)
                    {
                        var parameters = new List<string>();
                        var methods = methodInfoCollection.Where(m => m.Name == methodName);
                        foreach (var method in methods)
                        {
                            foreach (var parameter in method.GetParameters())
                            {
                                ParameterInfo param = parameter;
                                if (!parameters.Any(p => p == param.Name))
                                {
                                    parameters.Add(parameter.Name);
                                }
                            }
                        }

                        controllerDictionary.Add(methodName, parameters.ToArray());
                    }

                    controllers.Add(controller.Name.Replace("Controller", ""), controllerDictionary);
                }
                return controllers;
            }
        }
    }

    // Just so you don't have to type so much
    public class TemplateModel : SortedDictionary<string, SortedDictionary<string, string[]>>
    {
        
    }
}
