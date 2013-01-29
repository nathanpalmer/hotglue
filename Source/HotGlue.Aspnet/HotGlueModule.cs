using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Configuration;

namespace HotGlue.Aspnet
{
    public class HotGlueModule : IHttpModule
    {
        private static readonly Regex FilePath = new Regex(@"^~/hotglue\.axd(?<path>[^?]*)(\?(?<queryString>.*))?$");
        private const string HttpHandler = "~/hotglue.axd";

        public void Init(HttpApplication context)
        {
            if (Process.GetCurrentProcess().ProcessName.StartsWith("WebDev.WebServer"))
            {
                context.PostAuthorizeRequest += (s, e) =>
                    {
                        var path = HttpContext.Current.Request.AppRelativeCurrentExecutionFilePath;
                        if (path != null && path.StartsWith(HttpHandler+"/"))
                        {
                            var matches = FilePath.Match(path);
                            var relativePath = matches.Groups["path"].Value;
                            var queryString = matches.Groups["queryString"].Value;
                            HttpContext.Current.RewritePath(HttpHandler, relativePath, queryString);
                        }
                    };
            }
        }

        public void Dispose()
        {
            // Nothing to do here
        }
    }
}
