using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using RazorEngine;
using RazorEngine.Configuration;
using RazorEngine.Templating;

namespace HotGlue.Generator.MVCRoutes
{
    public class Template
    {
        public String Render(JavaScriptRoutingModel javaScriptRoutingModel)
        {
            var assembly = Assembly.GetExecutingAssembly();
            string template;
            using (var sr = new StreamReader(assembly.GetManifestResourceStream("HotGlue.Generator.MVCRoutes.Templates.Routing.razor")))
            {
                template = sr.ReadToEnd();
            }
            var config = new TemplateServiceConfiguration
                {
                    BaseTemplateType = typeof (JavaScriptRoutingTemplateBase<>)
                };
            string result;
            try
            {
                using (var service = new TemplateService(config))
                {
                    Razor.SetTemplateService(service);
                    result = Razor.Parse(template, javaScriptRoutingModel);
                    return result;
                }
            }
            catch (TemplateCompilationException ex)
            {
                foreach (var error in ex.Errors)
                {
                    Console.WriteLine(error.ErrorText);
                }
                throw;
            }
        }
    }
}
