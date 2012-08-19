using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Globalization;
using System.IO;
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
        private HttpContextCache _cache;

        public HotGlueHandler()
        {
            _configuration = HotGlueConfigurationSection.Load();
            _locator = new GraphReferenceLocator(_configuration);
            _cache = new HttpContextCache();
        }

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "application/x-javascript";

            if (File.Exists(context.Request.PhysicalPath))
            {
                context.Response.TransmitFile(context.Request.PhysicalPath);
                return;
            }

            // find references
            var root = context.Server.MapPath("~");
            var reference = context.BuildReference(Reference.TypeEnum.App);
            var references = _locator.Load(root, reference);

            var package = Package.Build(_configuration, root, _cache);
            var content = package.Compile(references);

            context.Response.AddHeader("Content-Length", content.Length.ToString(CultureInfo.InvariantCulture));
            context.Response.Write(content);
        }

        public bool IsReusable
        {
            get { return true; }
        }
    }
}
