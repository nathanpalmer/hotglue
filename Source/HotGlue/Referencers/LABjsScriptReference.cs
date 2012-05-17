using System.IO;
using HotGlue.Model;

namespace HotGlue
{
    public class LABjsScriptReference : IGenerateScriptReference
    {
        public string GenerateReference(string root, Reference reference)
        {
            return string.Format(".script(\"{0}\"){1}", reference.RelativePath(root, true), reference.Wait ? ".wait()" : "");
        }
    }
}