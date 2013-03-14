using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Routing;
using RazorEngine.Templating;

namespace HotGlue.Generator.MVCRoutes
{
    public class JavaScriptRoutingTemplateBase<T> : TemplateBase<T>
    {
        public HttpContextBase httpContext { get; private set; }
        public RouteCollection routeTable { get; private set; }
        
        public JavaScriptRoutingTemplateBase()
        {
            httpContext = new TempHttpContext();
            routeTable = MVCRouteGenerator.routes;
        }

        public IEnumerable<string[]> Variations(int position, string[] values, string[] array)
        {
            for (int i = position; i < array.Length; i++)
            {
                var all = new List<String>();
                if (values != null) all.AddRange(values);
                all.Add(array[i]);

                foreach (var v in Variations(i + 1, all.ToArray(), array))
                {
                    yield return v;
                }
                yield return all.ToArray();
            }
        }

        private class TempHttpContext : HttpContextBase
        {
            private TempHttpRequest request;
            private TempHttpResponse response;

            public override HttpRequestBase Request
            {
                get
                {
                    if (request == null)
                        request = new TempHttpRequest();

                    return request;
                }
            }

            public override HttpResponseBase Response
            {
                get
                {
                    if (response == null)
                        response = new TempHttpResponse();

                    return response;
                }
            }

            private class TempHttpRequest : HttpRequestBase
            {
                public override string ApplicationPath
                {
                    get
                    {
                        return "";
                    }
                }
            }

            private class TempHttpResponse : HttpResponseBase
            {
                public override string ApplyAppPathModifier(string virtualPath)
                {
                    return virtualPath;
                }
            }
        }
    }
}
