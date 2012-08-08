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

        public HotGlueRequireHandler()
        {
            _configuration = HotGlueConfigurationSection.Load();
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

            var package = Package.Build(_configuration, root);
            var content = package.CompileStitch();

            context.Response.AddHeader("Content-Length", content.Length.ToString(CultureInfo.InvariantCulture));
            context.Response.Write(content);
        }

        public bool IsReusable
        {
            get { return true; }
        }
    }
}