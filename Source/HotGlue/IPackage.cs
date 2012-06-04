using System.Collections.Generic;
using HotGlue.Model;

namespace HotGlue
{
    public interface IPackage
    {
        string Compile(IEnumerable<SystemReference> references);
        string References(IEnumerable<SystemReference> references);
        string CompileDependency(Reference reference);
        string CompileModule(SystemReference reference);
        string CompileStitch();
    }
}