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
        private static readonly Regex FileNameRegex = new Regex(@"(?<file>\S+)(?<extension>\.(.+(?=(-module|-glue|-require))|.+))");

        public static SystemReference BuildReference(this HttpContext context, Reference.TypeEnum type)
        {
            var fullPath = context.Server.MapPath(context.Request.AppRelativeCurrentExecutionFilePath);
            var name = Path.GetFileName(fullPath);
            var directory = Path.GetDirectoryName(fullPath);

            var match = FileNameRegex.Match(name);
            name = match.Groups["file"].Value + match.Groups["extension"].Value;

            var reference = new SystemReference(new DirectoryInfo(context.Server.MapPath("~")), new FileInfo(Path.Combine(directory, name)), name);
            var keys = context.Request.QueryString["name"] ?? "";
            foreach(var key in keys.Split(','))
            {
                reference.ReferenceNames.Add(key);
            }
            return reference;
        }
    }
}
