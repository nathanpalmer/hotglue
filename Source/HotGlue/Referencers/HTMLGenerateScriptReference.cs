using System.IO;
using HotGlue.Model;

namespace HotGlue
{
    public class HTMLGenerateScriptReference : IGenerateScriptReference
    {
        public string GenerateReference(Reference reference)
        {
            return string.Format("<script src=\"{0}\"></script>", Path.Combine(reference.Path,reference.Name));
        }
    }
}