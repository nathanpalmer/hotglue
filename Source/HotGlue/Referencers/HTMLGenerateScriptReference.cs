using System.IO;
using HotGlue.Model;

namespace HotGlue
{
    public class HTMLGenerateScriptReference : IGenerateScriptReference
    {
        public string GenerateReference(string root, Reference reference)
        {
            return string.Format("<script src=\"{0}\"></script>", reference.RelativePath(root, true));
        }
    }
}