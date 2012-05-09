using System.Globalization;
using System.Web;
using HotGlue.Compilers;
using HotGlue.Model;

namespace HotGlue.Web
{
    public class HotGlueModuleHandler : IHttpHandler
    {
        private ICompile[] _compilers;
        private IGenerateScriptReference _generateScriptReference;

        public HotGlueModuleHandler()
        {
            _compilers = new[]
                {
                    new JavaScriptCompiler()
                };
            _generateScriptReference = new HTMLGenerateScriptReference();
        }

        public void ProcessRequest(HttpContext context)
        {
            // find references
            var root = context.Server.MapPath("~");
            var reference = context.BuildReference(Reference.TypeEnum.Module);

            var package = new Package(root, _compilers, _generateScriptReference);
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