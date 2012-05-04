using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using HotGlue.Compilers;
using HotGlue.Model;

namespace HotGlue.Web
{
    public class HotGlueHttpHandler : IHttpHandler
    {
        private ICompile[] _compilers;
        private IReference _reference;

        public HotGlueHttpHandler()
        {
            _compilers = new[]
                {
                    new JavaScriptCompiler()
                };
            _reference = new HTMLReference();
        }

        public void ProcessRequest(HttpContext context)
        {
            var package = new Package("", _compilers, _reference);
            // find references
            var file = context.Server.MapPath(context.Request.AppRelativeCurrentExecutionFilePath).Replace(".jsglue",".js");
            var references = new[]
                {
                    new Reference
                        {
                            Root = "",
                            Path = file,
                            Module = false
                        }
                };
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
