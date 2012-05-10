using System.Globalization;
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
            // find references
            var root = context.Server.MapPath("~");

            var package = Package.Build(_configuration, root);
            var content = package.CompileStitch();

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