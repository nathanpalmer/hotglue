using System.Collections.Generic;
using HotGlue.Model;

namespace HotGlue
{
    public interface IPackage
    {
        string Compile(IEnumerable<SystemReference> references);
        string GenerateReferences(IEnumerable<SystemReference> references, HelperOptions options);
        IEnumerable<SystemReference> References(IEnumerable<SystemReference> references);
        string CompileDependency<T>(T reference) where T : Reference;
        string CompileModule(SystemReference reference);
        string CompileStitch();
    }
}