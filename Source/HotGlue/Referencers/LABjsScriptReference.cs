using System.IO;
using HotGlue.Model;

namespace HotGlue
{
    public class LABjsScriptReference : IGenerateScriptReference
    {
        public string GenerateReference(string root, Reference reference)
        {
            var relativePath = reference.RelativePath(root, true);
            var wait = reference.Wait ? ".wait()" : "";

            return reference.Name.EndsWith("-module")
                       ? string.Format(".script(\"{0}&name={2}\"){1}", relativePath, wait, reference.Name.Replace("-module", ""))
                       : string.Format(".script(\"{0}\"){1}", relativePath, wait);
        }
    }
}