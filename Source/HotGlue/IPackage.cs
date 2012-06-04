using System.Collections.Generic;
using HotGlue.Model;

namespace HotGlue
{
    public interface IPackage
    {
        string Compile(IEnumerable<SystemReference> references);
        string References(IEnumerable<SystemReference> references);
        string CompileDependency<T>(T reference) where T : Reference;
        string CompileModule(SystemReference reference);
        string CompileStitch();
    }
}