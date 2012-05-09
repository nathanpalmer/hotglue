using System.Globalization;
using System.Web;
using HotGlue.Compilers;

namespace HotGlue.Web
{
    public class HotGlueStitchHandler : IHttpHandler
    {
        private ICompile[] _compilers;
        private IGenerateScriptReference _generateScriptReference;

        public HotGlueStitchHandler()
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

            var package = new Package(root, _compilers, _generateScriptReference);
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