using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using HotGlue.Compilers;
using HotGlue.Model;

namespace HotGlue.Web
{
    public class HotGlueHandler : IHttpHandler
    {
        private IReferenceLocator _locator;
        private HotGlueConfiguration _configuration;

        public HotGlueHandler()
        {
            _configuration = HotGlueConfigurationSection.Load();
            _locator = new GraphReferenceLocator(_configuration);
        }

        public void ProcessRequest(HttpContext context)
        {
            // find references
            var root = context.Server.MapPath("~");
            var package = Package.Build(_configuration, root);
            var reference = context.BuildReference(Reference.TypeEnum.App);
            var references = _locator.Load(root, reference);
            
            var content = package.Compile(references);

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
