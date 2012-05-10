using System.Globalization;
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
            // find references
            var root = context.Server.MapPath("~");
            var reference = context.BuildReference(Reference.TypeEnum.Module);

            var package = Package.Build(_configuration, root);
            var content = package.CompileModule(reference);

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