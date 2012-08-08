using System.Globalization;
using System.IO;
using System.Web;
using HotGlue.Compilers;
using HotGlue.Model;

namespace HotGlue.Web
{
    public class HotGlueModuleHandler : IHttpHandler
    {
        private HotGlueConfiguration _configuration;

        public HotGlueModuleHandler()
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
            var reference = context.BuildReference(Reference.TypeEnum.Module);

            var package = Package.Build(_configuration, root);
            var content = package.CompileModule(reference);

            context.Response.AddHeader("Content-Length", content.Length.ToString(CultureInfo.InvariantCulture));
            context.Response.Write(content);
        }

        public bool IsReusable
        {
            get { return true; }
        }
    }
}