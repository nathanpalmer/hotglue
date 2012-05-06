using System;
using System.Collections.Generic;
using System.Configuration;
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
        private IReferenceLocator _locator;
        private HotGlueConfiguration _configuration;

        public HotGlueHttpHandler()
        {
            _compilers = new[]
                {
                    new JavaScriptCompiler()
                };
            _reference = new HTMLReference();
            _configuration = (HotGlueConfiguration)ConfigurationManager.GetSection("hotglue");
            
        }

        public void ProcessRequest(HttpContext context)
        {
            _configuration = new HotGlueConfiguration { ScriptPath = context.Server.MapPath("~/Scripts/"), ScriptSharedFolder = context.Server.MapPath("~/Scripts/Shared/") };
            _locator = new DynamicLoading(_configuration);
            var package = new Package(_configuration.ScriptPath, _compilers, _reference);
            // find references
            var file = context.Server.MapPath(context.Request.AppRelativeCurrentExecutionFilePath).Replace(".jsglue",".js");
            var root = context.Server.MapPath("~");
            var relative = context.Server.MapPath(".") + "\\";
            file = file.Replace(relative, "");
            var reference = new Reference
                {
                    Root = relative,
                    Path = file,
                    Module = false
                };

            var references = _locator.Load(root, reference);
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
