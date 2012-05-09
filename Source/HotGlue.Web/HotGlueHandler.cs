using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using HotGlue.Compilers;
using HotGlue.Model;

namespace HotGlue.Web
{
    public class HotGlueHandler : IHttpHandler
    {
        private ICompile[] _compilers;
        private IGenerateScriptReference _generateScriptReference;
        private IReferenceLocator _locator;
        private HotGlueConfiguration _configuration;

        public HotGlueHandler()
        {
            _compilers = new[]
                {
                    new JavaScriptCompiler()
                };
            _generateScriptReference = new HTMLGenerateScriptReference();
            _configuration = HotGlueConfigurationSection.Load();
            var findReferences = new List<IFindReference>() {new SlashSlashEqualReference(), new RequireReference() };
            _locator = new GraphReferenceLocator(_configuration, findReferences);
        }

        public void ProcessRequest(HttpContext context)
        {
            // find references
            var root = context.Server.MapPath("~");
            var reference = context.BuildReference(Reference.TypeEnum.App);

            var package = new Package(root, _compilers, _generateScriptReference);
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
