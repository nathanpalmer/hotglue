using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.Mvc;

namespace HotGlue.Generator.MVCRoutes
{
    public class JavaScriptRoutingModel
    {
        public SortedDictionary<string, SortedDictionary<string, string[]>> controllers { get; private set; }

        public JavaScriptRoutingModel(Assembly mvcAssembly, String controllerName)
        {
            controllers = GetControllers(mvcAssembly, controllerName);
        }
        
        public SortedDictionary<string, SortedDictionary<string, string[]>> GetControllers(Assembly mvcAssembly, String controllerName)
        {
            var controllerBase = typeof(Controller);
            var controllerInfo = mvcAssembly.GetTypes().Where(t => t != controllerBase && 
                                                                   controllerBase.IsAssignableFrom(t) && 
                                                                   (
                                                                        String.IsNullOrWhiteSpace(controllerName) ||
                                                                        t.Name.Equals(controllerName + "Controller", StringComparison.OrdinalIgnoreCase)
                                                                   )
                                                             );
            return ToMvcActionList(controllerInfo);
        }

        private static SortedDictionary<string, SortedDictionary<string, string[]>> ToMvcActionList(IEnumerable<Type> controllerInfo)
        {
            var controllers = new SortedDictionary<string, SortedDictionary<string, string[]>>();
            foreach (var controller in controllerInfo)
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
