using System.IO;
using HotGlue.Model;

namespace HotGlue
{
    public class LABjsScriptReference : IGenerateScriptReference
    {
        public string GenerateReference(Reference reference)
        {
            return string.Format(".script(\"{0}\"){1}", Path.Combine(reference.Path, reference.Name).Replace("\\", "/"), reference.Wait ? ".wait()" : "");
            //return string.Format(".script(\"{0}\")", Path.Combine(reference.Path, reference.Name).Replace("\\", "/"));
        }
    }
}