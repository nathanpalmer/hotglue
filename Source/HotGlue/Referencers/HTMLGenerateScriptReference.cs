using System.IO;
using HotGlue.Model;

namespace HotGlue
{
    public class HTMLGenerateScriptReference : IGenerateScriptReference
    {
        public string GenerateReference(string root, SystemReference reference)
        {
            var relativePath = reference.RelativePath(root, true);

            return reference.Name.EndsWith("-module")
                       ? string.Format("<script src=\"{0}&name={1}\"></script>", relativePath, string.Join("&name=", reference.ReferenceNames))
                       : string.Format("<script src=\"{0}\"></script>", relativePath);
        }
    }
}