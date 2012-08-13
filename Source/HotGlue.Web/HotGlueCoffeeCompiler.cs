using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using HotGlue.Model;

namespace HotGlue.Web
{
    public class HotGlueCoffeeCompiler : IHttpHandler
    {
        private HotGlueConfiguration _configuration;
        private IFileCache _cache;

        public HotGlueCoffeeCompiler()
        {
            _configuration = HotGlueConfigurationSection.Load();
            _cache = new HttpContextCache();
        }

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "application/x-javascript";

            // find references
            var root = context.Server.MapPath("~");
            var reference = context.BuildReference(Reference.TypeEnum.Dependency);
            var file = new FileInfo(reference.FullPath);

            dynamic cached = _cache.Get(file.FullName);
            if (cached != null && cached.File.LastWriteTimeUtc.Equals(file.LastWriteTimeUtc))
            {
                context.Response.AddHeader("Content-Length", cached.Content.Length.ToString(CultureInfo.InvariantCulture));
                context.Response.Write(cached.Content);
                return;
            }
            
            var package = Package.Build(_configuration, root);
            var content = package.CompileDependency(reference);

            context.Response.AddHeader("Content-Length", content.Length.ToString(CultureInfo.InvariantCulture));
            context.Response.Write(content);

            _cache.Set(file.FullName, new { File = file, Content = content });
        }

        public bool IsReusable
        {
            get { return true; }
        }
    }
}
