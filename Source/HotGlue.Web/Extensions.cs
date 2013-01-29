using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using HotGlue.Model;

namespace HotGlue
{
    public static class Extensions
    {
        public static string FullPath(this HttpContext context)
        {
            string fullPath = context.Request.Path.Contains("hotglue.axd")
                      ? context.Server.MapPath(("~" + context.Request.Path).Replace(context.Request.AppRelativeCurrentExecutionFilePath, "~"))
                      : context.Server.MapPath(context.Request.AppRelativeCurrentExecutionFilePath);

            return fullPath;
        }
    }
}
