using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace HotGlue.Aspnet
{
    public class HttpContextCache : IFileCache
    {
        public dynamic Get(string fullName)
        {
            return HttpContext.Current.Application[fullName];
        }

        public void Set(string fullName, dynamic o)
        {
            HttpContext.Current.Application[fullName] = o;
        }
    }
}
