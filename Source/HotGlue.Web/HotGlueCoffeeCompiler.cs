using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using HotGlue.Model;

namespace HotGlue.Web
{
    public class HotGlueCoffeeCompiler : IHttpHandler
    {
        private HotGlueConfiguration _configuration;

        public HotGlueCoffeeCompiler()
        {
            _configuration = HotGlueConfigurationSection.Load();
        }

        public void ProcessRequest(HttpContext context)
        {
            // find references
            var root = context.Server.MapPath("~");
            var reference = context.BuildReference(Reference.TypeEnum.Dependency);
            
            var package = Package.Build(_configuration, root);
            var content = package.CompileDependency(reference);

            context.Response.ContentType = "application/x-javascript";
            context.Response.AddHeader("Content-Length", content.Length.ToString(CultureInfo.InvariantCulture));
            context.Response.Write(content);
        }

        public bool IsReusable
        {
            get { return true; }
        }
    }
}
