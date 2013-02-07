using System.Globalization;
using System.IO;
using HotGlue.Model;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.Responses;

namespace HotGlue.Nancy
{
    public class HotGlueNancyStartup : IApplicationStartup
    {
        public static string Root { get; private set; }

        private LoadedConfiguration _configuration;
        private GraphReferenceLocator _locator;
        private bool _debug;
        private readonly IFileCache _cache;

        public HotGlueNancyStartup(IRootPathProvider rootPathProvider)
        {
            Root = rootPathProvider.GetRootPath();
            _cache = new DictionaryCache();
        }

        public void Initialize(IPipelines pipelines)
        {
            _debug = StaticConfiguration.IsRunningDebug;
            var config = HotGlueConfiguration.Load(_debug);
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
                _cache,
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
                            using (var sw = new StreamWriter(stream))
                            {
                                sw.Write(content);
                                sw.Close();
                            }
                        };
                        context.Response.ContentType = contentType;
                        context.Response.StatusCode = HttpStatusCode.OK;
                    });
        }
    }
}