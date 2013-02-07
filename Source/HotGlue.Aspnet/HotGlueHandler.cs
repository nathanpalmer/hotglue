using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Configuration;
using HotGlue.Model;

namespace HotGlue.Aspnet
{
    public class HotGlueHandler : IHttpHandler
    {
        private static LoadedConfiguration _configuration;
        private static IReferenceLocator _locator;
        private static HttpContextCache _cache;

        static HotGlueHandler()
        {
            var debug = ((CompilationSection)ConfigurationManager.GetSection(@"system.web/compilation")).Debug;
            var config = HotGlueConfiguration.Load(debug);
            _configuration = LoadedConfiguration.Load(config);
            _locator = new GraphReferenceLocator(_configuration);
            _cache = new HttpContextCache();
        }

        public void ProcessRequest(HttpContext context)
        {
            var root = context.Server.MapPath("~");
            var fullPath = context.FullPath();

            ScriptHelper.RewriteContent(
                _configuration,
                _locator,
                _cache,
                root,
                fullPath,
                (key) =>
                    {
                        var value = context.Request.QueryString[key];
                        return value ?? "";
                    },
                (path, contentType) =>
                    {
                        context.Response.ContentType = contentType;
                        context.Response.TransmitFile(fullPath);
                    },
                (content, contentType) =>
                    {
                        context.Response.ContentType = contentType;
                        context.Response.AddHeader("Content-Length", content.Length.ToString(CultureInfo.InvariantCulture));
                        context.Response.Write(content);
                    });
        }

        public bool IsReusable
        {
            get { return true; }
        }
    }
}
