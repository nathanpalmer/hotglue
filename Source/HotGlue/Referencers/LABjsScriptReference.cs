using System.IO;
using HotGlue.Model;

namespace HotGlue
{
    public class LABjsScriptReference : IGenerateScriptReference
    {
        public string GenerateReference(string root, SystemReference reference)
        {
            var relativePath = reference.RelativePath(root, true);
            var wait = reference.Wait ? ".wait()" : "";

            return reference.Name.EndsWith("-module")
                       ? string.Format(".script(\"{0}&name={2}\"){1}", relativePath, wait, string.Join("&name=", reference.ReferenceNames))
                       : string.Format(".script(\"{0}\"){1}", relativePath, wait);
        }
    }
}