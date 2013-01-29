using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using HotGlue;
using HotGlue.Model;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.Responses;

namespace HotGlue.Demos.Nancy
{
    public class HotGlueNancyStartup : IApplicationStartup
    {
        private LoadedConfiguration _configuration;
        private GraphReferenceLocator _locator;
        //TODO: Add in a file cache implementation
        //private IFileCache _cache;
        public static string Root { get; private set; }

        public HotGlueNancyStartup(IRootPathProvider rootPathProvider)
        {
            Root = rootPathProvider.GetRootPath();
        }

        public void Initialize(IPipelines pipelines)
        {
            var config = HotGlueConfiguration.Load();
            _configuration = LoadedConfiguration.Load(config);
            _locator = new GraphReferenceLocator(_configuration);

            pipelines.AfterRequest.AddItemToEndOfPipeline(RewriteContents);
        }

        private void RewriteContents(NancyContext context)
        {
            if (!context.Request.Path.StartsWith("/hotglue.axd/")) return;

            var fullPath = Path.Combine(Root, context.Request.Path.Replace("/hotglue.axd/", ""));

            ScriptHelper.RewriteContent(
                _configuration,
                _locator,
                //TODO: Replace with _cache when we have an implementation
                null,
                Root,
                fullPath, 
                (key) =>
                    {
                        var value = context.Request.Query[key];
                        return value.HasValue ? value.Value : null;
                    },
                (path, contentType) =>
                    {
                        context.Response = new GenericFileResponse(path, contentType);
                    },
                (content, contentType) =>
                    {
                        context.Response.Headers.Add("Content-Length", content.Length.ToString(CultureInfo.InvariantCulture));
                        context.Response.Contents = stream =>
                        {
                            StreamWriter sw = new StreamWriter(stream);
                            sw.Write(content);
                            sw.Close();
                        };
                        context.Response.ContentType = contentType;
                        context.Response.StatusCode = HttpStatusCode.OK;
                    });
        }
    }
}