using System.Globalization;
using System.IO;
using System.Web;
using HotGlue.Compilers;
using HotGlue.Model;

namespace HotGlue.Web
{
    public class HotGlueRequireHandler : IHttpHandler
    {
        private HotGlueConfiguration _configuration;
        private IFileCache _cache;

        public HotGlueRequireHandler()
        {
            _configuration = HotGlueConfigurationSection.Load();
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

            dynamic cached = _cache.Get("get.js-require");
            if (cached != null)
            {
                context.Response.AddHeader("Content-Length", cached.Content.Length.ToString(CultureInfo.InvariantCulture));
                context.Response.Write(cached.Content);
                return;
            }

            // We should pretty much never hit this after the first request
            var package = Package.Build(_configuration, root);
            var content = package.CompileStitch();

            context.Response.AddHeader("Content-Length", content.Length.ToString(CultureInfo.InvariantCulture));
            context.Response.Write(content);

            _cache.Set("get.js-require", new { Content = content });
        }

        public bool IsReusable
        {
            get { return true; }
        }
    }
}